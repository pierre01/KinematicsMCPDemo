using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using RobotMcpClient.Services.Interfaces;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Net.Security;
using System.Text;
using System.Text.Json.Nodes;

namespace RobotMcpClient.Services;

/// <summary>Runs the robot-control agent behind the service contract used by the MAUI UI.</summary>
public sealed class SemanticKernelService : ISemanticKernelService, IAsyncDisposable
{
    private const int MaxResponseTokens = 256;
    private const int HistoryTargetCount = 12;
    private const string RemoteModel = "gpt-5-mini";
    private const string LocalModel = "qwen/qwen3.6-35b-a3b";
    private const string Instructions =
        "Control the robot by calling the matching tool immediately. " +
        "Use millimeters, treat movement commands as relative deltas, and trust the tool result as the new absolute position. " +
        "For left movement always call MoveLeft with a positive distance. For right movement always call MoveRight with a positive distance. " +
        "Do not use the signed lateral parameter of MoveBy for left or right requests. " +
        "Execute every requested step exactly once. Multi-step instructions may use multiple tool calls in their stated order. " +
        "Do not immediately repeat an identical tool call unless the user explicitly requested consecutive repetition. " +
        "When the user says 'again', repeat the most recent requested action exactly once with the same arguments. " +
        "Do not reconstruct or debate prior coordinates. Keep the final response to one short sentence.";

    private static readonly string McpUrl = NormalizeMcpUrl(
        Environment.GetEnvironmentVariable("MCP_URL") ??
        Environment.GetEnvironmentVariable("MCP_WS_URL") ??
        "https://localhost:6805/mcp");

    private AIAgent? _agent;
    private AgentSession? _session;
    private McpClient? _mcpClient;
    private int _totalTokens;
    private long _toolTimeMs;
    private int _toolCallCount;
    private string? _previousToolCall;

    public async Task InitializeKernelAndPluginAsync()
    {
        await DisposeAgentResourcesAsync().ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

        _mcpClient = await McpClient.CreateAsync(
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Name = "Kinematics.McpServer",
                    Endpoint = new Uri(McpUrl),
                    TransportMode = HttpTransportMode.StreamableHttp,
                },
                CreateLocalhostHttpClient(),
                loggerFactory: null,
                ownsHttpClient: true)).ConfigureAwait(false);

        var mcpTools = await _mcpClient.ListToolsAsync().ConfigureAwait(false);
        IChatClient chatClient = await CreateChatClientAsync().ConfigureAwait(false);
        var baseAgent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "RobotController",
            ChatOptions = new ChatOptions
            {
                Instructions = Instructions,
                MaxOutputTokens = MaxResponseTokens,
                ToolMode = ChatToolMode.RequireAny,
                Tools = [.. mcpTools.Cast<AITool>()],
            },
#pragma warning disable MEAI001 // The framework's built-in count reducer is the direct replacement for SK truncation.
            ChatHistoryProvider = new InMemoryChatHistoryProvider(
                new InMemoryChatHistoryProviderOptions
                {
                    ChatReducer = new MessageCountingChatReducer(HistoryTargetCount),
                }),
#pragma warning restore MEAI001
        });

        _agent = baseAgent.AsBuilder().Use(TraceAndGuardToolCallAsync).Build();
        _session = await _agent.CreateSessionAsync().ConfigureAwait(false);
    }

    public async Task<KernelPluginResult> GetResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new KernelPluginResult { IsSuccess = false, Result = "Please enter a prompt" };
        }

        if (_agent is null || _session is null)
        {
            return new KernelPluginResult { IsSuccess = false, Result = "Agent is not initialized." };
        }

        try
        {
            _previousToolCall = null;
            _toolTimeMs = 0;
            _toolCallCount = 0;
            var stopwatch = Stopwatch.StartNew();
            AgentResponse result = await _agent.RunAsync(
                prompt,
                _session,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var inputTokens = ToInt32(result.Usage?.InputTokenCount);
            var outputTokens = ToInt32(result.Usage?.OutputTokenCount);
            var requestTokens = ToInt32(result.Usage?.TotalTokenCount) is var firstTotal && firstTotal > 0
                ? firstTotal
                : inputTokens + outputTokens;

            if (_toolCallCount == 0)
            {
                // Some local models occasionally ignore tool_choice=required and emit a
                // narrative answer. Retry once in a clean session so that narrative is
                // neither trusted nor retained in subsequent conversation history.
                _session = await _agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false);
                result = await _agent.RunAsync(
                    $"Invoke the appropriate robot function now. Do not answer with text before calling it. User request: {prompt}",
                    _session,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                inputTokens += ToInt32(result.Usage?.InputTokenCount);
                outputTokens += ToInt32(result.Usage?.OutputTokenCount);
                requestTokens += ToInt32(result.Usage?.TotalTokenCount) is var retryTotal && retryTotal > 0
                    ? retryTotal
                    : ToInt32(result.Usage?.InputTokenCount) + ToInt32(result.Usage?.OutputTokenCount);

                if (_toolCallCount == 0)
                {
                    _session = await _agent.CreateSessionAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            stopwatch.Stop();

            _totalTokens += requestTokens;
            var modelTimeMs = Math.Max(1, stopwatch.ElapsedMilliseconds - _toolTimeMs);

            return new KernelPluginResult
            {
                IsSuccess = _toolCallCount > 0,
                Result = _toolCallCount > 0
                    ? result.Text
                    : "No robot function was called, so no movement was performed.",
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                RequestTokens = requestTokens,
                TotalTokens = _totalTokens,
                GenerationMilliseconds = modelTimeMs,
                PipelineTokensPerSecond = requestTokens > 0 ? requestTokens / (modelTimeMs / 1000d) : 0,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new KernelPluginResult
            {
                IsSuccess = false,
                WasCancelled = true,
                Result = "Request cancelled.",
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting response: {ex}");
            return new KernelPluginResult
            {
                IsSuccess = false,
                Result = $"Error getting response: {ex.Message}",
            };
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAgentResourcesAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask<object?> TraceAndGuardToolCallAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        var signature = $"{context.Function.Name}|{string.Join("|", context.Arguments.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"))}";
        if (string.Equals(_previousToolCall, signature, StringComparison.Ordinal))
        {
            Debug.WriteLine($"Blocked duplicate tool call {context.Function.Name}.");
            return "Skipped duplicate: this exact tool call was already executed immediately before this call.";
        }

        _previousToolCall = signature;
        _toolCallCount++;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Debug.WriteLine($"Function {context.Function.Name} is about to be invoked.");
            return await next(context, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            _toolTimeMs += stopwatch.ElapsedMilliseconds;
            Debug.WriteLine($"Function {context.Function.Name} completed.");
        }
    }

    private static async Task<IChatClient> CreateChatClientAsync()
    {
        var useLocal = true;
        if (!useLocal)
        {
            var apiKey = await ApiKeyProvider.GetApiKeyAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("API key is not set.");
            }

            return new OpenAIClient(apiKey).GetChatClient(RemoteModel).AsIChatClient();
        }

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("http://127.0.0.1:8931/v1"),
            Transport = new HttpClientPipelineTransport(
                CreateLocalhostHttpClient(new QwenReasoningNoneHandler())),
        };
        return new OpenAIClient(new ApiKeyCredential("local-key"), options)
            .GetChatClient(LocalModel)
            .AsIChatClient();
    }

    private static HttpClient CreateLocalhostHttpClient(DelegatingHandler? outerHandler = null)
    {
        var innerHandler = new HttpClientHandler
        {
            CheckCertificateRevocationList = false,
            ServerCertificateCustomValidationCallback = (request, certificate, _, errors) =>
                (request.RequestUri is not null &&
                 request.RequestUri.IsLoopback &&
                 certificate?.Subject.Contains("CN=localhost", StringComparison.OrdinalIgnoreCase) == true) ||
                errors == SslPolicyErrors.None,
        };

        if (outerHandler is null)
        {
            return new HttpClient(innerHandler);
        }

        outerHandler.InnerHandler = innerHandler;
        return new HttpClient(outerHandler);
    }

    private async ValueTask DisposeAgentResourcesAsync()
    {
        if (_mcpClient is not null)
        {
            await _mcpClient.DisposeAsync().ConfigureAwait(false);
            _mcpClient = null;
        }

        _agent = null;
        _session = null;
    }

    private static string NormalizeMcpUrl(string url) =>
        url.EndsWith("/sse", StringComparison.OrdinalIgnoreCase) ? url[..^4] : url;

    private static int ToInt32(long? value) =>
        (int)Math.Clamp(value ?? 0, 0, int.MaxValue);
}

/// <summary>Adds the vLLM-compatible reasoning setting to local requests.</summary>
public sealed class QwenReasoningNoneHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null &&
            request.RequestUri?.AbsolutePath.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase) == true)
        {
            var json = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (JsonNode.Parse(json) is JsonObject body)
            {
                // LM Studio maps the OpenAI-compatible "none" value to Qwen's
                // model-specific "off" setting. Qwen 3.6 supports on/off rather
                // than the low/medium/xhigh levels exposed by newer models.
                body["reasoning_effort"] = "none";
                request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
