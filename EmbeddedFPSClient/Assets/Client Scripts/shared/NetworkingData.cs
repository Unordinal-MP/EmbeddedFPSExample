﻿using DarkRift;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Tags
{
    LoginRequest = 0,
    LoginRequestAccepted = 1,
    LoginRequestDenied = 2,

    LobbyJoinRoomRequest = 100,
    LobbyJoinRoomDenied = 101,
    LobbyJoinRoomAccepted = 102,

    GameJoinRequest = 200,
    GameStartDataResponse = 201,
    UnreliableGameUpdate = 202,
    GamePlayerInput = 203,
    ReliableGameUpdate = 204,
}

public enum PlayerAction
{
    Jump,
    Sprint,
    Fire,
    Aim,
    Reload,
    Inspect,
    SwitchWeapon,
    Forward,
    Left,
    Right,
    Back,

    NumActions
}

public struct LoginRequestData : IDarkRiftSerializable
{
    public string Name;

    public LoginRequestData(string name)
    {
        Name = name;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Name = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Name);
    }
}

public struct LoginInfoData : IDarkRiftSerializable
{
    public ushort Id;
    public LobbyInfoData Data;

    public LoginInfoData(ushort id, LobbyInfoData data)
    {
        Id = id;
        Data = data;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Id = e.Reader.ReadUInt16();
        Data = e.Reader.ReadSerializable<LobbyInfoData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Id);
        e.Writer.Write(Data);
    }
}

public struct LobbyInfoData : IDarkRiftSerializable
{
    public RoomData[] Rooms;

    public LobbyInfoData(RoomData[] rooms)
    {
        Rooms = rooms;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Rooms = e.Reader.ReadSerializables<RoomData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Rooms);
    }
}

public struct RoomData : IDarkRiftSerializable
{
    public string Name;
    public byte Slots;
    public byte MaxSlots;

    public RoomData(string name, byte slots, byte maxSlots)
    {
        Name = name;
        Slots = slots;
        MaxSlots = maxSlots;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Name = e.Reader.ReadString();
        Slots = e.Reader.ReadByte();
        MaxSlots = e.Reader.ReadByte();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Name);
        e.Writer.Write(Slots);
        e.Writer.Write(MaxSlots);
    }
}

public struct JoinRoomRequest : IDarkRiftSerializable
{
    public string RoomName;

    public JoinRoomRequest(string name)
    {
        RoomName = name;
    }

    public void Deserialize(DeserializeEvent e)
    {
        RoomName = e.Reader.ReadString();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(RoomName);
    }
}

public struct GameStartData : IDarkRiftSerializable
{
    public uint OnJoinServerTick;
    public PlayerSpawnData[] Players;

    public GameStartData(PlayerSpawnData[] players, uint serverTick)
    {
        Players = players;
        OnJoinServerTick = serverTick;
    }

    public void Deserialize(DeserializeEvent e)
    {
        OnJoinServerTick = e.Reader.ReadUInt32();
        Players = e.Reader.ReadSerializables<PlayerSpawnData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(OnJoinServerTick);
        e.Writer.Write(Players);
    }
}

public struct PlayerSpawnData : IDarkRiftSerializable
{
    public ushort PlayerId;
    public string Name;
    public Vector3 Position;
    public Quaternion Rotation;

    public PlayerSpawnData(ushort id, string name, Vector3 position, Quaternion rotation)
    {
        PlayerId = id;
        Name = name;
        Position = position;
        Rotation = rotation;
    }

    public void Deserialize(DeserializeEvent e)
    {
        PlayerId = e.Reader.ReadUInt16();
        Name = e.Reader.ReadString();

        Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        Rotation = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(PlayerId);
        e.Writer.Write(Name);

        e.Writer.Write(Position.x);
        e.Writer.Write(Position.y);
        e.Writer.Write(Position.z);

        e.Writer.Write(Rotation.x);
        e.Writer.Write(Rotation.y);
        e.Writer.Write(Rotation.z);
        e.Writer.Write(Rotation.w);
    }
}

public struct PlayerDespawnData : IDarkRiftSerializable
{
    public ushort PlayerId;

    public PlayerDespawnData(ushort id)
    {
        PlayerId = id;
    }

    public void Deserialize(DeserializeEvent e)
    {
        PlayerId = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(PlayerId);
    }
}

public struct PlayerStateData : IDarkRiftSerializable
{
    public PlayerStateData(ushort id, PlayerInputData input, Vector3 position, Quaternion rotation, CollisionFlags collisionFlags)
    {
        PlayerId = id;
        Input = input;
        Position = position;
        Rotation = rotation;
        LatestCollision = collisionFlags;
    }

    public ushort PlayerId;
    public PlayerInputData Input;
    public Vector3 Position;
    public Quaternion Rotation;

    //debug
    public CollisionFlags LatestCollision;

    public void Deserialize(DeserializeEvent e)
    {
        Input = e.Reader.ReadSerializable<PlayerInputData>();
        Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        Rotation = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        PlayerId = e.Reader.ReadUInt16();

        byte latestCollision = e.Reader.ReadByte();
        LatestCollision = (CollisionFlags)latestCollision;
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Input);

        e.Writer.Write(Position.x);
        e.Writer.Write(Position.y);
        e.Writer.Write(Position.z);
 
        e.Writer.Write(Rotation.x);
        e.Writer.Write(Rotation.y);
        e.Writer.Write(Rotation.z);
        e.Writer.Write(Rotation.w);

        e.Writer.Write(PlayerId);

        e.Writer.Write((byte)LatestCollision);
    }

    public override string ToString()
    {
        //for debug purposes
        string text = LatestCollision.ToString();
        text += " " + Position.ToString("F3");
        text += " " + Rotation.ToString("F1");

        return text;
    }
}

public struct ReliableGameUpdateData : IDarkRiftSerializable
{
    public PlayerSpawnData[] SpawnDataData;
    public PlayerDespawnData[] DespawnDataData;
    public PlayerKillData[] KillDataData;

    public ReliableGameUpdateData(PlayerSpawnData[] spawnData, PlayerDespawnData[] despawnData, PlayerKillData[] killData)
    {
        DespawnDataData = despawnData;
        SpawnDataData = spawnData;
        KillDataData = killData;
    }

    public void Deserialize(DeserializeEvent e)
    {
        SpawnDataData = e.Reader.ReadSerializables<PlayerSpawnData>();
        DespawnDataData = e.Reader.ReadSerializables<PlayerDespawnData>();
        KillDataData = e.Reader.ReadSerializables<PlayerKillData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(SpawnDataData);
        e.Writer.Write(DespawnDataData);
        e.Writer.Write(KillDataData);
    }
}

public struct UnreliableGameUpdateData : IDarkRiftSerializable, ICloneable
{
    public uint Frame;
    public bool Interpolated;
    public PlayerStateData[] UpdateData;
    public PlayerHealthUpdateData[] HealthData;

    public UnreliableGameUpdateData(uint frame, PlayerStateData[] updateData, PlayerHealthUpdateData[] healthData)
    {
        Frame = frame;
        Interpolated = false;
        UpdateData = updateData;
        HealthData = healthData;
    }

    public object Clone()
    {
        var clone = new UnreliableGameUpdateData();
        clone.UpdateData = (PlayerStateData[])UpdateData.Clone();
        clone.HealthData = (PlayerHealthUpdateData[])HealthData.Clone();
        return clone;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Frame = e.Reader.ReadUInt32();
        UpdateData = e.Reader.ReadSerializables<PlayerStateData>();
        HealthData = e.Reader.ReadSerializables<PlayerHealthUpdateData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Frame);
        e.Writer.Write(UpdateData);
        e.Writer.Write(HealthData);
    }
}

public struct PlayerHealthUpdateData : IDarkRiftSerializable
{
    public ushort PlayerId;
    public byte Value;

    public PlayerHealthUpdateData(ushort id, byte val)
    {
        PlayerId = id;
        Value = val;
    }

    public void Deserialize(DeserializeEvent e)
    {
        PlayerId = e.Reader.ReadUInt16();
        Value = e.Reader.ReadByte();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(PlayerId);
        e.Writer.Write(Value);
    }
}

public struct PlayerKillData : IDarkRiftSerializable
{
    public ushort Killer;
    public ushort Victim;

    public bool IsRespawn => Killer == Victim; //TODO: make respawn message

    public PlayerKillData(ushort killer, ushort victim)
    {
        Killer = killer;
        Victim = victim;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Killer = e.Reader.ReadUInt16();
        Victim = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Killer);
        e.Writer.Write(Victim);
    }
}

public struct PlayerInputMessage : IDarkRiftSerializable
{
    public const int MaxStackedInputs = 4;

    public PlayerInputData[] StackedInputs;

    public void Deserialize(DeserializeEvent e)
    {
        int length = e.Reader.ReadByte();
        length = System.Math.Min(MaxStackedInputs, length);
        if (StackedInputs == null || StackedInputs.Length != length)
            StackedInputs = new PlayerInputData[length];

        e.Reader.ReadSerializablesInto(StackedInputs, 0);
    }

    public void Serialize(SerializeEvent e)
    {
        if (StackedInputs.Length > MaxStackedInputs)
            throw new System.ArgumentOutOfRangeException(nameof(StackedInputs));

        e.Writer.Write((byte)StackedInputs.Length);
        e.Writer.Write(StackedInputs);
    }
}

public struct PlayerInputData : IDarkRiftSerializable
{
    public bool[] KeyInputs; //indexed by PlayerAction
    public Quaternion LookDirection;
    public uint Time;
    public uint SequenceNumber;

    public bool HasAction(PlayerAction action)
    {
        return KeyInputs[(int)action];
    }

    public PlayerInputData(bool[] keyInputs, Quaternion lookDirection, uint time, uint sequenceNumber)
    {
        KeyInputs = keyInputs;
        LookDirection = lookDirection;
        Time = time;
        SequenceNumber = sequenceNumber;
    }

    public void Deserialize(DeserializeEvent e)
    {
        KeyInputs = new bool[(int)PlayerAction.NumActions];
        for (int q = 0; q < (int)PlayerAction.NumActions; q++)
        {
            KeyInputs[q] = e.Reader.ReadBoolean();
        }

        LookDirection = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());

        Time = e.Reader.ReadUInt32();
        SequenceNumber = e.Reader.ReadUInt32();
    }

    public void CheckKeyInputArray()
    {
        //TODO: would be useful to get rid of this
        if (KeyInputs == null)
        {
            KeyInputs = new bool[(int)PlayerAction.NumActions];
        }
    }

    public void Serialize(SerializeEvent e)
    {
        CheckKeyInputArray();

        for (int q = 0; q < (int)PlayerAction.NumActions; q++)
        {
            e.Writer.Write(KeyInputs[q]);
        }

        e.Writer.Write(LookDirection.x);
        e.Writer.Write(LookDirection.y);
        e.Writer.Write(LookDirection.z);
        e.Writer.Write(LookDirection.w);

        e.Writer.Write(Time);
        e.Writer.Write(SequenceNumber);
    }
}