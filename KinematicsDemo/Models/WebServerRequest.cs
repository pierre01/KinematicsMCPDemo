using System.Net;
using Biosero.TeachPendant.Common;

namespace KinematicsDemo.Models;

internal class WebServerRequest
{
    public HttpListenerContext Context { get; set; }

    public RobotCommandInfo RobotCommandInfo { get; set; } = new RobotCommandInfo();

    public WebServerRequest(HttpListenerContext context, string command)
    {
        Context = context;
        RobotCommandInfo.Command = command;
    }
}
