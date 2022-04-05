using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    Dictionary<string, Room> rooms = new Dictionary<string, Room>();

    public static RoomManager Instance;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject roomPrefab;

    void Awake()
    {
        // loading map
        SceneManager.LoadScene("Map1", LoadSceneMode.Additive);

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
        CreateRoom("Main",16);
    }

    public RoomData[] GetRoomDataList()
    {
        RoomData[] data = new RoomData[rooms.Count];
        int i = 0;
        foreach (KeyValuePair<string, Room> kvp in rooms)
        {
            Room r = kvp.Value;
            data[i] = new RoomData(r.Name, (byte) r.ClientConnections.Count, r.MaxSlots);
            i++;
        }
        return data;
    }

    public void TryJoinRoom(IClient client, JoinRoomRequest data)
    {
        bool canJoin = ServerManager.Instance.Players.TryGetValue(client.ID, out var clientConnection);

        if (!rooms.TryGetValue(data.RoomName, out var room))
        {
            canJoin = false;
        }
        else if (room.ClientConnections.Count >= room.MaxSlots)
        {
            canJoin = false;
        }

        if (canJoin)
        {
            room.AddPlayerToRoom(clientConnection);
        }
        else
        {
            using (Message m = Message.Create((ushort)Tags.LobbyJoinRoomDenied, new LobbyInfoData(GetRoomDataList())))
            {
                client.SendMessage(m, SendMode.Reliable);
            }
        }
    }

    public void CreateRoom(string roomName, byte maxSlots)
    {
        GameObject go = Instantiate(roomPrefab);
        Room room = go.GetComponent<Room>();
        room.Initialize(roomName, maxSlots);
        rooms.Add(roomName, room);
    }

    public void RemoveRoom(string roomName)
    {
        Room r = rooms[roomName];
        r.Close();
        rooms.Remove(roomName);
    }

}
