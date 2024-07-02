using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

public class App {
    public static int Main(string[] args)
    {
        var task = Run();
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

    private static async Task Run()
    {
        using TcpClient client = new();
        await client.ConnectAsync(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 7532));
        await using NetworkStream stream = client.GetStream();

        await stream.WriteAsync(Encoding.UTF8.GetBytes("hello, world!"));
        
        var buffer = new byte[1024];
        int received = await stream.ReadAsync(buffer);

        var message = Encoding.UTF8.GetString(buffer, 0, received);
        Console.WriteLine($"Message received: \"{message}\"");
    }
}

internal static class WasiEventLoop
{
    internal static void Dispatch()
    {
        CallDispatch((Thread)null!);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Dispatch")]
        static extern void CallDispatch(Thread t);
    }
}
