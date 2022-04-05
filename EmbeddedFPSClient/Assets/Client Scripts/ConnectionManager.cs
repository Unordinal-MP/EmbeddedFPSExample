using System;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

[RequireComponent(typeof(UnityClient))]
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    [Header("Settings")]
    public string Hostname;
    [SerializeField]
    private int port;
    private readonly int udport = 4297;

    [Header("References")]
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
        Client = GetComponent<UnityClient>();
    }

    public void Connect(string hostname)
    {
        if (hostname == "localhost")
        {
            hostname = "127.0.0.1";
        }

        clientConnection = new LiteNetNetworkClientConnection(hostname, (ushort)port);
        
        Client.Client.ConnectInBackground(clientConnection, (e) => Client.Dispatcher.InvokeAsync(() => ConnectCallback(e)));
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
