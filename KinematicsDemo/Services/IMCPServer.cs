using System.Threading;
using System.Threading.Tasks;

namespace KinematicsDemo.Services;

internal interface IMCPServer
{
    Task StartAsync(string hostAddress);

    Task StopAsync(CancellationToken cancellationToken = default);
}
