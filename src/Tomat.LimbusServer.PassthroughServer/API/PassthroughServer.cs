﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tomat.LimbusServer.API;

namespace Tomat.LimbusServer.PassthroughServer.API;

public sealed class PassthroughServer : IPassthroughServer {
    public ServerState State { get; private set; }

    public string TargetServerHost { get; }

    public string TargetServerAddress { get; }

    public int TargetServerPort { get; }

    public event Func<HttpListenerContext, Task<bool>>? OnRequestReceived;

    public event Func<HttpListenerRequest, Task>? OnPreProcessRequest;

    public event Func<HttpRequestMessage, Task>? OnPostProcessRequest;

    public event Func<HttpResponseMessage, Task>? OnPreProcessResponse;

    public event Func<HttpListenerResponse, Task?>? OnPostProcessResponse;

    private readonly HttpListener listener = new();
    private readonly HttpClient client = new();

    private PassthroughServer(string targetServerAddress, int targetServerPort) {
        TargetServerHost = targetServerAddress.Split("://").Last().Trim('/');
        TargetServerAddress = targetServerAddress;
        TargetServerPort = targetServerPort;

        // If the user specifies a protocol (such as HTTP), then we use that.
        // Otherwise, we only allow (default to) HTTPS. This means localhost
        // needs to specify HTTP.
        listener.Prefixes.Add(targetServerAddress.Contains("://") ? $"{targetServerAddress}:{targetServerPort}/" : $"https://{targetServerAddress}:{targetServerPort}/");
    }

    public async Task Start() {
        // We don't want to allow servers to be started when they're busy doing
        // something or have been disposed of.
        if (State is not ServerState.Idle)
            throw new InvalidOperationException("Attempted to start non-idle server.");

        State = ServerState.Starting;

        listener.Start();

        State = ServerState.Running;

        while (true) {
            var context = await listener.GetContextAsync();

            // Skip this request if the event handler returns false.
            if (OnRequestReceived is not null && !await OnRequestReceived(context))
                continue;

            await ProcessRequest(context);
        }
    }

    void IDisposable.Dispose() {
        // We don't want to allow already-disposed servers to be disposed, nor
        // do we want to interrupt setup or tear-down processes.
        if (State is not (ServerState.Running or ServerState.Idle))
            throw new InvalidOperationException("Attempted to dispose of a server that is not running or idle.");

        State = ServerState.Stopping;

        // TODO: Disposal logic.

        State = ServerState.Stopped;
    }

    private async Task ProcessRequest(HttpListenerContext context) {
        if (OnPreProcessRequest is not null)
            await OnPreProcessRequest.Invoke(context.Request);

        var request = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), TargetServerAddress + context.Request.Url?.PathAndQuery);
        request.Content = new StreamContent(context.Request.InputStream);

        foreach (var header in context.Request.Headers.AllKeys) {
            if (header is not null)
                request.Headers.TryAddWithoutValidation(header, context.Request.Headers[header]);
        }

        request.Content.Headers.ContentLength = context.Request.ContentLength64;
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType!);
        request.Headers.Host = TargetServerHost;
        request.Version = context.Request.ProtocolVersion;

        if (OnPostProcessRequest is not null)
            await OnPostProcessRequest.Invoke(request);

        try {
            var response = await client.SendAsync(request);
            if (OnPreProcessResponse is not null)
                await OnPreProcessResponse.Invoke(response);

            context.Response.StatusCode = (int)response.StatusCode;

            foreach (var header in response.Headers)
                context.Response.Headers.Add(header.Key, string.Join(", ", header.Value));

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(context.Response.OutputStream);
        }
        catch (HttpRequestException e) {
            context.Response.StatusCode = 500;
            var bytes = Encoding.UTF8.GetBytes(e.Message);
            await context.Response.OutputStream.WriteAsync(bytes);
        }
        finally {
            if (OnPostProcessResponse is not null)
                await OnPostProcessResponse.Invoke(context.Response);

            context.Response.OutputStream.Close();
            context.Response.Close();
        }
    }

    public static IPassthroughServer Create(string targetServerAddress, int targetServerPort) {
        return new PassthroughServer(targetServerAddress, targetServerPort);
    }
}