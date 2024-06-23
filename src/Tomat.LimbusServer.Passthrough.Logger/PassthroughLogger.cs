using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Tomat.LimbusServer.Proxy;

namespace Tomat.LimbusServer.Passthrough.Logger;

internal sealed class PassthroughLogger(float priority) : PassthroughServerHooks.IPassthroughServerHook {
    private const string constant_padding = " ";
    private const int tab_length = 4;
    private const int padding_length = tab_length * 3;

    public float Priority { get; } = priority;

    public Task<bool> OnRequestReceived(HttpListenerContext context) {
        return Task.FromResult(true);
    }

    public Task OnPreProcessRequest(HttpListenerRequest request) {
        WritePadded(request.HttpMethod);
        Write(' ');
        ForegroundColor = ConsoleColor.DarkGray;
        Write(request.Url?.Host);
        ForegroundColor = ConsoleColor.White;
        Write(request.Url?.AbsolutePath);
        ResetColor();
        WriteLine();

        WriteHeaders(request.Headers);

        return Task.CompletedTask;
    }

    public async Task OnPostProcessRequest(HttpRequestMessage request) {
        var content = await request.Content!.ReadAsByteArrayAsync();
        var encrypted = request.Headers.TryGetValues("Content-Encrypted", out var contentEncryptedValues);
        WriteBody(content, encrypted, long.Parse(contentEncryptedValues?.First() ?? "0"));
    }

    public async Task OnPreProcessResponse(HttpResponseMessage response) {
        WritePadded("");
        Write(' ');
        WriteStatusCode(response.StatusCode);
        Write(' ');
        Write(response.Content.Headers.ContentType?.MediaType);
        Write(" - ");
        Write((response.Content.Headers.ContentLength ?? 0) + " bytes");
        WriteLine();

        WriteHeaders(response.Headers);

        if (response.Content.Headers.ContentType?.MediaType == "application/json") {
            var json = await response.Content.ReadAsByteArrayAsync();
            var encrypted = response.Headers.TryGetValues("Content-Encrypted", out var contentEncryptedValues);
            WriteBody(json, encrypted, long.Parse(contentEncryptedValues?.First() ?? "0"));
        }

        WriteLine();
        WriteLine();
    }

    public Task OnPostProcessResponse(HttpListenerResponse response) {
        return Task.CompletedTask;
    }

    private static void WritePadded(string text) {
        Write(constant_padding);

        ForegroundColor = ConsoleColor.Cyan;
        Write(text.PadLeft(padding_length));
        ResetColor();
    }

    private static void WriteStatusCode(HttpStatusCode statusCode) {
        ForegroundColor = ConsoleColor.Green;
        Write($"<- {(int)statusCode}");
        ForegroundColor = ConsoleColor.DarkGray;
        Write($" ({statusCode})");
        ResetColor();
    }

    private static void WriteBody(byte[] body, bool encrypted, long timestamp) {
        if (!encrypted) {
            var lines = Encoding.UTF8.GetString(body).Split('\n');
            foreach (var line in lines) {
                WritePadded("");
                WriteLine(' ' + line);
            }

            return;
        }

        var hexByteString = BitConverter.ToString(body).Replace("-", " ");

        WritePadded("ENCRYPTED");
        WriteLine($" ({timestamp})" + hexByteString);
    }

    private static void WriteHeaders(NameValueCollection headers) {
        foreach (var header in headers.AllKeys) {
            if (header is null)
                continue;

            if (!FilterHeader(header))
                continue;

            WritePadded("");
            Write("    ");
            ForegroundColor = ConsoleColor.DarkGray;
            Write(header);
            ForegroundColor = ConsoleColor.White;
            Write(": ");
            ResetColor();
            Write(headers[header]);
            WriteLine();
        }
    }

    private static void WriteHeaders(HttpResponseHeaders headers) {
        foreach (var header in headers) {
            if (!FilterHeader(header.Key))
                continue;

            WritePadded("");
            Write("    ");
            ForegroundColor = ConsoleColor.DarkGray;
            Write(header.Key);
            ForegroundColor = ConsoleColor.White;
            Write(": ");
            ResetColor();
            Write(header.Value.First());
            WriteLine();
        }
    }

    private static bool FilterHeader(string headerName) {
        return headerName.Equals("Content-Encrypted", StringComparison.CurrentCultureIgnoreCase);
    }

    #region Console Stubs
    private static ConsoleColor ForegroundColor {
        set => ConsoleManager.SetConsoleColor(value);
    }

    private static void Write(string text) {
        ConsoleManager.ConsoleStream?.Write(text);
    }

    private static void Write(char text) {
        ConsoleManager.ConsoleStream?.Write(text);
    }

    private static void WriteLine() {
        ConsoleManager.ConsoleStream?.WriteLine();
    }

    private static void WriteLine(string text) {
        ConsoleManager.ConsoleStream?.WriteLine(text);
    }

    private static void ResetColor() {
        ConsoleManager.SetConsoleColor(ConsoleColor.Gray);
    }
    #endregion
}
