using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tomat.LimbusServer.Passthrough.API;

namespace Tomat.LimbusServer.Proxy;

public static class PassthroughServerHooks {
    public interface IPassthroughServerHook {
        float Priority { get; }

        Task<bool> OnRequestReceived(HttpListenerContext context);

        Task OnPreProcessRequest(HttpListenerRequest request);

        Task OnPostProcessRequest(HttpRequestMessage request);

        Task OnPreProcessResponse(HttpResponseMessage response);

        Task OnPostProcessResponse(HttpListenerResponse response);
    }

    private static readonly List<IPassthroughServerHook> hooks = [];

    public static void RegisterHook(IPassthroughServerHook hook) {
        for (var i = 0; i < hooks.Count; i++) {
            if (!(hook.Priority > hooks[i].Priority))
                continue;

            hooks.Insert(i, hook);
            return;
        }

        hooks.Add(hook);
    }

    public static void Hook(IPassthroughServer server) {
        server.OnRequestReceived += async context => {
            foreach (var hook in hooks) {
                if (!await hook.OnRequestReceived(context))
                    return false;
            }

            return true;
        };

        server.OnPreProcessRequest += async request => {
            foreach (var hook in hooks)
                await hook.OnPreProcessRequest(request);
        };

        server.OnPostProcessRequest += async request => {
            foreach (var hook in hooks)
                await hook.OnPostProcessRequest(request);
        };

        server.OnPreProcessResponse += async response => {
            foreach (var hook in hooks)
                await hook.OnPreProcessResponse(response);
        };

        server.OnPostProcessResponse += async response => {
            foreach (var hook in hooks)
                await hook.OnPostProcessResponse(response);
        };
    }
}
