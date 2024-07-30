using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class App {
    public static int Main(string[] args)
    {
        var task = Run(Int32.Parse(args[0]));
        while (!task.IsCompleted)
        {
            WasiEventLoop.Dispatch();
        }
        var exception = task.Exception;
        if (exception is not null)
        {
            throw exception;
        }

        return 0;
    }

    private static async Task Run(int port)
    {
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://127.0.0.1:{port}/hello");
        response.EnsureSuccessStatusCode();
        Debug.Assert(4 == response.Content.Headers.ContentLength);
        Debug.Assert("text/plain".Equals(response.Content.Headers.ContentType));
        Debug.Assert("hola".Equals(await response.Content.ReadAsStringAsync()));
    }
}

internal static class WasiEventLoop
{
    internal static void Dispatch()
    {
        CallDispatch((Thread)null!);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "DispatchWasiEventLoop")]
        static extern void CallDispatch(Thread t);
    }
}
