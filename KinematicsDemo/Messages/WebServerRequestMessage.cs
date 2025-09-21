using CommunityToolkit.Mvvm.Messaging.Messages;
using KinematicsDemo.Models;

namespace KinematicsDemo.Messages;

internal class WebServerRequestMessage : ValueChangedMessage<WebServerRequest>
{
    public WebServerRequestMessage(WebServerRequest value)
        : base(value)
    {
    }
}
