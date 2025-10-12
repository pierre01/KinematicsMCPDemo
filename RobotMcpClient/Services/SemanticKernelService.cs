using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;
using RobotMcpClient.Services.Interfaces;
using System.Diagnostics;

namespace RobotMcpClient.Services;

public class SemanticKernelService : ISemanticKernelService
{
    // ===== OpenAI chat model config =====
    private const string chatModel = "gpt-5-mini";// gpt-5-nano

    // ===== MCP transport config (override via env vars) =====
    // MCP_WS_URL: ws://localhost:5059/mcp (when WS)
    private static readonly string McpWsUrl = Environment.GetEnvironmentVariable("MCP_WS_URL") ?? "https://localhost:6805/mcp/";  //"ws://localhost:6805/mcp/"

    private ChatHistory? _history;
    private IKernelBuilder? _builder;
    private Kernel? _kernel;
    private IChatCompletionService? _chatCompletionService;
    private OpenAIPromptExecutionSettings? _openAIPromptExecutionSettings;

#pragma warning disable SKEXP0001
    private IChatHistoryReducer _reducer;
#pragma warning restore SKEXP0001

    /// <summary>
    /// Initialize SK, OpenAI, and attach MCP tools from Lights.McpServer.
    /// </summary>
    public async Task InitializeKernelAndPluginAsync()
    {
        try
        {        
            _history = [];
            //Wait 10 seconds before initializing the kernel to allow time for the MCP server to start
            await Task.Delay(15000);

#pragma warning disable SKEXP0001
            _reducer = new ChatHistoryTruncationReducer(targetCount: 4, thresholdCount: 6);
#pragma warning restore SKEXP0001

            var openAiApiKey = await ApiKeyProvider.GetApiKeyAsync();
            var openApiOrgId = await ApiKeyProvider.GetAiOrgId();
            if (string.IsNullOrWhiteSpace(openAiApiKey))
                throw new InvalidOperationException("API key is not set.");

            _builder = Kernel.CreateBuilder();

            // OpenAI chat connector
            _builder.Services.AddOpenAIChatCompletion(
                modelId: chatModel,
                apiKey: openAiApiKey,
                orgId: openApiOrgId,
                serviceId: "Kinematics"
            );

            // Let the model auto-invoke MCP tools when helpful
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _openAIPromptExecutionSettings = new()
            {
                //ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }),
                Temperature = 1,
            };
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

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

            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing kernel: {ex.Message}");
            throw;
        }
    }

    private int _lastTotalTokens = 0;
    private int _totalTokens = 0;

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
    public async Task<KernelPluginResult> GetResponseAsync(string prompt)
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

            ChatMessageContent result = await _chatCompletionService.GetChatMessageContentAsync(
                _history,
                executionSettings: _openAIPromptExecutionSettings,
                kernel: _kernel);

            response.Result = result.ToString();

            // Token accounting (OpenAI connector metadata)
            if (result.Metadata != null && result.Metadata.TryGetValue("Usage", out var usageObj) && usageObj is OpenAI.Chat.ChatTokenUsage usage)
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
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        try
        {
            Debug.WriteLine($"Function {context.Function.Name} is about to be invoked.");
            await next(context);
            Debug.WriteLine($"Function {context.Function.Name} was invoked.");
        }
        catch (Exception ex)
        {
            // Log the exception for diagnostics, but do not rethrow
            Debug.WriteLine($"Exception during function invocation: {ex}");
            // Optionally, you could add more sophisticated logging here
        }
    }

}
