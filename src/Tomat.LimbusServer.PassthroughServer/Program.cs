using System;
using System.Threading.Tasks;
using Tomat.LimbusServer.API;

namespace Tomat.LimbusServer.PassthroughServer;

internal static class Program {
    /// <summary>
    ///     The IP address to listen on; defaults to <c>http://localhost:80/</c>.
    /// </summary>
    private const string argument_listening_addresses = "--listening-addresses";

    /// <summary>
    ///     The target address; defaults to
    ///     <c>https://www.limbuscompanyapi.com</c>.
    /// </summary>
    private const string argument_target_address = "--target-address";

    internal static async Task Main(string[] args) {
        var listeningAddresses = (GetArgument(args, argument_listening_addresses) ?? "http://localhost:80/").Split(' ');
        var targetAddress = GetArgument(args, argument_target_address) ?? "https://www.limbuscompanyapi.com";

        var server = Passthrough.API.PassthroughServer.Create(listeningAddresses, targetAddress);
        await StartServer(server);

        // Server is either killed by killing the process or by pressing Ctrl+C.
        // In either case, this condition should never be valid.
        if (server.State != ServerState.Stopped)
            throw new InvalidOperationException("Server did not stop properly.");
    }

    private static async Task StartServer(IServer server) {
        Console.CancelKeyPress += (_, e) => {
            if (e.Cancel)
                return;

            Console.WriteLine("Cancel key pressed, killing server...");
            server.Dispose();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) => {
            Console.WriteLine("Received SIGTERM, killing server...");
            server.Dispose();
        };

        await server.Start();
    }

    private static string? GetArgument(string[] args, string name) {
        var nameIndex = Array.IndexOf(args, name);
        if (nameIndex == -1 || nameIndex + 1 >= args.Length)
            return null;

        return args[nameIndex + 1];
    }
}
