using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotMcpClient.Services.Interfaces;

public  interface ISemanticKernelService
{
    Task InitializeKernelAndPluginAsync();
    /// <summary>
    /// Asynchronously generates a response based on the specified prompt.
    /// </summary>
    /// <param name="prompt">The input prompt to process. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="KernelPluginResult"/>
    /// with the generated response.</returns>
    Task<KernelPluginResult> GetResponseAsync(string prompt, CancellationToken cancellationToken); // TODO: Consider adding CancellationToken parameter for better async handling
}

public class KernelPluginResult
{
    public bool IsSuccess { get; set; } = true;
    public string Result { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public int RequestTokens { get; set; }

    public bool WasCancelled { get; set; } = false;

    /// <summary>
    /// Pipeline TPS (Tokens Per Second)
    /// Note: This is different from LLM TPS
    /// </summary>
    public double PipelineTokensPerSecond { get; set; }
    public long GenerationMilliseconds { get; set; }

}
