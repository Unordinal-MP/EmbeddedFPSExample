using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using UnityEngine;

[RequireComponent(typeof(XmlUnityServer))]
public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }
    
    private DarkRiftServer server;

    public Dictionary<ushort, ClientConnection> Players = new Dictionary<ushort, ClientConnection>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        XmlUnityServer xmlServer = GetComponent<XmlUnityServer>();
        server = xmlServer.Server;
        server.ClientManager.ClientConnected += OnClientConnected;
        server.ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        server.ClientManager.ClientConnected -= OnClientConnected;
        server.ClientManager.ClientDisconnected -= OnClientDisconnected;
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        IClient client = e.Client;
        if (Players.TryGetValue(client.ID, out ClientConnection p))
        {
            p.OnClientDisconnect(sender, e);
        }
        else
        {
            e.Client.MessageReceived -= OnMessage;
        }
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessage;
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"Received message {(Tags)e.Tag}");

        IClient client = (IClient)sender;

        using Message message = e.GetMessage();

        switch ((Tags)e.Tag)
        {
            case Tags.LoginRequest:
                OnClientLogin(client, message.Deserialize<LoginRequestData>());
                break;
        }
    }

    private void OnClientLogin(IClient client, LoginRequestData data)
    {
        //not an example of a high quality word filter implementation
        if (data.Name.ToLower().Contains("hitler"))
        {
            Debug.Log("Player denied");

            using Message message = Message.CreateEmpty((ushort)Tags.LoginRequestDenied);
            
            client.SendMessage(message, SendMode.Reliable);

            return;
        }

        // In the future the ClientConnection will handle its messages
        client.MessageReceived -= OnMessage;

        var connection = new ClientConnection(client, data);
        Players.Add(client.ID, connection);
    }
}
