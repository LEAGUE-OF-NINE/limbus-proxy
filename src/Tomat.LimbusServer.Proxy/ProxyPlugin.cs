using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Text;
using Server;
using Tomat.LimbusServer.Passthrough.API;

namespace Tomat.LimbusServer.Proxy;

/// <summary>
///     Proxy mod plugin entrypoint.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal sealed class ProxyPlugin : BasePlugin {
    private const string default_proxy_url = "http://127.0.0.1";
    private const string default_identifier_key = "DEFAULT_CONSTANT_VALUE_THIS_GENERATES_A_NEW_KEY";
    private const bool default_host_log_passthrough = true;
    private const string default_host_url = "www.limbuscompanyapi.com";
    private static readonly string[] known_urls = [default_host_url, "https://www.limbuscompanyapi-2.com"];

    private static ConfigEntry<string>? proxyUrl;
    private static ConfigEntry<string>? identifierKey;
    private static ConfigEntry<bool>? hostLogPassthrough;
    private static ManualLogSource? log;

    public override void Load() {
        log = Log;

        proxyUrl = Config.Bind("General", "Proxy URL", default_proxy_url);
        identifierKey = Config.Bind("General", "Identifier Key", default_identifier_key);
        hostLogPassthrough = Config.Bind("General", "Host Log Passthrough", default_host_log_passthrough);
        Log.LogInfo($"Using proxy url: {proxyUrl.Value} (host url: {GetHostUrl()})");
        Log.LogInfo($"Using identifier key: {identifierKey.Value}");
        Log.LogInfo($"Hosting log passthrough server: {hostLogPassthrough.Value}");

        if (identifierKey.Value == default_identifier_key) {
            Log.LogInfo($"Generating new identifier key (because key was '{default_identifier_key}')...");

            const string default_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var key = new StringBuilder();
            for (var i = 0; i < 16; i++)
                key.Append(default_chars[random.Next(default_chars.Length)]);

            identifierKey.Value = key.ToString();
            Log.LogInfo($"Generated new identifier key: {identifierKey.Value}");
        }
        else if (string.IsNullOrEmpty(identifierKey.Value)) {
            Log.LogInfo("Identifier key was null or emptying, not including in query...");
        }

        Log.LogInfo("Applying Harmony patches...");
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ProxyPlugin));

        if (!hostLogPassthrough.Value)
            return;

        var port = new Random().Next(10000, 20000);
        var listeningAddress = $"http://127.0.0.1:{port}/";

        Task.Run(
            () => {
                using var server = PassthroughServer.Create([listeningAddress], "https://www.limbuscompanyapi.com");
                server.Start();
            }
        );
    }

    // ReSharper disable once InconsistentNaming
    // ReSharper disable UnusedParameter.Local
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.SendRequest))]
    private static bool PrefixSendRequest(HttpApiRequester __instance, HttpApiSchema httpApiSchema, bool isUrgent) {
        foreach (var url in known_urls)
            httpApiSchema._url = httpApiSchema._url.Replace(url, GetHostUrl());

        if (!string.IsNullOrEmpty(identifierKey?.Value))
            httpApiSchema._url += $"?key={identifierKey.Value}";

        log?.LogDebug($"Got request, validate URL: {httpApiSchema.URL}");
        return true;
    }
    // ReSharper restore UnusedParameter.Local

    private static string GetHostUrl() {
        var theProxyUrl = proxyUrl?.Value;
        return string.IsNullOrEmpty(theProxyUrl) ? default_host_url : theProxyUrl;
    }
}
