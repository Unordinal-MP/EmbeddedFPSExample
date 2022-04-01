using System;
using DarkRift;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject loginWindow;
    [SerializeField]
    private InputField nameInput;
    [SerializeField]
    private InputField hostInput;
    [SerializeField] 
    private Button submitLoginButton;

    void Start()
    {
        ConnectionManager.Instance.OnConnected += OnConnected;
        submitLoginButton.onClick.AddListener(StartConnectingIfPossible);
        ConnectionManager.Instance.Client.MessageReceived += OnMessage;
    }

    void OnDestroy()
    {
        ConnectionManager.Instance.OnConnected -= OnConnected;
        ConnectionManager.Instance.Client.MessageReceived -= OnMessage;
    }

    public void OnConnected()
    {
        SubmitLogin();
    }

    void StartConnectingIfPossible()
    {
        if (nameInput.text == "")
        {
            nameInput.text = "Beginner" + UnityEngine.Random.Range(1, 100);
            return;
        }

        string hostname = hostInput.text;
        if (hostname == "")
            hostname = ConnectionManager.Instance.Hostname;
        ConnectionManager.Instance.Connect(hostname);
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            switch ((Tags) message.Tag)
            {
                case Tags.LoginRequestDenied:
                    OnLoginDecline();
                    break;
                case Tags.LoginRequestAccepted:
                    OnLoginAccept(message.Deserialize<LoginInfoData>());
                    break;
            }
        }
    }

    public void SubmitLogin()
    {
        if (String.IsNullOrEmpty(nameInput.text))
            nameInput.text = "Unnamed Player";

        loginWindow.SetActive(false);

        using (Message message = Message.Create((ushort)Tags.LoginRequest, new LoginRequestData(nameInput.text)))
        {
            ConnectionManager.Instance.Client.SendMessage(message, SendMode.Reliable);
        }
    }

    private void OnLoginDecline()
    {
        loginWindow.SetActive(true);
    }

    private void OnLoginAccept(LoginInfoData data)
    {
        ConnectionManager.Instance.OwnPlayerId = data.Id;
        ConnectionManager.Instance.LobbyInfoData = data.Data;
        SceneManager.LoadScene("Lobby");
    }
}

