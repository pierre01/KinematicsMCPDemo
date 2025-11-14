using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Biosero.Kinematics.Common.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KinematicsDemo.Services;

/// <summary>
/// Provides functionality to initialize, start, and stop an MCP (Modular Control Platform) server using HTTP transport.
/// Manages the server's lifecycle and resources, supporting asynchronous startup, shutdown, and disposal operations.
/// </summary>
/// <remarks>McpService is designed to host an MCP server with configurable network settings and secure (HTTPS)
/// support. It implements both IMCPServer and IAsyncDisposable, allowing integration with dependency injection and
/// proper resource cleanup. The service should be initialized before starting, and disposed after stopping to release
/// resources. This class is not thread-safe; concurrent calls to lifecycle methods may result in undefined
/// behavior.</remarks>
public class McpService : IMCPServer, IAsyncDisposable
{
    private WebApplication? _webApp;
    private CancellationTokenSource? _cts;
    private bool _started;

    /// <summary>
    /// Initializes and configures the MCP web service, setting up HTTP or HTTPS endpoints and registering required
    /// services.
    /// </summary>
    /// <remarks>This method should be called once during application startup. Subsequent calls have no effect
    /// if the service is already initialized.</remarks>
    /// <param name="args">An array of command-line arguments used to configure the web application.</param>
    /// <param name="hostAddress">The IP address to bind the web service to. If null or empty, the service listens on all available network
    /// interfaces.</param>
    /// <param name="port">The port number on which the web service will listen. The default is 6805.</param>
    /// <param name="useHttps">Specifies whether the web service should use HTTPS. Set to <see langword="true"/> to enable HTTPS; otherwise,
    /// <see langword="false"/>.</param>
    public void InitializeMcpService(string[] args, string? hostAddress = null, int port = 6805, bool useHttps = true)
    {
        if (_webApp != null)
        {
            return; // already built
        }

        var builder = WebApplication.CreateSlimBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            if (string.IsNullOrWhiteSpace(hostAddress))
            {
                options.ListenAnyIP(port, listen =>
                {
                    if (useHttps)
                    {
                        listen.UseHttps();
                    }
                });
            }
            else
            {
                options.Listen(System.Net.IPAddress.Parse(hostAddress), port, listen =>
                {
                    if (useHttps)
                    {
                        listen.UseHttps();
                    }
                });
            }
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, RobotJsonContext.Default);
        });

        var toolSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = RobotJsonContext.Default,
        };

        builder.Services.AddMcpServer()
               .WithHttpTransport()
               .WithToolsFromAssembly(serializerOptions: toolSerializerOptions);

        _webApp = builder.Build();

        var mcpGroup = _webApp.MapGroup("/mcp");
        mcpGroup.MapMcp();
    }

    /// <summary>
    /// Starts the web application asynchronously using the specified host address, if it has not already been started.
    /// </summary>
    /// <remarks>If the application is already started, this method returns immediately without performing any
    /// action. The cancellation token used during startup is intended only for aborting the startup process, not for
    /// controlling the application's lifetime.</remarks>
    /// <param name="hostAddress">The network address to bind the web application to. This value determines where the application will listen for
    /// incoming requests.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the web application has not been initialized prior to calling this method.</exception>
    public async Task StartAsync(string hostAddress)
    {
        if (_started)
        {
            return;
        }

        InitializeMcpService(Array.Empty<string>(), hostAddress: hostAddress);

        if (_webApp == null)
        {
            throw new InvalidOperationException("Web application is not initialized.");
        }

        _cts = new CancellationTokenSource();
        
        // NOTE: StartAsync’s token is for aborting startup, not lifetime.
        await _webApp.StartAsync(_cts.Token).ConfigureAwait(false);
        _started = true;
    }

    /// <summary>
    /// Asynchronously stops the web application and releases associated resources.
    /// </summary>
    /// <remarks>The method attempts a graceful shutdown of the web application, waiting up to 10 seconds
    /// before forcing cancellation. If the application was not started or is already stopped, the method returns
    /// immediately.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to request the operation to cancel before the shutdown completes. If not
    /// specified, the default token is used.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_started || _webApp == null)
        {
            return;
        }

        try
        {
            _cts?.Cancel(); // signal your own lifetime intent
            // StopAsync token bounds how long to wait for graceful shutdown.
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked.CancelAfter(TimeSpan.FromSeconds(10));

            await _webApp.StopAsync(linked.Token).ConfigureAwait(false);
        }
        finally
        {
            await DisposeAsync().ConfigureAwait(false);
            _started = false;
        }
    }

    /// <summary>
    /// Asynchronously releases resources used by the instance, including any associated web application and
    /// cancellation tokens.
    /// </summary>
    /// <remarks>Call this method to clean up resources when the instance is no longer needed. This method
    /// should be awaited to ensure that all asynchronous disposal operations complete before continuing.</remarks>
    /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_webApp is IAsyncDisposable asyncDisp)
        {
            await asyncDisp.DisposeAsync().ConfigureAwait(false);
        }

        _cts?.Dispose();
        _cts = null;
        _webApp = null;
    }
}
