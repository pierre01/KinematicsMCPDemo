using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;
using RobotMcpClient.Services.Interfaces;
using System.Diagnostics;
using System.Net.Security;
using System.Text;
using System.Text.Json.Nodes;

namespace RobotMcpClient.Services;

public class SemanticKernelService : ISemanticKernelService
{
    private const int MaxResponseTokens = 1024;
    private const int HistoryTargetCount = 12;
    private const int HistoryThresholdCount = 20;

    // ===== OpenAI chat model config =====
    private const string chatModel = "gpt-5-mini";// gpt-5-nano

    // ===== MCP transport config (override via env vars) =====
    // MCP_WS_URL: ws://localhost:5059/mcp (when WS)
    private static readonly string McpWsUrl = Environment.GetEnvironmentVariable("MCP_WS_URL") ?? "https://localhost:6805/mcp/sse";

    private ChatHistory? _history;
    private IKernelBuilder? _builder;
    private Kernel? _kernel;
    private IChatCompletionService? _chatCompletionService;
    private OpenAIPromptExecutionSettings? _openAIPromptExecutionSettings;

    private IChatHistoryReducer? _reducer;

    private int _lastTotalTokens = 0;
    private int _totalTokens = 0;

    /// <summary>
    /// Initialize SK, OpenAI, and attach MCP tools from Lights.McpServer.
    /// </summary>
    public async Task InitializeKernelAndPluginAsync()
    {
        try
        {        
            _history =
            [
                new ChatMessageContent(
                    AuthorRole.System,
                    "Control the robot by calling the matching tool immediately. " +
                    "Use millimeters, treat movement commands as relative deltas, and trust the tool result as the new absolute position. " +
                    "For left movement always call MoveLeft with a positive distance. For right movement always call MoveRight with a positive distance. " +
                    "Do not use the signed lateral parameter of MoveBy for left or right requests. " +
                    "Execute every requested step exactly once. Multi-step instructions may use multiple tool calls in their stated order. " +
                    "Do not immediately repeat an identical tool call unless the user explicitly requested consecutive repetition. " +
                    "When the user says 'again', repeat the most recent requested action exactly once with the same arguments. " +
                    "Do not reconstruct or debate prior coordinates. Keep the final response to one short sentence.")
            ];
            //Wait 10 seconds before initializing the kernel to allow time for the MCP server to start
            await Task.Delay(10000);

            // If you keep cloud as an option, set useLocal = true/false to toggle
            var useLocal = true;

            _reducer = new ChatHistoryTruncationReducer(
                targetCount: HistoryTargetCount,
                thresholdCount: HistoryThresholdCount);


            _builder = Kernel.CreateBuilder();
            string serviceID = "LocalGPT";
            if (!useLocal)
            {
                serviceID = "RemoteGPT"; //Use OpenAI API
                var openAiApiKey = await ApiKeyProvider.GetApiKeyAsync();
                var openApiOrgId = await ApiKeyProvider.GetAiOrgId();
                if (string.IsNullOrWhiteSpace(openAiApiKey))
                    throw new InvalidOperationException("API key is not set.");

                _builder.AddOpenAIChatCompletion(
                    apiKey: openAiApiKey,
                    modelId: chatModel,
                    orgId: openApiOrgId,
                    serviceId:serviceID
                );
            }
            else
            {
                serviceID = "LocalGPT"; 
                // Build a handler that skips CRL/OCSP (revocation) for localhost only.
                var handler = new HttpClientHandler
                {
                    CheckCertificateRevocationList = false,
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) =>
                    {
                        // Allow only our localhost certs; still fail anything else
                        if (cert?.Subject?.Contains("CN=localhost", StringComparison.OrdinalIgnoreCase) == true)
                            return true;

                        return errors == SslPolicyErrors.None;
                    }
                };

                var httpsClient = new HttpClient(new QwenReasoningNoneHandler(handler))
                {
                    BaseAddress = new Uri("http://127.0.0.1:8931/v1")
                };

                // Register the local vLLM endpoint with Semantic Kernel
                _builder.AddOpenAIChatCompletion(
                    apiKey: "local-key",
                    modelId: "qwen/qwen3.6-35b-a3b",            // must match --served-model-name"openai/gpt-oss-20b"
                    orgId: null,
                    serviceId: serviceID,
                    httpClient: httpsClient
                );


            }


            // ===== Prompt execution settings =====
            // Optimized for robot control with tool use
            _openAIPromptExecutionSettings = new()
            {
                // This is the key line – lets the model pick functions
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,

                // Tool selection and a short confirmation should fit comfortably
                // while preventing a local reasoning model from consuming the
                // entire context budget on a simple robot command.
                MaxTokens = MaxResponseTokens
            };

            _kernel = _builder.Build();

            ////////////////////////////////////////////
            // =====    Attach MCP tools          =====

            // Default: start the MCP server locally via SSE and bind its tools
            // Connect to a running http  server 
            await _kernel.Plugins.AddMcpFunctionsFromSseServerAsync(
                serverName: "Kinematics.McpServer",
                    endpoint: McpWsUrl);

            // Optional: inspect/trace tool invocations
            _kernel.FunctionInvocationFilters.Add(new FunctionInvocationFilter());
            _kernel.AutoFunctionInvocationFilters.Add(new DuplicateToolCallFilter());

            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(serviceID);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing kernel: {ex.Message}");
            throw;
        }
    }


    public async Task HomeRobot()
    {
        if (_kernel is null)
        {
            return;
        }

        foreach (var plugin in _kernel.Plugins)
        {
            Debug.WriteLine($"Plugin: {plugin.Name}");
            foreach (var func in plugin)
            {
                var meta = func.Metadata;

                Debug.WriteLine($"Function: {meta.Name}");
                Debug.WriteLine($"  Description: {meta.Description}");

                foreach (var param in meta.Parameters)
                {
                    Debug.WriteLine($"  Parameter: {param.Name}");
                    Debug.WriteLine($"    Type: {param.ParameterType}");
                    Debug.WriteLine($"    Description: {param.Description}");
                    Debug.WriteLine($"    Required: {param.IsRequired}");
                    Debug.WriteLine($"    Default: {param.DefaultValue}");
                }
            }
        }

        var homeFn = _kernel.Plugins["Kinematics.McpServer"]["HomeRobotArm"];
        var result = await _kernel.InvokeAsync(homeFn);//, new() { ["railChange"] = 25, ["xChange"] = 10 });
    }

    /// <summary>
    /// Chat with tool use (MCP functions auto-invoked when needed).
    /// </summary>
    public async Task<KernelPluginResult> GetResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        var response = new KernelPluginResult();
        try
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                response.IsSuccess = false;
                response.Result = "Please enter a prompt";
                return response;
            }

            if (_history == null)
            {
                response.IsSuccess = false;
                response.Result = "Chat history is not initialized.";
                return response;
            }

            _history.AddUserMessage(prompt);

            if (_reducer is not null)
            {
                var reduced = await _reducer.ReduceAsync(_history, cancellationToken);
                if (reduced is not null)
                {
                    _history = new ChatHistory(reduced);
                }
            }

            if (_chatCompletionService is null)
            {
                response.IsSuccess = false;
                response.Result = "ChatCompletionService is not initialized.";
                return response;
            }

            var stopwatch = Stopwatch.StartNew();

            ChatMessageContent result = await _chatCompletionService.GetChatMessageContentAsync(
                _history,
                executionSettings: _openAIPromptExecutionSettings,
                kernel: _kernel,
                cancellationToken: cancellationToken);

            // Auto function invocation adds its intermediate tool-call/result
            // messages, but the returned final assistant message must be kept
            // explicitly so follow-ups such as "again" see a completed turn.
            _history.Add(result);

            stopwatch.Stop();

            var toolTimeMs = FunctionInvocationFilter.ConsumeToolTimeMs();
            var llmTimeMs = stopwatch.ElapsedMilliseconds - toolTimeMs;
            if (llmTimeMs < 1) llmTimeMs = stopwatch.ElapsedMilliseconds; // fallback


            response.Result = result.ToString();

            // Token accounting (OpenAI connector metadata)
            // Token accounting
            if (result.Metadata != null &&
                result.Metadata.TryGetValue("Usage", out var usageObj) &&
                usageObj is OpenAI.Chat.ChatTokenUsage usage)
            {
                var totalTokens = usage.TotalTokenCount;
                var inputTokens = usage.InputTokenCount - _lastTotalTokens;
                _lastTotalTokens = usage.InputTokenCount;
                var outputTokens = usage.OutputTokenCount;

                _totalTokens += totalTokens;

                response.InputTokens = inputTokens;
                response.OutputTokens = outputTokens;
                response.TotalTokens = _totalTokens;
                response.RequestTokens = totalTokens;

                // ===== Tokens per Second =====
                response.GenerationMilliseconds = llmTimeMs;
                if (outputTokens > 0 && llmTimeMs > 0)
                {
                    response.PipelineTokensPerSecond =
                        (outputTokens + inputTokens) / (llmTimeMs / 1000.0);
                }
            }

            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            response.Result = $"Error getting response: {ex.Message}";
            Debug.WriteLine($"Error getting response: {ex.Message}");
            response.IsSuccess = false;
        }
        return response;
    }


}

/// <summary>
/// Disables reasoning through the OpenAI-compatible API after the connector
/// has validated its request. The API maps "none" to Qwen's internal "off"
/// setting; sending "off" directly is rejected by the API layer.
/// </summary>
public sealed class QwenReasoningNoneHandler(HttpMessageHandler innerHandler)
    : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null &&
            request.RequestUri?.AbsolutePath.EndsWith(
                "/chat/completions",
                StringComparison.OrdinalIgnoreCase) == true)
        {
            var json = await request.Content.ReadAsStringAsync(cancellationToken);
            if (JsonNode.Parse(json) is JsonObject body)
            {
                body["reasoning_effort"] = "none";
                request.Content = new StringContent(
                    body.ToJsonString(),
                    Encoding.UTF8,
                    "application/json");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

/// <summary>
/// Stops a runaway model from immediately repeating the same tool with the same
/// arguments while still permitting an intentional sequence of different steps.
/// </summary>
public sealed class DuplicateToolCallFilter : IAutoFunctionInvocationFilter
{
    private readonly object _sync = new();
    private string? _previousCallSignature;

    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        var signature = CreateSignature(context);
        var isDuplicate = false;

        lock (_sync)
        {
            if (context.RequestSequenceIndex == 0 && context.FunctionSequenceIndex == 0)
            {
                _previousCallSignature = null;
            }

            isDuplicate = string.Equals(
                _previousCallSignature,
                signature,
                StringComparison.Ordinal);

            if (!isDuplicate)
            {
                _previousCallSignature = signature;
            }
        }

        if (!isDuplicate)
        {
            await next(context);
            return;
        }

        Debug.WriteLine(
            $"Blocked duplicate tool call {context.Function.Name} " +
            $"(request {context.RequestSequenceIndex}, function {context.FunctionSequenceIndex}).");

        context.Result = new FunctionResult(
            context.Function,
            "Skipped duplicate: this exact tool call was already executed immediately before this call.");
        context.Terminate = true;
    }

    private static string CreateSignature(AutoFunctionInvocationContext context)
    {
        var arguments = string.Join(
            "|",
            (context.Arguments ?? [])
                .OrderBy(argument => argument.Key, StringComparer.Ordinal)
                .Select(argument => $"{argument.Key}={argument.Value}"));

        return $"{context.Function.PluginName}.{context.Function.Name}|{arguments}";
    }
}

/// <summary>
/// Optional function-invocation tracer
/// </summary>
public sealed class FunctionInvocationFilter : IFunctionInvocationFilter
{
    // Accumulates tool time per async flow
    private static readonly AsyncLocal<long> _toolTimeMs = new();

    // Helper so your service can access and reset it
    public static long ConsumeToolTimeMs()
    {
        var value = _toolTimeMs.Value;
        _toolTimeMs.Value = 0;
        return value;
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            Debug.WriteLine($"Function {context.Function.Name} is about to be invoked.");
            await next(context);
            Debug.WriteLine($"Function {context.Function.Name} was invoked.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during function invocation: {ex}");
        }
        finally
        {
            sw.Stop();
            _toolTimeMs.Value = _toolTimeMs.Value + sw.ElapsedMilliseconds;
        }
    }
}

