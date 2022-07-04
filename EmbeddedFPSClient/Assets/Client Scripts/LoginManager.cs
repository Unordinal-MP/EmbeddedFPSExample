using System;
using DarkRift;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unordinal.Discoverability;
using LootLocker.Requests;
using Random = UnityEngine.Random;

public class LoginManager : MonoBehaviour
{
    public GameObject LoginWindow;
    public InputField HostInput;
    public InputField PortInput;
    public Button SubmitLoginButton;
    public Button ManualConnectButton;
    public ServerBrowser ServerBrowser;
    private double lastServerRefresh;
    private string NameInput;
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
            
                SetRandomPlayerName();


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
        NameInput = "Beginner" + UnityEngine.Random.Range(1, 100);
    }

    private void StartConnectingIfPossible()
    {
        if (NameInput == "")
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
        StartLootLockerSession();



        using Message message = Message.Create((ushort)Tags.LoginRequest, new LoginRequestData(NameInput));
        
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

    private void StartLootLockerSession()
    {
        if (string.IsNullOrEmpty(NameInput))
        {
            SetRandomPlayerName();
        }

        string player_identifier = NameInput;

        LootLockerSDKManager.StartGuestSession(player_identifier,(guestResponse) =>
        {
            if (guestResponse.success)
            {
                Debug.Log("LootLocker guest session started.");

                PlayerPrefs.SetInt("PlayerID", guestResponse.player_id);
                PlayerPrefs.SetString("PlayerName", NameInput);
                PlayerPrefs.Save();

                // If it is a new player, set a random name
                if (guestResponse.seen_before == false)
                {

                    string newPlayerName = NameInput;

                    LootLockerSDKManager.SetPlayerName(newPlayerName, (nameResponse) =>
                    {
                        if (nameResponse.success)
                        {
                            Debug.Log("Set new players name to:" + nameResponse.name);
                        }
                        else
                        {
                            Debug.Log("Could not set player name:" + nameResponse.Error);
                        }
                    });
                }
                else
                {
                    // Otherwise get the name
                    LootLockerSDKManager.GetPlayerName((getNameResponse) =>
                    {
                        if (getNameResponse.success)
                        {
                            NameInput = getNameResponse.name;
                        }
                    });
                }
            }
            else
            {
                Debug.Log("Could not start guest session:" + guestResponse.Error);
            }
        });
    }
}
