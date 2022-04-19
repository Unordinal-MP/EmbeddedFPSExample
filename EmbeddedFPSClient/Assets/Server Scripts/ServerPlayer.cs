using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using UnityEngine;

[RequireComponent(typeof(PlayerLogic))]
public class ServerPlayer : MonoBehaviour
{
    private ClientConnection clientConnection;
    private Room room;

    private PlayerStateData currentPlayerStateData;

    private readonly Buffer<PlayerInputData> inputBuffer = new Buffer<PlayerInputData>(PlayerInputMessage.MaxStackedInputs, PlayerInputMessage.MaxStackedInputs);

    private int health;

    public PlayerLogic PlayerLogic { get; private set; }
    public uint InputTick { get; private set; }
    public IClient Client { get; private set; }
    public PlayerStateData CurrentPlayerStateData => currentPlayerStateData;
    public List<PlayerStateData> PlayerStateDataHistory { get; } = new List<PlayerStateData>();

    private PlayerInputData[] inputs;

    private uint highestSequenceNumber;

    public Vector3 SpawnPosition { get; private set; }

    private void Awake()
    {
        PlayerLogic = GetComponent<PlayerLogic>();
    }

    public void Initialize(ClientConnection clientConnection)
    {
        this.clientConnection = clientConnection;
        this.clientConnection.Player = this;

        room = clientConnection.Room;
        Client = clientConnection.Client;
        
        InputTick = room.ServerTick;
        highestSequenceNumber = room.ServerTick;

        Respawn();

        var playerSpawnData = room.GetSpawnDataForAllPlayers();
        
        using Message m = Message.Create((ushort)Tags.GameStartDataResponse, new GameStartData(playerSpawnData, room.ServerTick));
        
        Client.SendMessage(m, SendMode.Reliable);
    }

    private void Respawn()
    {
        health = 100;

        SpawnManager.Instance.GetSpawnpoint(this, room.Players, out Vector3 position, out Quaternion rotation);

        SpawnPosition = position;

        transform.SetPositionAndRotation(position, rotation);

        currentPlayerStateData = new PlayerStateData(Client.ID, default, position, rotation, CollisionFlags.None);
    }

    public void RecieveInput(PlayerInputData input)
    {
        uint target = input.SequenceNumber;

        if (target > highestSequenceNumber + 1)
        {
            //insert potential duplicates on purpose, these will be replaced by real data arriving before playback
            for (uint sequenceNumber = highestSequenceNumber + 1; sequenceNumber <= target; ++sequenceNumber)
            {
                input.SequenceNumber = sequenceNumber;
                inputBuffer.Add(input, sequenceNumber);
            }
        }
        else
        {
            inputBuffer.Add(input, target);
        }

        highestSequenceNumber = Math.Max(highestSequenceNumber, target);
    }

    public void TakeDamage(int value, ServerPlayer shooter)
    {
        health -= value;
        if (health <= 0)
        {
            Die(shooter);
        }

        room.UpdatePlayerHealth(this, (byte)health);
    }

    private void Die(ServerPlayer shooter)
    {
        room.UpdateKill(shooter, this);

        Respawn();
    }

    //private uint nextseq;

    public void PlayerPreUpdate()
    {
        inputs = inputBuffer.Get();
        for (int i = 0; i < inputs.Length; i++)
        {
            ref PlayerInputData input = ref inputs[i];

            /*if (input.SequenceNumber != nextseq)
            {
                Debug.Log("expected input " + nextseq + " but got " + input.SequenceNumber);
            }
            nextseq = input.SequenceNumber + 1;*/

            if (input.KeyInputs[(int)PlayerAction.Fire])
            {
                room.PerformShootRayCast(input.Time, this);
                return;
            }
        }
    }

    public PlayerStateData PlayerUpdate()
    {
        if (inputs.Length > 0)
        {
            PlayerInputData input = inputs.First();
            InputTick++;

            for (int i = 1; i < inputs.Length; i++)
            {
                InputTick++;
                for (int j = 0; j < input.KeyInputs.Length; j++)
                {
                    input.KeyInputs[j] = input.KeyInputs[j] || inputs[i].KeyInputs[j];
                }

                input.LookDirection = inputs[i].LookDirection;
            }

            currentPlayerStateData = PlayerLogic.GetNextFrameData(input, currentPlayerStateData);
        }
        
        PlayerStateDataHistory.Add(currentPlayerStateData);
        if (PlayerStateDataHistory.Count > 10)
        {
            PlayerStateDataHistory.RemoveAt(0);
        }

        transform.SetPositionAndRotation(currentPlayerStateData.Position, currentPlayerStateData.Rotation);
        
        return currentPlayerStateData;
    }

    public PlayerSpawnData GetPlayerSpawnData()
    {
        return new PlayerSpawnData(Client.ID, clientConnection.Name, transform.position, transform.rotation);
    }
}
