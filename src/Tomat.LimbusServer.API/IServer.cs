using System;
using System.Threading.Tasks;

namespace Tomat.LimbusServer.API;

/// <summary>
///     Provides a server interface for the Limbus server.
/// </summary>
/// <remarks>
///     Servers should be stopped/terminated by calling
///     <see cref="IDisposable.Dispose"/>; this is most preferably done through
///     the dispose pattern (i.e. <see langword="using"/> statement) if they
///     have an internal condition for termination, otherwise one should listen
///     for a SIGTERM signal or similar and handle disposal manually.
/// </remarks>
public interface IServer : IDisposable {
    /// <summary>
    ///     The current state of the server.
    /// </summary>
    ServerState State { get; }

    /// <summary>
    ///     Starts the server.
    /// </summary>
    Task Start();
}
