namespace Tomat.LimbusServer.API;

public enum ServerState {
    /// <summary>
    ///     The server is not running.
    /// </summary>
    Idle,

    /// <summary>
    ///     The server is starting up.
    /// </summary>
    Starting,

    /// <summary>
    ///     The server is running.
    /// </summary>
    Running,

    /// <summary>
    ///     The server is shutting down.
    /// </summary>
    Stopping,

    /// <summary>
    ///     The server has stopped (disposed).
    /// </summary>
    Stopped,
}
