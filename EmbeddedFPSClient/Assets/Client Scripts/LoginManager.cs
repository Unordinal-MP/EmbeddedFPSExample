using System;
using DarkRift;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unordinal.Discoverability;

public class LoginManager : MonoBehaviour
{
    public GameObject LoginWindow;
    public InputField NameInput;
    public InputField HostInput;
    public InputField PortInput;
    public Button SubmitLoginButton;
    public Button ManualConnectButton;
    public ServerBrowser ServerBrowser;
    private double lastServerRefresh;

    private void Start()
    {
        ConnectionManager.Instance.OnConnected += OnConnected;
        ConnectionManager.Instance.Client.MessageReceived += OnMessage;

        SubmitLoginButton.onClick.AddListener(StartConnectingIfPossible);
        ManualConnectButton.onClick.AddListener(delegate { LoginWindow.SetActive(!LoginWindow.activeSelf); });

        LoginWindow.SetActive(false);

        DiscoverabilityClientApi.Connect = server =>
        {
            HostInput.text = server.IpAddress;
            PortInput.text = "4296";
            if (NameInput.text == "")
            {
                SetRandomPlayerName();
            }

            StartConnectingIfPossible();
        };

        InvokeRepeating(nameof(CheckRefreshServers), 1, 1);
    }

    private void OnDestroy()
    {
        ConnectionManager.Instance.OnConnected -= OnConnected;
        ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
    }

    private void CheckRefreshServers()
    {
        if (Time.realtimeSinceStartup - lastServerRefresh < 30)
            return;

        RefreshServers();
    }

    public void RefreshServers()
    {
        lastServerRefresh = Time.realtimeSinceStartup;
        ServerBrowser.RefreshServers();
    }

    public void OnConnected()
    {
        SubmitLogin();
    }

    private void SetRandomPlayerName()
    {
        NameInput.text = "Beginner" + UnityEngine.Random.Range(1, 100);
    }

    private void StartConnectingIfPossible()
    {
        if (NameInput.text == "")
        {
            SetRandomPlayerName();
            return;
        }

        string hostname = HostInput.text;
        if (hostname == "")
        {
            hostname = ConnectionManager.Instance.Hostname;
        }

        string port = PortInput.text;
        if (port == "")
        {
            port = "4296";
        }

        LoginWindow.SetActive(false);
        ServerBrowser.gameObject.SetActive(false);

        var connectionManager = ConnectionManager.Instance;
        connectionManager.Hostname = hostname;
        connectionManager.Port = ushort.Parse(port);
        connectionManager.Connect(ex => OnLoginDecline());
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        using Message message = e.GetMessage();

        switch ((Tags)message.Tag)
        {
            case Tags.LoginRequestDenied:
                OnLoginDecline();
                break;
            case Tags.LoginRequestAccepted:
                OnLoginAccept(message.Deserialize<LoginInfoData>());
                break;
            case Tags.LobbyJoinRoomDenied:
                OnRoomJoinDenied(message.Deserialize<LobbyInfoData>());
                break;
            case Tags.LobbyJoinRoomAccepted:
                OnRoomJoinAcepted();
                break;
        }
    }

    public void SubmitLogin()
    {
        Debug.Log("Submitting login");

        LoginWindow.SetActive(false);

        if (string.IsNullOrEmpty(NameInput.text))
        {
            NameInput.text = "Unnamed Player";
        }

        using Message message = Message.Create((ushort)Tags.LoginRequest, new LoginRequestData(NameInput.text));
        
        ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
    }

    private void OnLoginDecline()
    {
        ServerBrowser.gameObject.SetActive(true);
    }

    private void OnLoginAccept(LoginInfoData data)
    {
        ConnectionManager.Instance.OwnPlayerId = data.Id;
        ConnectionManager.Instance.LobbyInfoData = data.Data;

        SendJoinRoomRequest();
    }

    public void SendJoinRoomRequest()
    {
        Debug.Log("Joining room");

        string roomName = "Main";

        using Message message = Message.Create((ushort)Tags.LobbyJoinRoomRequest, new JoinRoomRequest(roomName));

        ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
    }

    public void OnRoomJoinDenied(LobbyInfoData data)
    {
        Debug.Log("Too many players on server?");
    }

    public void OnRoomJoinAcepted()
    {
        SceneManager.LoadScene("Map1");
    }
}
