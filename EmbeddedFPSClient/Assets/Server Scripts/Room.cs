using System.Collections.Generic;
using DarkRift;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room : MonoBehaviour
{
    private readonly List<ServerPlayer> serverPlayers = new List<ServerPlayer>();

    private readonly List<PlayerStateData> playerStateData = new List<PlayerStateData>(4);
    private readonly List<PlayerSpawnData> playerSpawnData = new List<PlayerSpawnData>(4);
    private readonly List<PlayerDespawnData> playerDespawnData = new List<PlayerDespawnData>(4);
    private readonly List<PlayerHealthUpdateData> healthUpdateData = new List<PlayerHealthUpdateData>(4);

    public IReadOnlyList<ServerPlayer> Players => serverPlayers;


    [Header("Public Fields")]
    public string Name;
    public List<ClientConnection> ClientConnections = new List<ClientConnection>();
    public byte MaxSlots;
    public uint ServerTick { get; private set; }
    public LayerMask hitLayers;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject playerPrefab;

    void FixedUpdate()
    {
        ServerTick++;

        foreach (ServerPlayer player in serverPlayers)
        {
            player.PlayerPreUpdate();
        }

        for (var i = 0; i < serverPlayers.Count; i++)
        {
            ServerPlayer player = serverPlayers[i];
            playerStateData[i] = player.PlayerUpdate();
        }

        // Send update message to all players.
        PlayerStateData[] playerStateDataArray = playerStateData.ToArray();
        PlayerSpawnData[] playerSpawnDataArray = playerSpawnData.ToArray();
        PlayerDespawnData[] playerDespawnDataArray = playerDespawnData.ToArray();
        PlayerHealthUpdateData[] healthUpdateDataArray = healthUpdateData.ToArray();
        foreach (ServerPlayer player in serverPlayers)
        {
            using Message message = Message.Create((ushort)Tags.GameUpdate, new GameUpdateData(player.InputTick, playerStateDataArray, playerSpawnDataArray, playerDespawnDataArray, healthUpdateDataArray));
            
            player.Client.SendMessage(message, SendMode.Unreliable);
        }
        
        playerSpawnData.Clear();
        playerDespawnData.Clear();
        healthUpdateData.Clear();
    }


    public void Initialize(string name, byte maxslots)
    {
        Name = name;
        MaxSlots = maxslots;

        CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        Scene scene = SceneManager.CreateScene("Room_" + name, csp);

        SceneManager.MoveGameObjectToScene(gameObject, scene);
    }

    public void AddPlayerToRoom(ClientConnection clientConnection)
    {
        ClientConnections.Add(clientConnection);
        clientConnection.Room = this;
        using (Message message = Message.CreateEmpty((ushort)Tags.LobbyJoinRoomAccepted))
        {
            clientConnection.Client.SendMessage(message, SendMode.Reliable);
        }
    }

    public void RemovePlayerFromRoom(ClientConnection clientConnection)
    {
        Destroy(clientConnection.Player.gameObject);
        playerDespawnData.Add(new PlayerDespawnData(clientConnection.Client.ID));
        ClientConnections.Remove(clientConnection);
        serverPlayers.Remove(clientConnection.Player);
        clientConnection.Room = null;
    }

    public void JoinPlayerToGame(ClientConnection clientConnection)
    {
        GameObject go = Instantiate(playerPrefab);

        ServerPlayer player = go.GetComponent<ServerPlayer>();
        serverPlayers.Add(player);

        player.Initialize(clientConnection);

        playerStateData.Add(default);
        playerSpawnData.Add(player.GetPlayerSpawnData());
    }
    
    public void Close()
    {
        foreach(ClientConnection p in ClientConnections)
        {
            RemovePlayerFromRoom(p);
        }
        Destroy(gameObject);
    }

    public void PerformShootRayCast(uint frame, ServerPlayer shooter)
    {
        int dif = (int) (ServerTick - 1 - frame);
        if (dif < 0)
        {
            return; //TODO: how can this occur? better checking there than out of bounds
        }

        // Get the position of the ray
        Vector3 startPosition;
        Vector3 direction;

        if (shooter.PlayerStateDataHistory.Count > dif)
        {
            startPosition = shooter.PlayerStateDataHistory[dif].Position;
            direction = shooter.PlayerStateDataHistory[dif].Input.LookDirection * Vector3.forward;
        }
        else
        {
            startPosition = shooter.CurrentPlayerStateData.Position;
            direction = shooter.CurrentPlayerStateData.Input.LookDirection * Vector3.forward;
        }

        startPosition += direction * 0.6f + Vector3.up * 1.8f;

        const float debugLifetimeSeconds = 300;

        //set all players back in time
        foreach (ServerPlayer player in serverPlayers)
        {
            if (player.PlayerStateDataHistory.Count > dif)
            {
                player.PlayerLogic.CharacterController.enabled = false;
                player.transform.position = player.PlayerStateDataHistory[dif].Position;
            }

            if (player != shooter)
            {
                Debug.DrawLine(player.transform.position + Vector3.down, player.transform.position + 200 * Vector3.up, Color.blue, debugLifetimeSeconds);
            }
        }

        const float rayDistance = 200;
        RaycastHit hit;
        if (Physics.Raycast(startPosition, direction, out hit, rayDistance, hitLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.transform.CompareTag("Unit"))
            {
                hit.transform.GetComponent<ServerPlayer>().TakeDamage(5, shooter);
            }
        }
        else
        {
            hit.point = direction.normalized * rayDistance;
        }

        Debug.DrawLine(startPosition, hit.point, Color.red, debugLifetimeSeconds);

        // Set all players back.
        foreach (ServerPlayer player in serverPlayers)
        {
            player.transform.position = player.CurrentPlayerStateData.Position;
            player.PlayerLogic.CharacterController.enabled = true;
        }
    }

    public PlayerSpawnData[] GetSpawnDataForAllPlayers()
    {
        PlayerSpawnData[] fullPlayerSpawnData = new PlayerSpawnData[serverPlayers.Count];
        for (int i = 0; i < serverPlayers.Count; i++)
        {
            ServerPlayer p = serverPlayers[i];
            fullPlayerSpawnData[i] = p.GetPlayerSpawnData();
        }

        return fullPlayerSpawnData;
    }

    public void UpdatePlayerHealth(ServerPlayer player, byte health)
    {
        healthUpdateData.Add(new PlayerHealthUpdateData(player.Client.ID, health));
    }

    public void UpdateKill(ServerPlayer shooter, ServerPlayer victim)
    {
        var killData = new KillData()
        {
            Killer = shooter.Client.ID,
            Victim = victim.Client.ID,
        };

        using Message message = Message.Create((ushort)Tags.Kill, killData);

        foreach (ServerPlayer player in serverPlayers)
        {
            player.Client.SendMessage(message, SendMode.Reliable);
        }
    }
}
