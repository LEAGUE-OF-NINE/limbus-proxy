using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Text;
using Server;

namespace Tomat.LimbusServer.Proxy;

/// <summary>
///     Proxy mod plugin entrypoint.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal sealed class ProxyPlugin : BasePlugin {
    private const string default_proxy_url = "http://127.0.0.1";
    private const string default_identifier_key = "DEFAULT_CONSTANT_VALUE_THIS_GENERATES_A_NEW_KEY";
    private static readonly string[] known_urls = ["https://www.limbuscompanyapi.com", "https://www.limbuscompanyapi-2.com"];

    private static ConfigEntry<string>? proxyUrl;
    private static ConfigEntry<string>? identifierKey;
    private static ManualLogSource? log;

    public override void Load() {
        log = Log;

        proxyUrl = Config.Bind("General", "Proxy URL", default_proxy_url);
        identifierKey = Config.Bind("General", "Identifier Key", default_identifier_key);
        Log.LogInfo($"Using proxy url: {proxyUrl.Value}");
        Log.LogInfo($"Using identifier key: {identifierKey.Value}");

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
    }

    // ReSharper disable once InconsistentNaming
    // ReSharper disable UnusedParameter.Local
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.SendRequest))]
    private static bool PrefixSendRequest(HttpApiRequester __instance, HttpApiSchema httpApiSchema, bool isUrgent) {
        var theProxyUrl = proxyUrl?.Value ?? default_proxy_url;

        foreach (var url in known_urls)
            httpApiSchema._url = httpApiSchema._url.Replace(url, theProxyUrl);

        if (!string.IsNullOrEmpty(identifierKey?.Value))
            httpApiSchema._url += $"?key={identifierKey.Value}";

        log?.LogDebug($"Got request, validate URL: {httpApiSchema.URL}");
        return true;
    }
    // ReSharper restore UnusedParameter.Local
}
