using DarkRift;
using DarkRift.Client;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LiteNetNetworkClientConnection : NetworkClientConnection
{
    public override DarkRift.ConnectionState ConnectionState => GetConnectionState();

    public override IEnumerable<IPEndPoint> RemoteEndPoints => new IPEndPoint[] { GetRemoteEndPoint("udp") };

    private NetManager _net;
    private NetPeer _peer;
    private readonly string _host;
    private readonly ushort _port;

    public LiteNetNetworkClientConnection(string host, ushort port)
    {
        _host = host;
        _port = port;

        //main initialization in Connect()
    }

    protected override void Dispose(bool disposing)
    {
        //TODO: make this fully correct

        Disconnect();

        base.Dispose(disposing);
    }

    public override void Connect()
    {
        EventBasedNetListener listener = new EventBasedNetListener();
        _net = new NetManager(listener);

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Debug.Log("Disconnected from server: " + peer.EndPoint + " reason " + disconnectInfo.Reason);
            HandleDisconnection();
        };
        
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            int length = dataReader.AvailableBytes;
            using (var messageBuffer = MessageBuffer.Create(length))
            {
                Buffer.BlockCopy(dataReader.RawData, dataReader.Position, messageBuffer.Buffer, 0, length);
                messageBuffer.Count = length;
                HandleMessageReceived(messageBuffer, deliveryMethod == DeliveryMethod.ReliableOrdered? SendMode.Reliable : SendMode.Unreliable);
            }

            dataReader.Recycle();
        };

        _net.Start();
        _peer = _net.Connect(_host, _port, "DarkRift2");
    }

    public void Update()
    {
        if (_net != null)
            _net.PollEvents();
    }

    private DarkRift.ConnectionState GetConnectionState()
    {
        if (_peer == null)
            return DarkRift.ConnectionState.Disconnected;

        //TODO: verify, because this is a guess
        switch (_peer.ConnectionState)
        {
            case LiteNetLib.ConnectionState.Connected:
                return DarkRift.ConnectionState.Connected;
            case LiteNetLib.ConnectionState.Disconnected:
                return DarkRift.ConnectionState.Disconnected;
            case LiteNetLib.ConnectionState.Outgoing:
                return DarkRift.ConnectionState.Connecting;
            case LiteNetLib.ConnectionState.ShutdownRequested:
                return DarkRift.ConnectionState.Disconnecting;
            default:
                return DarkRift.ConnectionState.Disconnected;
        }
    }

    public override bool Disconnect()
    {
        if (_peer == null)
            return true;

        _peer.Disconnect();
        _net.Stop();
        _peer = null;

        return true;
    }

    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        if (name == "udp")
        {
            return _peer.EndPoint;
        }
        else
        {
            throw new ArgumentException("Not a valid endpoint name!");
        }
    }

    public override bool SendMessageReliable(MessageBuffer message)
    {
        return SendMessage(message, DeliveryMethod.ReliableOrdered);
    }

    public override bool SendMessageUnreliable(MessageBuffer message)
    {
        return SendMessage(message, DeliveryMethod.Unreliable);
    }

    private bool SendMessage(MessageBuffer message, DeliveryMethod deliveryMethod)
    {
        if (_peer == null)
            return false;

        using (message)
        {
            //TODO: remove alloc/copy
            var writer = new NetDataWriter();
            writer.Put(message.Buffer, message.Offset, message.Count);

            _peer.Send(writer, deliveryMethod);

            return true;
        }
    }
}
