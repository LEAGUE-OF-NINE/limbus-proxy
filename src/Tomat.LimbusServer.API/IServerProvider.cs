using System;

namespace Tomat.LimbusServer.API;

/// <summary>
///     Provides APIs for setting up and managing an <see cref="IServer"/>.
/// </summary>
/// <remarks>
///     The server provider's lifetime should not be ended until all servers
///     managed by it have been disposed.
/// </remarks>
public interface IServerProvider : IDisposable {
    /// <summary>
    ///     Starts the server.
    /// </summary>
    /// <returns>
    ///     An initialized and running <see cref="IServer"/> instance.
    /// </returns>
    IServer Start();
}

/// <summary>
///     Provides APIs for setting up and managing a <see cref="TServer"/>.
/// </summary>
/// <remarks>
///     The server provider's lifetime should not be ended until all servers
///     managed by it have been disposed.
/// </remarks>
/// <typeparam name="TServer">
///     The type of server that this provider manages.
/// </typeparam>
public interface IServerProvider<out TServer> : IServerProvider where TServer : IServer {
    IServer IServerProvider.Start() {
        return Start();
    }

    /// <summary>
    ///     Starts the server.
    /// </summary>
    /// <returns>
    ///     An initialized and running <see cref="TServer"/> instance.
    /// </returns>
    new TServer Start();
}
