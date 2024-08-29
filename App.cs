using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class App
{
    public static int Main(string[] args)
    {
        var task = TestHttpAsync(46507);
        while (!task.IsCompleted)
        {
            WasiEventLoop.DispatchWasiEventLoop();
        }
        var exception = task.Exception;
        if (exception is not null)
        {
            throw exception;
        }
        return 0;
    }

    private static async Task TestHttpAsync(ushort port)
    {
        using var client = new HttpClient();
        var urlBase = $"http://127.0.0.1:{port}";

        Console.WriteLine("yossa a");
        {
            var response = await client.GetAsync($"{urlBase}/hello");
        Console.WriteLine("yossa a.1");
            response.EnsureSuccessStatusCode();
        Console.WriteLine("yossa a.2");            
            Trace.Assert(
                4 == response.Content.Headers.ContentLength,
                $"unexpected content length: {response.Content.Headers.ContentLength}"
            );
            Trace.Assert(
                "text/plain".Equals(response.Content.Headers.ContentType.ToString()),
                $"unexpected content type: \"{response.Content.Headers.ContentType}\""
            );
        Console.WriteLine("yossa a.2.1");                                    
            var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine("yossa a.3");                        
            Trace.Assert("hola".Equals(content), $"unexpected content: \"{content}\"");
        Console.WriteLine("yossa a.4");                                    
        }

        Console.WriteLine("yossa b");
        {
            var length = 10 * 1024 * 1024;
            var body = new byte[length];
            new Random().NextBytes(body);

            var content = new StreamContent(new MemoryStream(body));
            var type = "application/octet-stream";
            content.Headers.ContentType = new MediaTypeHeaderValue(type);

            var response = await client.PostAsync($"{urlBase}/echo", content);
            response.EnsureSuccessStatusCode();
            Trace.Assert(
                length == response.Content.Headers.ContentLength,
                $"unexpected content length: {response.Content.Headers.ContentLength}"
            );
            Trace.Assert(
                type.Equals(response.Content.Headers.ContentType.ToString()),
                $"unexpected content type: \"{response.Content.Headers.ContentType}\""
            );
            var received = await response.Content.ReadAsByteArrayAsync();
            Trace.Assert(body.SequenceEqual(received), "unexpected content");
        }

        Console.WriteLine("yossa c");
        using var impatientClient = new HttpClient();
        impatientClient.Timeout = TimeSpan.FromMilliseconds(100);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            await impatientClient.GetAsync($"{urlBase}/slow-hello");
            throw new Exception("request to /slow-hello endpoint should have timed out");
        }
        catch (TaskCanceledException _)
        {
            // The /slow-hello endpoint takes 10 seconds to return a
            // response, whereas we've set a 100ms timeout, so this is
            // expected.
        }
        stopwatch.Stop();
        Trace.Assert(stopwatch.ElapsedMilliseconds >= 100);
        Trace.Assert(stopwatch.ElapsedMilliseconds < 1000);
        Console.WriteLine("yossa d");        
    }

    internal static class WasiEventLoop
    {
        internal static void DispatchWasiEventLoop()
        {
            CallDispatchWasiEventLoop((Thread)null!);

            [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "DispatchWasiEventLoop")]
            static extern void CallDispatchWasiEventLoop(Thread t);
        }
    }
}
