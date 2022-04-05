using System;
using System.Collections.Generic;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using LiteNetLib;
using UnityEngine;

public class LiteNetNetworkListener : NetworkListener
{
    public override Version Version => new Version(0, 0, 0);

    private NetManager server;
    private readonly Dictionary<NetPeer, LiteNetNetworkServerConnection> connections = new Dictionary<NetPeer, LiteNetNetworkServerConnection>();
    private readonly object @lock = new object();
    private readonly ushort port;
    private Thread updateThread;
    private volatile bool stopping;

    public LiteNetNetworkListener(NetworkListenerLoadData pluginLoadData)
        : base(pluginLoadData)
    {
        port = pluginLoadData.Port;
    }

    protected override void Dispose(bool disposing)
    {
        //TODO: make this fully correct

        if (updateThread != null)
        {
            stopping = true;
            updateThread.Join();
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

            lock (@lock)
            {
                connections.Add(peer, connection);
            }

            RegisterConnection(connection);
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Debug.Log("We got a disconnection: " + peer.EndPoint);

            lock (@lock)
            {
                if (connections.TryGetValue(peer, out LiteNetNetworkServerConnection connection))
                {
                    connection.OnDisconnection();
                    connections.Remove(peer);
                }
            }
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            int length = dataReader.AvailableBytes;
            using (var messageBuffer = MessageBuffer.Create(length))
            {
                Buffer.BlockCopy(dataReader.RawData, dataReader.Position, messageBuffer.Buffer, 0, length);
                messageBuffer.Count = length;

                lock (@lock)
                {
                    if (connections.TryGetValue(fromPeer, out LiteNetNetworkServerConnection connection))
                    {
                        connection.OnMessageReceived(messageBuffer, deliveryMethod == DeliveryMethod.ReliableOrdered ? SendMode.Reliable : SendMode.Unreliable);
                    }
                }
            }

            dataReader.Recycle();
        };

        server = new NetManager(listener);
        server.Start(port);

        Debug.Log("We started listening on UDP port: " + port);

        updateThread = new Thread(UpdateLoop);
        updateThread.Start();
    }

    private void UpdateLoop()
    {
        while (!stopping)
        {
            server.PollEvents();
            Thread.Sleep(1);
        }
    }
}
