using DarkRift;
using DarkRift.Server;
using UnityEngine;

public class ClientConnection
{
    public string Name { get; }
    public IClient Client { get; }
    public Room Room { get; set; }
    public ServerPlayer Player { get; set; }

    public ClientConnection(IClient client , LoginRequestData data)
    {
        Client = client;
        Name = data.Name;

        Client.MessageReceived += OnMessage;
        
        using (Message m = Message.Create((ushort)Tags.LoginRequestAccepted, new LoginInfoData(client.ID, new LobbyInfoData(RoomManager.Instance.GetRoomDataList()))))
        {
            client.SendMessage(m, SendMode.Reliable);
        }
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        IClient client = (IClient)sender;
        using (Message message = e.GetMessage())
        {
            switch ((Tags)message.Tag)
            {
                case Tags.LobbyJoinRoomRequest:
                    RoomManager.Instance.TryJoinRoom(client, message.Deserialize<JoinRoomRequest>());
                    break;
                case Tags.GameJoinRequest:
                    Room.JoinPlayerToGame(this);
                    break;
                case Tags.GamePlayerInput:
                    Player.RecieveInput(message.Deserialize<PlayerInputData>());
                    break;
            }
        }
    }

    public void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
    {
        if (Room != null)
        {
            Room.RemovePlayerFromRoom(this);
        }

        ServerManager.Instance.Players.Remove(Client.ID);
        e.Client.MessageReceived -= OnMessage;
    }
}
