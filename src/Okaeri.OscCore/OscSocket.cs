using System.Net;
using System.Net.Sockets;

namespace Okaeri.OscCore;

internal sealed class OscSocket : IDisposable
{
    private readonly Socket _socket;
    private readonly Task _task;

    public int Port { get; }
    public OscServer Server { get; }

    private static CancellationTokenSource? _serveCancellationTokenSource;

    public OscSocket(int port, OscServer server)
    {
        Port = port;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = int.MaxValue };
        _task = new Task(Serve, TaskCreationOptions.LongRunning);
        Server = server;
    }

    public void Start()
    {
        if (_task != null && (_task.Status == TaskStatus.Running || _task.Status == TaskStatus.WaitingForActivation))
            return;

        if (!_socket.IsBound)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _socket.Bind(new IPEndPoint(IPAddress.Any, Port));

                    break;
                }
                catch (SocketException)
                {
                    _serveCancellationTokenSource?.Cancel();

                    if (_socket.Connected)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket.Disconnect(true);
                    }
                }

                Thread.Sleep(500);
            }
        }

        _task?.Start();
    }

    private async void Serve()
    {
        var buffer = Server.Parser._buffer;
        var socket = _socket;

        _serveCancellationTokenSource = new();

        while (!_serveCancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var receiveTask = socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                await Task.WhenAny(receiveTask, Task.Delay(-1, _serveCancellationTokenSource.Token));
                if (receiveTask.IsCompleted)
                {
                    int receivedByteCount = receiveTask.Result;
                    if (receivedByteCount == 0)
                        continue;

                    Server.ParseBuffer(receivedByteCount);
                }
            }
            catch (SocketException)
            {
                // Read Timeouts?
            }
        }
    }

    public void Dispose()
    {
        _serveCancellationTokenSource?.Cancel();
        _task.Wait();

        if (_socket.Connected)
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(true);
        }

        _socket.Close();
        _socket.Dispose();

        GC.SuppressFinalize(this);
    }
}
