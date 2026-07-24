using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;
using RobotMcpClient.Services.Interfaces;
using System.Diagnostics;
using System.Net.Security;

namespace RobotMcpClient.Services;

public class SemanticKernelService : ISemanticKernelService
{
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

    private IChatHistoryReducer _reducer;

    private int _lastTotalTokens = 0;
    private int _totalTokens = 0;

    /// <summary>
    /// Initialize SK, OpenAI, and attach MCP tools from Lights.McpServer.
    /// </summary>
    public async Task InitializeKernelAndPluginAsync()
    {
        try
        {        
            _history = [];
            //Wait 10 seconds before initializing the kernel to allow time for the MCP server to start
            await Task.Delay(10000);

            // If you keep cloud as an option, set useLocal = true/false to toggle
            var useLocal = false;

            _reducer = new ChatHistoryTruncationReducer(targetCount: 40, thresholdCount: 60);


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

                var httpsClient = new HttpClient(handler)
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
                //Temperature = 1,
                //TopP = 0.4,
                //FrequencyPenalty = 0,
                //PresencePenalty = 0, 
                ////ReasoningEffort = "minimal",

                // This is the key line – lets the model pick functions
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,

                // Low MaxTokens does not reduce tool-call reliability.
                // MaxTokens reduces natural-language verbosity — nothing else.
                MaxTokens = 8000
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

            // If you want trimming, uncomment to apply reducer:
            // var reduced = await _reducer.ReduceAsync(_history);
            // if (reduced is not null) _history = new ChatHistory(reduced);

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
                kernel: _kernel);

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

