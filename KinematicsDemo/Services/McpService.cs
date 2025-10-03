using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Biosero.Kinematics.Common.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KinematicsDemo.Services
{
    public class McpService : IWebServerService
    {
        private WebApplication? _webApp;
        private CancellationToken _serverCancellationToken;

        /// <summary>
        /// Initializes and starts the MCP service web application using the specified command-line arguments.
        /// </summary>
        /// <remarks>This method configures the web server to use HTTPS on port 3001 and sets up JSON
        /// serialization options for the MCP service. It should be called once during application startup to ensure the
        /// service is properly initialized and ready to handle requests.</remarks>
        /// <param name="args">An array of command-line arguments to configure the web application. Typically includes options for server
        /// configuration and environment settings.</param>
        public void InitializeMcpService(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            // Configure Kestrel to use HTTPS
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(3001, listenOptions =>
                {
                    listenOptions.UseHttps(); // HTTPS
                });
            });

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, RobotJsonContext.Default);
            });

            // Combine resolvers so AOT metadata is available for *all* involved types
            var toolSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                TypeInfoResolver = RobotJsonContext.Default,   // Jso serialization from LightsAPICommon
            };

            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly(serializerOptions: toolSerializerOptions);

            _webApp = builder.Build();

            var mcpGroup = _webApp.MapGroup("/mcp");
            mcpGroup.MapMcp();   // <— call MapMcp on the group; all routes get the prefix + auth

            //_webApp.Run(); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Task StartAsync(string hostAddress)
        {
            InitializeMcpService(new string[] { });
            if (_webApp == null)
            {
                throw new System.InvalidOperationException("Web application is not initialized.");
            }
            _serverCancellationToken = new CancellationToken();
            return _webApp.StartAsync(_serverCancellationToken);
        }

        public void Stop()
        {
            if (_webApp == null)
            {
                throw new System.InvalidOperationException("Web application is not initialized.");
            }
            _webApp.StopAsync(_serverCancellationToken).GetAwaiter().GetResult();
        }
    }
}
