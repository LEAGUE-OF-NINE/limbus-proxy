using System;
using Tomat.LimbusServer.API;
using Tomat.LimbusServer.PassthroughServer.API;

namespace Tomat.LimbusServer.PassthroughServer;

/// <summary>
///     Provides and hosts an <see cref="IPassthroughServer"/> instance.
/// </summary>
public sealed class PassthroughServerProvider : IServerProvider<IPassthroughServer> {
    public IPassthroughServer Start() {
    }

    void IDisposable.Dispose() {
    }
}
