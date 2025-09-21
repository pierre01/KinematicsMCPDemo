using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Biosero.TeachPendant.Common;
using CommunityToolkit.Mvvm.Messaging;
using KinematicsDemo.Messages;
using KinematicsDemo.Models;
using Newtonsoft.Json;

namespace KinematicsDemo.Services;

internal class WebServerService : IWebServerService, IDisposable
{
    private CancellationTokenSource _cancellationToken;
    private HttpListener _listener;

    private readonly Dictionary<string, Action<WebServerRequest>> _resources =
        new Dictionary<string, Action<WebServerRequest>>();

    public WebServerService()
    {
        _resources = CreateResources();

        WeakReferenceMessenger.Default.Register<WebServerResponseMessage>(this, (r, m) =>
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (m.Value.IsSuccess)
            {
                SendResponse(m.Value.Context, HttpStatusCode.OK, m.Value.DataJson);
                return;
            }

            // TBD: Handle error
        });
    }

    private Dictionary<string, Action<WebServerRequest>> CreateResources()
    {
        return new Dictionary<string, Action<WebServerRequest>>()
        {
            { TeachPendantWebApiResources.Coordinates, CoordinatesResource },
            { TeachPendantWebApiResources.Move, MoveResource },
            { TeachPendantWebApiResources.Play, PlayResource },
            { TeachPendantWebApiResources.RailPosition, RailPositionResource },
            { TeachPendantWebApiResources.RecordedPoints, RecordedPointsResource },
            { TeachPendantWebApiResources.RecordPoint, RecordPointResource },
            { TeachPendantWebApiResources.StepPrecision, StepPrecisionResource },
        };
    }

    public async Task StartAsync(string hostAddress)
    {
        if (_listener != null &&
            _listener.IsListening)
        {
            Stop();
        }

        var stop = false;
        _listener = CreateHttpListener(hostAddress);
        _cancellationToken = new CancellationTokenSource();
        await Task.Run(
            () =>
        {
            _listener.Start();
            while (!stop)
            {
                try
                {
                    var context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // This exception is expected when the listener is stopped
                    stop = true;
                }
            }
        },
            _cancellationToken.Token);
    }

    public void Stop()
    {
        _cancellationToken.Cancel();
        Dispose();
    }

    public void Dispose()
    {
        if (_listener == null ||
            !_listener.IsListening)
        {
            return;
        }

        _listener.Stop();
        _listener.Close();
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var resource = context?.Request?.RawUrl?.Substring(1)?.ToLower();
        if (string.IsNullOrEmpty(resource) ||
            !_resources.ContainsKey(resource))
        {
            SendNotImplementedResponse(context);
            return;
        }

        _resources[resource].Invoke(new WebServerRequest(context, resource));
    }

    private static void CoordinatesResource(WebServerRequest webServerRequest)
        => WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));

    private static HttpListener CreateHttpListener(string hostAddress)
        => new HttpListener()
        {
            Prefixes = { hostAddress },
        };

    private static void MoveResource(WebServerRequest webServerRequest)
    {
        if (webServerRequest.Context.Request.HttpMethod != "POST")
        {
            SendMethodNotAllowedResponse(webServerRequest.Context);
            return;
        }

        try
        {
            var reader = new System.IO.StreamReader(
                webServerRequest.Context.Request.InputStream,
                webServerRequest.Context.Request.ContentEncoding);
            var data = reader.ReadToEnd();
            webServerRequest.RobotCommandInfo.Coordinate = JsonConvert.DeserializeObject<RobotCoordinate>(data);

            WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));
        }
        catch
        {
            SendBadRequestResponse(webServerRequest.Context);
        }
    }

    private static void PlayResource(WebServerRequest webServerRequest)
        => WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));

    private static void RailPositionResource(WebServerRequest webServerRequest)
        => WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));

    private static void RecordedPointsResource(WebServerRequest webServerRequest)
        => WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));

    private static void RecordPointResource(WebServerRequest webServerRequest)
        => WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));

    private static void StepPrecisionResource(WebServerRequest webServerRequest)
        => WeakReferenceMessenger.Default.Send(new WebServerRequestMessage(webServerRequest));

    private static void SendBadRequestResponse(HttpListenerContext context)
        => SendResponse(context, HttpStatusCode.BadRequest);

    private static void SendMethodNotAllowedResponse(HttpListenerContext context)
        => SendResponse(context, HttpStatusCode.MethodNotAllowed);

    private static void SendNotImplementedResponse(HttpListenerContext context)
        => SendResponse(context, HttpStatusCode.NotImplemented);

    private static void SendResponse(HttpListenerContext context, HttpStatusCode statusCode, string data = null)
    {
        var response = context.Response;
        response.StatusCode = (int)statusCode;

        if (!string.IsNullOrEmpty(data))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            response.ContentLength64 = buffer.Length;
            using var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
        }

        response.Close();
    }
}
