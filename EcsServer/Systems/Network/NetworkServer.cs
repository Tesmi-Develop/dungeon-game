using System.Net;
using System.Net.Sockets;
using EcsServer.Components.Events;
using EcsServer.Events;
using Hypercube.Utilities.Dependencies;

namespace EcsServer.Systems.Network;

[EcsSystem]
public class NetworkServer : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus;
    private readonly TcpListener _tcpListener;
    private readonly UdpClient _udpClient;
    private readonly int _port;
    
    public readonly Dictionary<IPEndPoint, ClientConnection> Connections = new();

    public NetworkServer()
    {
        _port = 5000;
        _tcpListener = new TcpListener(IPAddress.Any, _port);
        _udpClient = new UdpClient(_port);
    }

    public override void Initialize()
    {
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
        
        ReceiveUdp();
    }

    private void OnTcpClientConnected(IAsyncResult ar)
    {
        var client = _tcpListener.EndAcceptTcpClient(ar);
        var endPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
        
        var connection = new ClientConnection(client, _udpClient);
        lock (Connections) { Connections[endPoint] = connection; }
        
        _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
        _eventBus.Raise(new ClientConnected { ClientConnection = connection });
    }

    private async void ReceiveUdp()
    {
        while (true)
        {
            var result = await _udpClient.ReceiveAsync();

            lock (Connections)
            {
                if (Connections.TryGetValue(result.RemoteEndPoint, out var conn))
                {
                    conn.HandleUdpData(result.Buffer);
                }
            }
        }
    }
}