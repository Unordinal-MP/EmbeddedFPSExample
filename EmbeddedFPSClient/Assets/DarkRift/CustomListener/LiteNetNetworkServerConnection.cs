using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Server;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class LiteNetNetworkServerConnection : NetworkServerConnection
{
    public override DarkRift.ConnectionState ConnectionState => GetConnectionState();

    public override IEnumerable<IPEndPoint> RemoteEndPoints => new IPEndPoint[] { GetRemoteEndPoint("udp") };

    private readonly NetPeer peer;

    private bool handledDisconnection;

    internal LiteNetNetworkServerConnection(NetPeer peer)
    {
        this.peer = peer;

        //any meaty initialization should go into StartListening()
    }

    public override void StartListening()
    {
    }

    private DarkRift.ConnectionState GetConnectionState()
    {
        //TODO: verify, because this is a guess
        switch (peer.ConnectionState)
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
        peer.Disconnect();

        //TODO: verify, because this is a guess
        var newState = GetConnectionState();
        return newState == DarkRift.ConnectionState.Disconnected
            || newState == DarkRift.ConnectionState.Disconnecting;
    }

    internal void OnDisconnection()
    {
        TryHandleDisconnection("whilst updating");
    }

    public override IPEndPoint GetRemoteEndPoint(string name)
    {
        if (name == "udp")
        {
            return peer.EndPoint;
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
            if (CheckDisconnection())
            {
                return false;
            }

            //TODO: remove alloc/copy
            var writer = new NetDataWriter();
            writer.Put(message.Buffer, message.Offset, message.Count);

            peer.Send(writer, deliveryMethod);

            return true;
        }
    }

    private bool CheckDisconnection()
    {
        if (GetConnectionState() != DarkRift.ConnectionState.Connected)
        {
            TryHandleDisconnection("whilst sending");
            return true;
        }

        return false;
    }

    private void TryHandleDisconnection(string suffix)
    {
        if (handledDisconnection)
        {
            return;
        }

        handledDisconnection = true;
        Debug.Log("Detected disconnection " + suffix);
        HandleDisconnection();
    }
}
