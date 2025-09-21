using KinematicsDemo.Models;
using KinematicsDemo.ViewModels;

namespace KinematicsDemo.Services;

internal interface IWebServerCommandParser
{
    void ParseCommand(TeachPendantViewModel teachPendantViewModel, WebServerRequest webServerRequest);
}
