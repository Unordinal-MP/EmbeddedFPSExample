using System;
using System.Net;
using System.Reflection;
using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    public string Hostname = "127.0.0.1";
    public int Port = 4296;

    [SerializeField]
    private LoginManager loginManager;

    public UnityClient Client { get; private set; }

    public ushort OwnPlayerId { get; set; }

    public LobbyInfoData LobbyInfoData { get; set; }

    private LiteNetNetworkClientConnection clientConnection;

    public delegate void OnConnectedDelegate();
    public event OnConnectedDelegate OnConnected;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        Client = gameObject.AddComponent<UnityClient>();
        Client.GetType().GetField("connectOnStart", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Client, false);
    }

    public void Connect(Action<Exception> onDone)
    {
        if (Hostname == "localhost")
        {
            Hostname = "127.0.0.1";
        }

        clientConnection = new LiteNetNetworkClientConnection(Hostname, (ushort)Port);
        //Client.ConnectInBackground(Hostname, Port, true, ex => Client.Dispatcher.InvokeAsync(() => { onDone(ex); ConnectCallback(ex); }));
        Client.Client.ConnectInBackground(clientConnection, ex => Client.Dispatcher.InvokeAsync(() => { onDone(ex); ConnectCallback(ex); }));
    }

    private void Update()
    {
        if (clientConnection != null)
        {
            clientConnection.Update();
        }
    }

    private void ConnectCallback(Exception exception)
    {
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            OnConnected?.Invoke();
        }
        else if (exception != null)
        {
            Debug.LogError("Unable to connect to server: " + exception.Message);
        }
        else
        {
            Debug.LogError("Unable to connect to server.");
        }
    }
}
