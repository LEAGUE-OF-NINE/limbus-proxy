using Tomat.LimbusServer.API;
using Tomat.LimbusServer.PassthroughServer.API;

namespace Tomat.LimbusServer.PassthroughServer;

internal sealed class PassthroughServer : IPassthroughServer {
    public IServerProvider ServerProvider { get; }

    public string TargetServerAddress { get; }

    public int TargetServerPort { get; }

    public PassthroughServer() { }

    public void Dispose() { }
}
