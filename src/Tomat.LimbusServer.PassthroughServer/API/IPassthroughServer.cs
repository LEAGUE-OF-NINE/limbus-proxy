using Tomat.LimbusServer.API;

namespace Tomat.LimbusServer.PassthroughServer.API;

/// <summary>
///     A simple <see cref="IServer"/> implementation that forwards all requests
///     to a target server and provides events for listening to and handling
///     requests and responses.
/// </summary>
public interface IPassthroughServer : IServer {
    /// <summary>
    ///     The IP address of the target server.
    /// </summary>
    string TargetServerAddress { get; }

    /// <summary>
    ///     The port of the target server.
    /// </summary>
    int TargetServerPort { get; }
}
