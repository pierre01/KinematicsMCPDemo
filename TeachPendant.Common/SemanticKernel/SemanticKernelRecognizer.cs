using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Biosero.TeachPendant.Common.SemanticKernel;

/// <summary>
/// Singleton class to handle the semantic kernel
/// </summary>
public partial class SemanticKernelRecognizer : ObservableObject
{
    private static SemanticKernelRecognizer _instance = null;
    private static readonly object padlock = new();

    public static SemanticKernelRecognizer Instance
    {
        get
        {
            lock (padlock)
            {
                return _instance ??= new SemanticKernelRecognizer();
            }
        }
    }

    private string _newUserInput;
    public bool IsNewMessageAvailable { get; private set; }

    [ObservableProperty]
    private string _aiResponse;

    [ObservableProperty]
    private string _pluginResponse;

    private SemanticKernelRecognizer()
    {
        IsWaiting = true;
        _newUserInput = string.Empty;
        IsNewMessageAvailable = false;
        StartSemanticInterpreter();
    }

    /// <summary>
    /// Set to false to break the loop
    /// </summary>
    public bool IsWaiting { get; set; } = true;

    /// <summary>
    /// Give the new text to be interpreted
    /// </summary>
    /// <param name="userInput"></param>
    public void SetNewUserInput(string userInput)
    {
        _newUserInput = userInput;
        IsNewMessageAvailable = true;
    }

    public async Task StartSemanticInterpreter()
    {

        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion("gpt-4-0125-preview", "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", "org-RRBnXYYjTq5b4qr7TLaaHsLD");

        //builder.AddAzureOpenAIChatCompletion(
        //         "gpt-35-turbo",                      // Azure OpenAI Deployment Name
        //         "https://delangeopenai.openai.azure.com/", // Azure OpenAI Endpoint
        //         "6d9b78b36713455ab0657b9332e9f288");      // Azure OpenAI Key

        //builder.AddAzureOpenAIChatCompletion(
        //         "mobile-teach-pendant-gpt-4",                              // Azure OpenAI Deployment Name
        //         "https://openai-2024q1-hackathon.openai.azure.com/",       // Azure OpenAI Endpoint
        //         "98fc40ab1e184f399c2a1d1503a72146");                       // Azure OpenAI Key

        builder.Plugins.AddFromType<RobotControlPlugin>();
        //builder.Plugins.AddFromType<LightPlugin>();

        var kernel = builder.Build();
        //kernel.ImportPluginFromType<RobotControlPlugin>();

        // Create chat history
        var history = new ChatHistory();

        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Start the conversation
        //Console.Write("User > ");
        //string? userInput;
        //while ((userInput = Console.ReadLine()) != null)
        while (IsWaiting || IsNewMessageAvailable)
        {
            if (IsNewMessageAvailable)
            {
                IsNewMessageAvailable = false;
                var userInput = GetNewUserInput();
                // Add user input
                history.AddUserMessage(userInput);

                // Enable auto function calling
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                // Get the response from the AI
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

                // Print the results
                if (result != null)
                {
                    /// Set property with AI result
                    AiResponse = result.Content;
                }
                //Console.WriteLine("Assistant > " + result);

                // Add the message from the agent to the chat history
                history.AddMessage(result.Role, result.Content ?? string.Empty);
            }
            if (IsWaiting)
            {
                // Wait for a while
                // replace line below with awaitable Task.Delay
                await Task.Delay(100);
                continue;
            }
        }
    }

    private string GetNewUserInput()
    {
        IsNewMessageAvailable = false;
        var newUserInput = _newUserInput;
        _newUserInput = string.Empty;
        return newUserInput;
    }
}
