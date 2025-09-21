using System.Net;
using Newtonsoft.Json;

namespace KinematicsDemo.Models;

internal class WebServerResponse
{
    public HttpListenerContext Context { get; set; }

    public string DataJson
        => DataObj == null
               ? string.Empty
               : JsonConvert.SerializeObject(DataObj);

    public object DataObj { get; set; }


    public bool IsSuccess { get; set; }

    public WebServerResponse(bool isSuccess = true)
    {
        IsSuccess = isSuccess;
    }

    public WebServerResponse(object dataObj, bool isSuccess = true)
        : this(isSuccess)
    {
        DataObj = dataObj;
    }
}
