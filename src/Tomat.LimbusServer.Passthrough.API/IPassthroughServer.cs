using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tomat.LimbusServer.API;

namespace Tomat.LimbusServer.Passthrough.API;

/// <summary>
///     A simple <see cref="IServer"/> implementation that forwards all requests
///     to a target server and provides events for listening to and handling
///     requests and responses.
/// </summary>
public interface IPassthroughServer : IServer {
    /// <summary>
    ///     The IP address of the target server.
    /// </summary>
    string TargetServerAddress { get; }

    /// <summary>
    ///     Invoked when a request is received.
    ///     <br />
    ///     Return <see langword="false"/> to skip processing the request.
    /// </summary>
    event Func<HttpListenerContext, Task<bool>>? OnRequestReceived;

    /// <summary>
    ///     Invoked when a request is first being processed.
    /// </summary>
    event Func<HttpListenerRequest, Task>? OnPreProcessRequest;

    /// <summary>
    ///     Invoked when a new request has been constructed, but before it is
    ///     sent.
    /// </summary>
    event Func<HttpRequestMessage, Task>? OnPostProcessRequest;

    /// <summary>
    ///     Invoked when a response is first being processed.
    /// </summary>
    event Func<HttpResponseMessage, Task>? OnPreProcessResponse;

    /// <summary>
    ///     Invoked when a response has been processed, but before it's been
    ///     sent.
    /// </summary>
    event Func<HttpListenerResponse, Task>? OnPostProcessResponse;
}
