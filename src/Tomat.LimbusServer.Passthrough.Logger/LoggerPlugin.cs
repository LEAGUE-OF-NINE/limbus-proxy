using BepInEx;
using BepInEx.Unity.IL2CPP;
using Tomat.LimbusServer.Proxy;

namespace Tomat.LimbusServer.Passthrough.Logger;

/// <summary>
///     Logger mod plugin entrypoint.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal sealed class LoggerPlugin : BasePlugin {
    public override void Load() {
        // Low priority to start with.
        // If many hooks are added, someone may want to add a high-priority
        // logger hook as well so before-and-after logs are documented.
        PassthroughServerHooks.RegisterHook(new PassthroughLogger(0.05f));
    }
}
