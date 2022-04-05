using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public Slider MouseSensitivitySlider;

    private bool _debugViewEnabled = true;

    private UnityClient _client;

    private string _messageInRate;
    private string _byteInRate;
    private string _reconciliationRate;
    private string _confirmationRate;

    void Awake()
    {
        if (ServerManager.Instance != null )
        {          
            DestroyImmediate(gameObject);
            return;
        }
    }
    void Start()
    {
        MouseSensitivitySlider.onValueChanged.AddListener(sliderValue =>
        {
            var controller = GetFirstPersonController();
            if (controller == null)
                return;

            float mouseSensitivity = sliderValue - 0.5f;
            mouseSensitivity *= 3; //arbitrary constant chosen because it feels right
            mouseSensitivity = Mathf.Exp(mouseSensitivity);
            controller.MouseSensitivity = mouseSensitivity;
        });

        const float statRate = 0.8f;
        InvokeRepeating(nameof(UpdateStats), statRate, statRate);
    }

    private static FirstPersonController GetFirstPersonController()
    {
        //not fast but OK for single instance on isolated event handling
        return Object.FindObjectOfType<FirstPersonController>();
    }

    private void Update()
    {
        if (_client == null)
        {
            _client = Object.FindObjectOfType<UnityClient>();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            _debugViewEnabled = !_debugViewEnabled;
        }
    }

    private void UpdateStats()
    {
        //this primarily serves a purpose of being more visually appealing and readable than values changing every frame
        var culture = CultureInfo.InvariantCulture;
        _messageInRate = ClientStats.instance.MessagesIn.GetWindowRate().ToString("N3", culture);
        _byteInRate = ClientStats.instance.BytesIn.GetWindowRate().ToString("N3", culture);
        _reconciliationRate = ClientStats.instance.Reconciliations.GetWindowRate().ToString("N3", culture);
        _confirmationRate = ClientStats.instance.Confirmations.GetWindowRate().ToString("N3", culture);
    }

    private void OnGUI()
    {
        if (_debugViewEnabled)
            MakeDebugView();

        //temp implementation of scoreboard
        MakeScoreboard();

        //temp implementation of name signs
        MakeNameSigns();
    }

    private void MakeDebugView()
    {
        if (_client == null)
            return;
        
        GUILayout.Label("Network Debug View (F1 to toggle)");
        GUILayout.Label("Connection status: " + _client.ConnectionState);

        foreach (var endpoint in _client.Client.RemoteEndPoints)
        {
            GUILayout.Label("Remote: " + endpoint);
        }

        GUILayout.Label("RTT: " + _client.Client.RoundTripTime.LatestRtt);
        GUILayout.Label("In messages/s: " + _messageInRate);
        GUILayout.Label("In bytes/s: " + _byteInRate);
        GUILayout.Label("Reconciliations/s: " + _reconciliationRate);
        GUILayout.Label("Confirmations/s: " + _confirmationRate);
        GUILayout.Label("Reconciliation history: " + ClientStats.instance.ReconciliationHistorySize);
        if (GameManager.Instance != null)
        {
            GUILayout.Label("Server tick: " + GameManager.Instance.LastReceivedServerTick);
            GUILayout.Label("Client tick: " + GameManager.Instance.ClientTick);
        } 
    }

    private void MakeScoreboard()
    {
        const int width = 240;

        GUILayout.BeginArea(new Rect(Screen.width - width, 0, width, Screen.height));
        
        List<ClientPlayer> players = GameManager.Instance.Players.ToList();
        players.Sort((a, b) =>
        {
            if (a.Kills == b.Kills)
                return a.Deaths.CompareTo(b.Deaths);

            return b.Kills.CompareTo(a.Kills);
        });
        foreach (var player in players)
        {
            GUILayout.Label(player.playerName + "   " + player.Kills + " kills  " + player.Deaths + " deaths");
        }
        
        GUILayout.EndArea();
    }

    private void MakeNameSigns()
    {
        if (GameManager.Instance.OwnPlayer == null)
            return;

        Camera camera = GameManager.Instance.OwnPlayer.GetComponent<FirstPersonController>().camera;

        foreach (ClientPlayer player in GameManager.Instance.Players)
        {
            if (player.isOwn)
                continue;

            Vector3 signPosition = player.transform.position;
            signPosition += Vector3.up * 0.9f;

            var direction = signPosition - camera.transform.position;
            if (Vector3.Dot(direction, camera.transform.forward) < 0)
                continue;

            if (Physics.Raycast(camera.transform.position, direction, out RaycastHit hitInfo, direction.magnitude, 1, QueryTriggerInteraction.Ignore))
            {
                //Debug.DrawLine(camera.transform.position, hitInfo.point, Color.red, 10);
                continue;
            }

            signPosition = camera.WorldToScreenPoint(signPosition);
            signPosition.y = Screen.height - signPosition.y;

            var label = new GUIContent(player.playerName);
            Vector2 size = GUIStyle.none.CalcSize(label);

            GUILayout.BeginArea(new Rect(signPosition.x - size.x / 2, signPosition.y - 2 - size.y, 200, 30));
            GUILayout.Label(label);
            GUILayout.EndArea();
        }
    }
}
