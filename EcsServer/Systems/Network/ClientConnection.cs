using System.Net;
using System.Net.Sockets;

namespace EcsServer.Systems.Network;

public class ClientConnection
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly UdpClient _udpClient;
    public readonly Queue<byte[]> IncomingPackets = new();

    public ClientConnection(TcpClient tcpClient, UdpClient udpClient)
    {
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
        _udpClient = udpClient;
        StartReadingTcp();
    }

    private async void StartReadingTcp()
    {
        var buffer = new byte[4096];
        try
        {
            while (_tcpClient.Connected)
            {
                var bytesRead = await _stream.ReadAsync(buffer);
                if (bytesRead == 0) 
                    break;
                
                var data = new byte[bytesRead];
                Array.Copy(buffer, data, bytesRead);
                lock (IncomingPackets) { IncomingPackets.Enqueue(data); }
            }
        }
        catch { /* Handle disconnect */ }
    }

    public void SendTcp(byte[] data)
    {
        if (!_tcpClient.Connected) 
            return;
        
        var lengthPrefix = BitConverter.GetBytes(data.Length);
        _stream.Write(lengthPrefix, 0, 4);
        _stream.Write(data, 0, data.Length);
    }

    public void SendUdp(byte[] data)
    {
        if (!_tcpClient.Connected) 
            return;
        
        _udpClient.SendAsync(data, data.Length, (IPEndPoint)_tcpClient.Client.RemoteEndPoint!);
    }
    
    public void HandleUdpData(byte[] data)
    {
        lock (IncomingPackets) { IncomingPackets.Enqueue(data); }
    }
}