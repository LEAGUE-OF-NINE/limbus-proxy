using System;

namespace Tomat.LimbusServer.API;

/// <summary>
///     Provides a server interface for the Limbus server.
/// </summary>
/// <remarks>
///     <see cref="IServerProvider"/>s should be used to initialize and start
///     servers. As such, there is not exposed API for starting a server from
///     the <see cref="IServer"/> instance.
///     <br />
///     Servers should be closed by invoking <see cref="IDisposable.Dispose"/>
///     (preferably automatically through this dispose pattern).
/// </remarks>
public interface IServer : IDisposable {
    /// <summary>
    ///     The server provider that created this server instance.
    /// </summary>
    IServerProvider ServerProvider { get; }
}
