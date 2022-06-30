using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Client;
using UnityEngine;
using LootLocker.Requests;

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

    //the following circular buffer responsible for duplicate detection is used like a hash set with limited spaces
    //the way we use it requires that the sequence number 0 is never used, so it can represent empty slots
    private CircularBuffer<uint> receivedGameUpdates;

    private UnreliableGameUpdateData? previousUpdate;
    private Queue<UnreliableGameUpdateData> updateQueue;

    public int UpdateQueueLength => updateQueue.Count;

    public string OnlineLeaderboardString;

    private void Awake()
    {
        if (ServerManager.Instance == null)
        {
            //is is client
            if (Instance != null)
            {
                Debug.Log("Destroying redundant GameManager");
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
            Debug.Log("Destroying GameManager because ServerManager already exists");
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
        Debug.Log("Starting GameManager");

        players = new Dictionary<ushort, ClientPlayer>();
        gameUpdateDataBuffer = new Buffer<UnreliableGameUpdateData>(1, 1);
        receivedGameUpdates = new CircularBuffer<uint>(40);
        updateQueue = new Queue<UnreliableGameUpdateData>();

        ConnectionManager.Instance.Client.MessageReceived += OnMessage;

        Debug.Log("Starting GameManager");

        using Message message = Message.CreateEmpty((ushort)Tags.GameJoinRequest);
        
        ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);

        Invoke(nameof(InterpolationFrame), Constants.TickInterval);
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
                OnUnreliableGameUpdate(message.Deserialize<UnreliableGameUpdateData>());
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
        RegisterKillOnOnlineLeaderboard(killer);
        victim.IsDead = true;  
    }

    private void OnRespawn(PlayerKillData kill)
    {
        ClientPlayer victim = players[kill.Victim];
        RegisterDeathOnOnlineLeaderboard(victim);

        victim.IsDead = false;
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

    private void OnUnreliableGameUpdate(UnreliableGameUpdateData gameUpdateData)
    {
        if (receivedGameUpdates.Contains(gameUpdateData.Frame))
            return;

        receivedGameUpdates.Add(gameUpdateData.Frame);
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
        ClientTick += 1;

        UnreliableGameUpdateData[] receivedGameUpdateData = gameUpdateDataBuffer.Get();
        foreach (UnreliableGameUpdateData data in receivedGameUpdateData)
        {
            if (previousUpdate == null)
                previousUpdate = data;
            if (data.Frame < previousUpdate.Value.Frame)
                continue; //drop, buffer should have sorted it nominally speaking

            int gap = (int)(data.Frame - previousUpdate.Value.Frame);
            if (gap > 1)
            {
                //create interpolated fake frames for other players
                for (int i = 1; i < gap; ++i)
                {
                    float interpolationFactor = (float)i / gap;
                    var fake = (UnreliableGameUpdateData)data.Clone();
                    fake.Interpolated = true;
                    fake.Frame = previousUpdate.Value.Frame + (uint)i;

                    PlayerStateData[] fakePlayerState = fake.UpdateData;
                    for (int k = 0; k < fakePlayerState.Length; ++k)
                    {
                        ref PlayerStateData state = ref fakePlayerState[k];
                        int player = state.PlayerId;

                        PlayerStateData source = previousUpdate.Value.UpdateData.SingleOrDefault(x => x.PlayerId == player);
                        if (source.Input.SequenceNumber == 0)
                            continue;

                        ref PlayerStateData destination = ref data.UpdateData[k];

                        state.Position = Vector3.Lerp(source.Position, destination.Position, interpolationFactor);
                        state.Rotation = Quaternion.Slerp(source.Rotation, destination.Rotation, interpolationFactor);
                    }

                    updateQueue.Enqueue(fake);
                }
            }

            updateQueue.Enqueue(data);

            previousUpdate = data;

            //always update on player because reconciliation handles differently
            LastReceivedServerTick = data.Frame;
            PlayerStateData ownPlayerData = data.UpdateData.SingleOrDefault(x => x.PlayerId == ConnectionManager.Instance.OwnPlayerId);
            if (ownPlayerData.Input.SequenceNumber != 0) //avoid default initialized result as per above line
                OwnPlayer.OnServerDataUpdate(ownPlayerData);
        }
    }

    void InterpolationFrame()
    {
        if (updateQueue.Count > 0)
        {
            UpdateClientGameState(updateQueue.Dequeue());
        }

        Invoke(nameof(InterpolationFrame), updateQueue.Count > 2 ? 0.97f * Constants.TickInterval : Constants.TickInterval);
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
            if (data.IsRespawn)
                OnRespawn(data);
            else
                OnKill(data);
        }
    }

    private void UpdateClientGameState(UnreliableGameUpdateData gameUpdateData)
    {
        foreach (PlayerStateData data in gameUpdateData.UpdateData)
        {
            if (players.TryGetValue(data.PlayerId, out ClientPlayer player))
            {
                if (player.IsOwn)
                    continue;

                player.OnServerDataUpdate(data);
            }
        }

        if (!gameUpdateData.Interpolated)
        {
            foreach (PlayerHealthUpdateData data in gameUpdateData.HealthData)
            {
                if (players.TryGetValue(data.PlayerId, out ClientPlayer player))
                {
                    player.SetHealth(data.Value);
                }
            }
        }
    }

    private class LeaderboardEntry
    {
        private int rank;

        private string playerName;

        public List<float> scores { get; private set; }

        public LeaderboardEntry(int rank, string playername, params float[] scores)
        {
            this.rank = rank;
            this.playerName = playername;
            this.scores = new List<float>(scores);
        }

        public string GetAsText()
        {
            string _str = $"{rank}. {playerName}";

            for(int i = 0; i < scores.Count; i++)
            {
                _str += $" - {scores[i].ToString("0.0")}";
            }

            return _str;
        }
    }

    private void UpdateOnlineLeaderboard()
    {
        Dictionary<string, LeaderboardEntry> _entries = new Dictionary<string, LeaderboardEntry>();

        // Update and format leaderboard string
        LootLockerSDKManager.GetScoreList(4416, 10, (scoreResponse) =>
        {
            if (scoreResponse.success)
            {
                for (int i = 0; i < scoreResponse.items.Length; i++)
                {
                    var currentEntry = scoreResponse.items[i];

                    var _entry = new LeaderboardEntry(currentEntry.rank, currentEntry.player.name, currentEntry.score, 0, currentEntry.score);

                    _entries.Add(currentEntry.player.name, _entry);
                }

                LootLockerSDKManager.GetScoreList(4415, 10, (scoreResponse) =>
                {
                    if (scoreResponse.success)
                    {
                        for (int i = 0; i < scoreResponse.items.Length; i++)
                        {
                            LootLockerLeaderboardMember currentEntry = scoreResponse.items[i];

                            var _entry = _entries.ContainsKey(currentEntry.player.name) ? _entries[currentEntry.player.name] : null;

                            if (_entry == null)
                                continue;

                            _entry.scores[1] = currentEntry.score;
                            _entry.scores[2] = _entry.scores[1] > 0 ? _entry.scores[0] / _entry.scores[1] : _entry.scores[0]; 
                        }

                        OnlineLeaderboardString = string.Join("\n", _entries.Values.ToList().ConvertAll(_item => _item.GetAsText()));
                    }
                });
            }
        });
    }

    private void RegisterKillOnOnlineLeaderboard(ClientPlayer killer)
    {
        // Upload current kills to leaderboard
        if (killer == OwnPlayer)
        {
            LootLockerSDKManager.SubmitScore(PlayerPrefs.GetString("LootLockerGuestPlayerID"), killer.Kills, 4416, (response) =>

            {
                if (response.success)
                {
                    Debug.Log("Succesfully uploaded kill score");
                    UpdateOnlineLeaderboard();
                }
                else
                {
                    Debug.Log("Failed to upload kill score:" + response.Error);
                }
            });
        }
        else
        {
           UpdateOnlineLeaderboard();
        }
    }


    private void RegisterDeathOnOnlineLeaderboard(ClientPlayer killer)
    {
        // Upload current kills to leaderboard
        if (killer == OwnPlayer)
        {
            LootLockerSDKManager.SubmitScore(PlayerPrefs.GetString("LootLockerGuestPlayerID"), killer.Deaths, 4415, (response) =>

            {
                if (response.success)
               {
                   Debug.Log("Succesfully uploaded Death score");
                   UpdateOnlineLeaderboard();
               }
               else
               {
                   Debug.Log("Failed to upload Death score:" + response.Error);
               }
           }) ;
        }
        else
        {
            UpdateOnlineLeaderboard();
        }
    }
    


    private void EndLootLockerSession()
    {
        LootLockerSessionRequest newSession = new LootLockerSessionRequest();
        LootLocker.LootLockerAPIManager.EndSession(newSession, (response) =>
        {
            if (response.success)
            {
                PlayerPrefs.DeleteKey("LootLockerGuestPlayerID");
                PlayerPrefs.DeleteKey("PlayerID");
                PlayerPrefs.DeleteKey("PlayerName");

                Debug.Log("Ended lootlocker session.");
            }
            else
            {
                Debug.Log("Could not end session, " + response.Error);
            }
        });
    }

    private void OnApplicationQuit()
    {
        EndLootLockerSession();
    }
}
