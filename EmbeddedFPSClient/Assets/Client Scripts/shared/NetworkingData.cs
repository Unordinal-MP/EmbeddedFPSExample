using DarkRift;
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
    GameUpdate = 202,
    GamePlayerInput = 203,
    
    Kill = 204, //separate reliable message because death has implications and you need notice
}

public enum PlayerAction
{
    Jump,
    Sprint,
    Fire,
    Grounded,
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
    public ushort Id;
    public string Name;
    public Vector3 Position;
    public Quaternion Rotation;

    public PlayerSpawnData(ushort id, string name, Vector3 position, Quaternion rotation)
    {
        Id = id;
        Name = name;
        Position = position;
        Rotation = rotation;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Id = e.Reader.ReadUInt16();
        Name = e.Reader.ReadString();

        Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        Rotation = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Id);
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
    public ushort Id;

    public PlayerDespawnData(ushort id)
    {
        Id = id;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Id = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Id);
    }
}

public struct PlayerStateData : IDarkRiftSerializable
{

    public PlayerStateData(ushort id, PlayerInputData input, float gravity, Vector3 position, Quaternion rotation)
    {
        Id = id;
        Input = input;
        Position = position;
        Rotation = rotation;
        Gravity = gravity;
    }

    public ushort Id;
    public PlayerInputData Input;
    public Vector3 Position;
    public Quaternion Rotation;
    public float Gravity;

    public void Deserialize(DeserializeEvent e)
    {
        Input = e.Reader.ReadSerializable<PlayerInputData>();
        Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        Rotation = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        Id = e.Reader.ReadUInt16();
        Gravity = e.Reader.ReadSingle();
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
        e.Writer.Write(Id);
        e.Writer.Write(Gravity);
    }
}


public struct GameUpdateData : IDarkRiftSerializable
{
    public uint Frame;
    public PlayerSpawnData[] SpawnDataData;
    public PlayerDespawnData[] DespawnDataData;
    public PlayerStateData[] UpdateData;
    public PlayerHealthUpdateData[] HealthData;

    public GameUpdateData(uint frame, PlayerStateData[] updateData, PlayerSpawnData[] spawnData, PlayerDespawnData[] despawnData, PlayerHealthUpdateData[] healthData)
    {
        Frame = frame;
        UpdateData = updateData;
        DespawnDataData = despawnData;
        SpawnDataData = spawnData;
        HealthData = healthData;
    }
    public void Deserialize(DeserializeEvent e)
    {
        Frame = e.Reader.ReadUInt32();
        SpawnDataData = e.Reader.ReadSerializables<PlayerSpawnData>();
        DespawnDataData = e.Reader.ReadSerializables<PlayerDespawnData>();
        UpdateData = e.Reader.ReadSerializables<PlayerStateData>();
        HealthData = e.Reader.ReadSerializables<PlayerHealthUpdateData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Frame);
        e.Writer.Write(SpawnDataData);
        e.Writer.Write(DespawnDataData);
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

public struct KillData : IDarkRiftSerializable
{
    public ushort Killer;
    public ushort Victim;

    public KillData(ushort killer, ushort victim)
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

public struct PlayerInputData : IDarkRiftSerializable
{
    public bool[] Keyinputs; //indexed by PlayerAction
    public Quaternion LookDirection;
    public uint Time;
    public uint SequenceNumber;

    public bool HasAction(PlayerAction action)
    {
        return Keyinputs[(int)action];
    }

    public float horizontal => HasAction(PlayerAction.Left) ? -1 : (HasAction(PlayerAction.Right) ? 1 : 0);

    public float vertical => HasAction(PlayerAction.Back) ? -1 : (HasAction(PlayerAction.Forward) ? 1 : 0);

    public bool isJumping => HasAction(PlayerAction.Jump);

    public bool isSprinting => HasAction(PlayerAction.Sprint);

    public bool isShooting => HasAction(PlayerAction.Fire);

    public bool isGrounded => HasAction(PlayerAction.Grounded);

    public bool isAiming => HasAction(PlayerAction.Aim);

    public bool isReloading => HasAction(PlayerAction.Reload);

    public bool isSwitchingWeapon => HasAction(PlayerAction.SwitchWeapon);

    public bool isInspecting => HasAction(PlayerAction.Inspect);

    public PlayerInputData(bool[] keyInputs, Quaternion lookdirection, uint time, uint sequenceNumber)
    {
        Keyinputs = keyInputs;
        LookDirection = lookdirection;
        Time = time;
        SequenceNumber = sequenceNumber;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Keyinputs = new bool[(int)PlayerAction.NumActions];
        for (int q = 0; q < (int)PlayerAction.NumActions; q++)
        {
            Keyinputs[q] = e.Reader.ReadBoolean();
        }

        LookDirection = new Quaternion(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());

        Time = e.Reader.ReadUInt32();
        SequenceNumber = e.Reader.ReadUInt32();
    }

    public void CheckKeyInputArray()
    {
        //TODO: would be useful to get rid of this
        if (Keyinputs == null)
        {
            Keyinputs = new bool[(int)PlayerAction.NumActions];
        }
    }

    public void Serialize(SerializeEvent e)
    {
        CheckKeyInputArray();

        for (int q = 0; q < (int)PlayerAction.NumActions; q++)
        {
            e.Writer.Write(Keyinputs[q]);
        }
        e.Writer.Write(LookDirection.x);
        e.Writer.Write(LookDirection.y);
        e.Writer.Write(LookDirection.z);
        e.Writer.Write(LookDirection.w);

        e.Writer.Write(Time);
        e.Writer.Write(SequenceNumber);
    }
}