using DarkRift;
using DarkRift.Server;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Net;

public class LiteNetNetworkServerConnection : NetworkServerConnection
{
    public override DarkRift.ConnectionState ConnectionState => GetConnectionState();

    public override IEnumerable<IPEndPoint> RemoteEndPoints => new IPEndPoint[]{GetRemoteEndPoint("udp")};

    private readonly NetPeer _peer;

    internal LiteNetNetworkServerConnection(NetPeer peer)
    {
        _peer = peer;

        //any meaty initialization should go into StartListening()
    }

    public override void StartListening()
    {
    }

    private DarkRift.ConnectionState GetConnectionState()
    {
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
        _peer.Disconnect();

        //TODO: verify, because this is a guess
        var newState = GetConnectionState();
        return newState == DarkRift.ConnectionState.Disconnected
            || newState == DarkRift.ConnectionState.Disconnecting;
    }

    internal void OnDisconnection()
    {
        HandleDisconnection();
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

    internal void OnMessageReceived(MessageBuffer messageBuffer, SendMode sendMode)
    {
        HandleMessageReceived(messageBuffer, sendMode);
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
        using (message)
        {
            if (GetConnectionState() != DarkRift.ConnectionState.Connected)
                return false;

            //TODO: remove alloc/copy
            var writer = new NetDataWriter();
            writer.Put(message.Buffer, message.Offset, message.Count);

            _peer.Send(writer, deliveryMethod);

            return true;
        }
    }
}
