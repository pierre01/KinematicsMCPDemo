using System.Threading.Tasks;

namespace KinematicsDemo.Services;

internal interface IWebServerService
{
    Task StartAsync(string hostAddress);
    void Stop();
}
