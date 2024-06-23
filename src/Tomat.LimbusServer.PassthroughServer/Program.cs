using System;
using System.Threading.Tasks;
using Tomat.LimbusServer.API;

namespace Tomat.LimbusServer.PassthroughServer;

internal static class Program {
    /// <summary>
    ///     The IP address to listen on (with or without a protocol); defaults
    ///     to <c>http://127.0.0.1</c>.
    /// </summary>
    private const string argument_ip = "--ip";

    /// <summary>
    ///     The port to listen on; defaults to <c>80</c>.
    /// </summary>
    private const string argument_port = "--port";

    private sealed class Arguments(string[] args) {
        public string IpAddress { get; } = GetArgument(args, argument_ip) ?? "http://127.0.0.1";

        public string Port { get; } = GetArgument(args, argument_port) ?? "80";

        private static string? GetArgument(string[] args, string name) {
            var nameIndex = Array.IndexOf(args, name);
            if (nameIndex == -1 || nameIndex + 1 >= args.Length)
                return null;

            return args[nameIndex + 1];
        }
    }

    internal static async Task Main(string[] args) {
        var arguments = new Arguments(args);
        var ip = arguments.IpAddress;
        if (!ushort.TryParse(arguments.Port, out var port))
            throw new ArgumentException($"Invalid port; expected a number between {ushort.MinValue} and {ushort.MaxValue} (but got {arguments.Port}).");

        var server = API.PassthroughServer.Create(ip, port);
        await StartServer(server);

        // Server is either killed by killing the process or by pressing Ctrl+C.
        // In either case, this condition should never be valid.
        if (server.State != ServerState.Stopped)
            throw new InvalidOperationException("Server did not stop properly.");
    }

    private static async Task StartServer(IServer server) {
        Console.CancelKeyPress += (_, cancelArgs) => {
            if (cancelArgs.Cancel)
                return;

            Console.WriteLine("Received SIGTERM, killing server...");
            server.Dispose();
        };

        await server.Start();
    }
}
