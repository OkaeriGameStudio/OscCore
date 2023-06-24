using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using BlobHandles;

namespace Okaeri.OscCore;

public class OscForwarder : IDisposable
{
    /// <summary>
    /// The socket for forwarding messages
    /// </summary>
    public Socket Socket { get; }

    /// <summary>Serializes outgoing messages</summary>
    public BlobString BlobString { get; }

    /// <summary>Serializes outgoing messages</summary>
    public OscMessageValues? OscMessageValues { get; }

    /// <summary>Where this forwarder is sending messages to</summary>
    public IPEndPoint Destination { get; }

    /// <summary>Serializes outgoing messages</summary>
    public OscWriter Writer { get; }

    public ConcurrentQueue<(BlobString, OscMessageValues)> ForwardingQueue { get; set;  }

    public OscForwarder(string ipAddress, int port)
    {
        Writer = new();

        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        if (ipAddress == "255.255.255.255")
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        Destination = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        Socket.Connect(Destination);
    }

    public void ForwardMessage(BlobString address, OscMessageValues values)
    {
        ForwardingQueue.Enqueue((address, values));
    }

    public void Send()
    {
        while (true)
        {

            ForwardingQueue.TryDequeue(out var message);

            BlobString address = message.Item1;
            OscMessageValues values = message.Item2;

            // Process the message, parse it, and send the BlobString + parsed message to another class
            // Example:
            // var parsedMessage = ParseMessage(values);
            // AnotherClass.SendMessage(address, parsedMessage);


            // Send the serialized message via the socket to the destination
        }
    }

    private bool _isDisposed = false;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            Socket.Dispose();
            Writer.Dispose();
            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    ~OscForwarder()
    {
        Dispose();
    }
}
