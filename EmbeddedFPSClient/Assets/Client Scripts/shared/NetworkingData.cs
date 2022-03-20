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
    
    ShotBullet = 204
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

    public PlayerSpawnData(ushort id, string name, Vector3 position)
    {
        Id = id;
        Name = name;
        Position = position;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Id = e.Reader.ReadUInt16();
        Name = e.Reader.ReadString();

        Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Id);
        e.Writer.Write(Name);

        e.Writer.Write(Position.x);
        e.Writer.Write(Position.y);
        e.Writer.Write(Position.z);
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
    /// <summary>
    /// Mainly for movement Inputs
    /// </summary>
    public float[] MovementInputs;
    /// <summary>
    /// KeyInputs are mostly for separate inputs such as 
    /// shoot, jump, sprint
    /// </summary>
    public bool[] Keyinputs;
    public Vector3 LookDirection;
    public uint Time;

    public ushort Id;
    public Vector3 Position;
    public float Gravity;

    public float horizontal { get => MovementInputs[0]; }

    public float vertical { get => MovementInputs[1]; }

    public bool isJumping { get => Keyinputs[0]; }

    public bool isSprinting { get => Keyinputs[1]; }

    public bool isShooting { get => Keyinputs[2]; }

    public bool isGrounded { get => Keyinputs[3]; }

    public bool isAiming { get => Keyinputs[4]; }

    public bool isReloading { get => Keyinputs[5]; }

    public bool isSwitchingWeapon { get => Keyinputs[7]; }

    public bool isInspecting { get => Keyinputs[6]; }

    public PlayerStateData(ushort id)
    {
        Id = id;
        Gravity = -9.81f;
        Position = Vector3.zero;
        LookDirection = Vector3.zero;

        MovementInputs = new float[2];
        Keyinputs = new bool[8];

        Time = 0;
    }

    public PlayerStateData(ushort id, float gravity, Vector3 position, Vector3 lookDirection, float[] movementInputs, bool[] keyInputs, uint time)
    {
        Id = id;
        Position = position;
        LookDirection = lookDirection;
        Gravity = gravity;

        MovementInputs = movementInputs;
        Keyinputs = keyInputs;
        Time = time;
    }

    public void Deserialize(DeserializeEvent e)
    {
        Position = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());
        LookDirection = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());

        Id = e.Reader.ReadUInt16();
        Gravity = e.Reader.ReadSingle();

        MovementInputs = new float[2];

        for (int i = 0; i < MovementInputs.Length; i++)
        {
            MovementInputs[i] = e.Reader.ReadSingle();
        }

        Keyinputs = new bool[8];

        for (int q = 0; q < Keyinputs.Length; q++)
        {
            Keyinputs[q] = e.Reader.ReadBoolean();
        }

        if (Keyinputs[3])
        {
            Time = e.Reader.ReadUInt32();
        }
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Position.x);
        e.Writer.Write(Position.y);
        e.Writer.Write(Position.z);
 
        e.Writer.Write(LookDirection.x);
        e.Writer.Write(LookDirection.y);
        e.Writer.Write(LookDirection.z);

        e.Writer.Write(Id);
        e.Writer.Write(Gravity);

        //e.Writer.Write(MovementInputs.Length);

        for(int i = 0; i < MovementInputs.Length; i++)
        {
            e.Writer.Write(MovementInputs[i]);
        }

        //e.Writer.Write(Keyinputs.Length);

        for (int q = 0; q < Keyinputs.Length; q++)
        {
            e.Writer.Write(Keyinputs[q]);
        }

        e.Writer.Write(Time);
    }
}


public struct GameUpdateData : IDarkRiftSerializable
{
    public uint Frame;
    public PlayerSpawnData[] SpawnDataData;
    public PlayerDespawnData[] DespawnDataData;
    public PlayerStateData[] PlayerStateData;
    public PlayerHealthUpdateData[] HealthData;

    public GameUpdateData(uint frame, PlayerStateData[] playerData, PlayerSpawnData[] spawnData, PlayerDespawnData[] despawnData, PlayerHealthUpdateData[] healthData)
    {
        Frame = frame;
        PlayerStateData = playerData;
        DespawnDataData = despawnData;
        SpawnDataData = spawnData;
        HealthData = healthData;
    }
    public void Deserialize(DeserializeEvent e)
    {
        Frame = e.Reader.ReadUInt32();
        SpawnDataData = e.Reader.ReadSerializables<PlayerSpawnData>();
        DespawnDataData = e.Reader.ReadSerializables<PlayerDespawnData>();
        PlayerStateData = e.Reader.ReadSerializables<PlayerStateData>();
        HealthData = e.Reader.ReadSerializables<PlayerHealthUpdateData>();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Frame);
        e.Writer.Write(SpawnDataData);
        e.Writer.Write(DespawnDataData);
        e.Writer.Write(PlayerStateData);
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

//[System.Serializable]
//public struct PlayerInputData : IDarkRiftSerializable
//{
//    /// <summary>
//    /// Mainly for movement Inputs
//    /// </summary>
//    public float[] MovementInputs;
//    /// <summary>
//    /// KeyInputs are mostly for separate inputs such as 
//    /// shoot, jump, sprint
//    /// </summary>
//    public bool[] Keyinputs; 
//    public Vector3 LookDirection;
//    public uint Time;

//    public PlayerInputData(float[] movementInputs, bool[] keyInputs, Vector3 lookdirection, uint time)
//    {
//        MovementInputs = movementInputs;
//        Keyinputs = keyInputs;
//        LookDirection = lookdirection;
//        Time = time;
//    }

//    public void Deserialize(DeserializeEvent e)
//    {
//        MovementInputs = new float[2];
//        MovementInputs[0] = e.Reader.ReadSingle();
//        MovementInputs[1] = e.Reader.ReadSingle();

//        Keyinputs = new bool[4];
//        for (int q = 0; q < 4; q++)
//        {
//            Keyinputs[q] = e.Reader.ReadBoolean();
//        }
//        LookDirection = new Vector3(e.Reader.ReadSingle(), e.Reader.ReadSingle(), 0);
//        if (Keyinputs[3])
//        {
//            Time = e.Reader.ReadUInt32();
//        }
//    }

//    public void Serialize(SerializeEvent e)
//    {
//        e.Writer.Write(MovementInputs[0]);
//        e.Writer.Write(MovementInputs[1]);

//        for (int q = 0; q < 4; q++)
//        {
//            e.Writer.Write(Keyinputs[q]);
//        }
//        e.Writer.Write(LookDirection.x);
//        e.Writer.Write(LookDirection.y);
//        e.Writer.Write(LookDirection.z);
//        e.Writer.Write(Time);
//    }
//}