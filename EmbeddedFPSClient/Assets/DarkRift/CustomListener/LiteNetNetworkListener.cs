using DarkRift.Server;
using System;
using System.Collections.Generic;
using LiteNetLib;
using DarkRift;
using System.Threading;
using UnityEngine;

public class LiteNetNetworkListener : NetworkListener
{
    public override Version Version => new Version(0, 0, 0);

    private NetManager _server;
    private readonly Dictionary<NetPeer, LiteNetNetworkServerConnection> _connections = new Dictionary<NetPeer, LiteNetNetworkServerConnection>();
    private readonly object _lock = new object();
    private ushort _port;
    private Thread _updateThread;
    private volatile bool _stopping;

    public LiteNetNetworkListener(NetworkListenerLoadData pluginLoadData)
        : base(pluginLoadData)
    {
        _port = pluginLoadData.Port;
    }

    protected override void Dispose(bool disposing)
    {
        //TODO: make this fully correct

        if (_updateThread != null)
        {
            _stopping = true;
            _updateThread.Join();
        }

        base.Dispose(disposing);
    }

    public override void StartListening()
    {
        var listener = new EventBasedNetListener();
        listener.ConnectionRequestEvent += request =>
        {
            request.AcceptIfKey("DarkRift2");
            Debug.Log("We got a new client " + request.RemoteEndPoint);
        };

        listener.PeerConnectedEvent += peer =>
        {
            Debug.Log("We got a connection: " + peer.EndPoint);

            var connection = new LiteNetNetworkServerConnection(peer);

            lock (_lock)
            {
                _connections.Add(peer, connection);
            }

            RegisterConnection(connection);
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Debug.Log("We got a disconnection: " + peer.EndPoint);

            lock (_lock)
            {
                if (_connections.TryGetValue(peer, out LiteNetNetworkServerConnection connection))
                {
                    connection.OnDisconnection();
                    _connections.Remove(peer);
                };
            }
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            int length = dataReader.AvailableBytes;
            using (var messageBuffer = MessageBuffer.Create(length))
            {
                Buffer.BlockCopy(dataReader.RawData, dataReader.Position, messageBuffer.Buffer, 0, length);
                messageBuffer.Count = length;

                lock (_lock)
                {
                    if (_connections.TryGetValue(fromPeer, out LiteNetNetworkServerConnection connection))
                    {
                        connection.OnMessageReceived(messageBuffer, deliveryMethod == DeliveryMethod.ReliableOrdered ? SendMode.Reliable : SendMode.Unreliable);
                    }
                }
            }

            dataReader.Recycle();
        };

        _server = new NetManager(listener);
        _server.Start(_port);

        Debug.Log("We started listening on UDP port: " + _port);

        _updateThread = new Thread(UpdateLoop);
        _updateThread.Start();
    }

    private void UpdateLoop()
    {
        while (!_stopping)
        {
            _server.PollEvents();
            Thread.Sleep(1);
        }
    }
}
