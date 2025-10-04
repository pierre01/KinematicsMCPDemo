using System.Threading;
using System.Threading.Tasks;

namespace KinematicsDemo.Services;

internal interface IWebServerService
{
    Task StartAsync(string hostAddress);

    Task StopAsync(CancellationToken cancellationToken = default);
}
