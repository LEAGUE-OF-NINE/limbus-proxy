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
