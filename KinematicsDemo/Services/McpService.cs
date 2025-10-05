using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Biosero.Kinematics.Common.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KinematicsDemo.Services
{
    public class McpService : IWebServerService, IAsyncDisposable
    {
        private WebApplication? _webApp;
        private CancellationTokenSource? _cts;
        private bool _started;

        public void InitializeMcpService(string[] args, string? hostAddress = null, int port = 6805, bool useHttps = true)
        {
            if (_webApp != null) return; // already built

            var builder = WebApplication.CreateSlimBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                if (string.IsNullOrWhiteSpace(hostAddress))
                {
                    options.ListenAnyIP(port, listen =>
                    {
                        if (useHttps) listen.UseHttps();
                    });
                }
                else
                {
                    options.Listen(System.Net.IPAddress.Parse(hostAddress), port, listen =>
                    {
                        if (useHttps) listen.UseHttps();
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

        public async Task StartAsync(string hostAddress)
        {
            if (_started) return;

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

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_started || _webApp == null) return;

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
}
