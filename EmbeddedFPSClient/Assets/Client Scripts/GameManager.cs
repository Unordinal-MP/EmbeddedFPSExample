using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Client;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Dictionary<ushort, ClientPlayer> players;
    private Buffer<UnreliableGameUpdateData> gameUpdateDataBuffer;

    [Header("Prefabs")]
    public GameObject PlayerPrefab;

    public IEnumerable<ClientPlayer> Players => players.Values;
    public ClientPlayer OwnPlayer { get; private set; }

    public uint ClientTick { get; private set; }
    public uint LastReceivedServerTick { get; private set; }

    private void Awake()
    {
        if (ServerManager.Instance == null)
        {
            //is is client
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            //if is server
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (ServerManager.Instance == null)
        {
            Instance = null;
            ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
        }
    }

    private void Start()
    {
        players = new Dictionary<ushort, ClientPlayer>();

        gameUpdateDataBuffer = new Buffer<UnreliableGameUpdateData>(1, 1);

        ConnectionManager.Instance.Client.MessageReceived += OnMessage;

        Debug.Log("Starting GameManager");

        using Message message = Message.CreateEmpty((ushort)Tags.GameJoinRequest);
        
        ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        using Message message = e.GetMessage();

        ClientStats.Instance.MessagesIn.AddNow();
        ClientStats.Instance.BytesIn.AddNow(message.DataLength);

        switch ((Tags)message.Tag)
        {
            case Tags.GameStartDataResponse:
                OnGameJoinAccept(message.Deserialize<GameStartData>());
                break;
            case Tags.UnreliableGameUpdate:
                OnGameUpdate(message.Deserialize<UnreliableGameUpdateData>());
                break;
            case Tags.ReliableGameUpdate:
                OnReliableGameUpdate(message.Deserialize<ReliableGameUpdateData>());
                break;
        }
    }

    private void OnKill(PlayerKillData kill)
    {
        ClientPlayer killer = players[kill.Killer];
        ClientPlayer victim = players[kill.Victim];

        killer.Kills += 1;
        victim.Deaths += 1;

        //jury is out on whether we should manipulate health here
    }

    private void OnGameJoinAccept(GameStartData gameStartData)
    {
        LastReceivedServerTick = gameStartData.OnJoinServerTick;
        ClientTick = gameStartData.OnJoinServerTick;
        foreach (PlayerSpawnData playerSpawnData in gameStartData.Players)
        {
            SpawnPlayer(playerSpawnData, "OnGameJoinAccept");
        }
    }

    private void OnGameUpdate(UnreliableGameUpdateData gameUpdateData)
    {
        gameUpdateDataBuffer.Add(gameUpdateData, gameUpdateData.Frame);
    }

    private void SpawnPlayer(PlayerSpawnData playerSpawnData, string src)
    {
        Debug.Log("Will spawn player " + playerSpawnData.PlayerId + " from " + src);

        GameObject go = Instantiate(PlayerPrefab, playerSpawnData.Position, playerSpawnData.Rotation);
        var controller = GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.camera.transform.rotation = playerSpawnData.Rotation;
        }

        ClientPlayer player = go.GetComponent<ClientPlayer>();
        player.Initialize(playerSpawnData.PlayerId, playerSpawnData.Name);
        players.Add(playerSpawnData.PlayerId, player);

        if (player.IsOwn)
        {
            OwnPlayer = player;
        }
    }

    private void FixedUpdate()
    {
        ClientTick++;
        UnreliableGameUpdateData[] receivedGameUpdateData = gameUpdateDataBuffer.Get();
        foreach (UnreliableGameUpdateData data in receivedGameUpdateData)
        {
            UpdateClientGameState(data);
        }
    }

    private void OnReliableGameUpdate(ReliableGameUpdateData gameUpdateData)
    {
        foreach (PlayerSpawnData data in gameUpdateData.SpawnDataData)
        {
            if (data.PlayerId != ConnectionManager.Instance.OwnPlayerId)
            {
                SpawnPlayer(data, "OnReliableGameUpdate");
            }
        }

        foreach (PlayerDespawnData data in gameUpdateData.DespawnDataData)
        {
            Destroy(players[data.PlayerId].gameObject);
            players.Remove(data.PlayerId);
        }

        foreach (PlayerKillData data in gameUpdateData.KillDataData)
        {
            OnKill(data);
        }
    }

    private void UpdateClientGameState(UnreliableGameUpdateData gameUpdateData)
    {
        LastReceivedServerTick = gameUpdateData.Frame;
        
        foreach (PlayerStateData data in gameUpdateData.UpdateData)
        {
            if (players.TryGetValue(data.PlayerId, out ClientPlayer p))
            {
                p.OnServerDataUpdate(data);
            }
        }

        foreach (PlayerHealthUpdateData data in gameUpdateData.HealthData)
        {
            if (players.TryGetValue(data.PlayerId, out ClientPlayer p))
            {
                p.SetHealth(data.Value);
            }
        }
    }
}
