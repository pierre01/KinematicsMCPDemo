using CommunityToolkit.Mvvm.Messaging.Messages;
using KinematicsDemo.Models;

namespace KinematicsDemo.Messages;

internal class WebServerResponseMessage : ValueChangedMessage<WebServerResponse>
{
    public WebServerResponseMessage(WebServerResponse value)
        : base(value)
    {
    }
}
