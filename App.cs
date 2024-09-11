using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcGreeterClient;

public class App
{
    public static int Main(string[] args)
    {
        var task = MainAsync();
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

    private static async Task MainAsync()
    {
        using var channel = GrpcChannel.ForAddress(
            "http://localhost:50051",
            new GrpcChannelOptions { HttpHandler = new HttpClientHandler() }
        );
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
        Console.WriteLine($"Reply: ${reply}");
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
