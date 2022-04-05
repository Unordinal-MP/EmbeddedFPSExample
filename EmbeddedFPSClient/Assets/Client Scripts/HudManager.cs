using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DarkRift.Client.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public Slider MouseSensitivitySlider;

    private bool debugViewEnabled = true;

    private UnityClient client;

    private string messageInRate;
    private string byteInRate;
    private string reconciliationRate;
    private string confirmationRate;

    private void Awake()
    {
        if (ServerManager.Instance != null)
        {          
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        MouseSensitivitySlider.onValueChanged.AddListener(sliderValue =>
        {
            var controller = GetFirstPersonController();
            if (controller == null)
            {
                return;
            }

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
        if (client == null)
        {
            client = Object.FindObjectOfType<UnityClient>();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            debugViewEnabled = !debugViewEnabled;
        }
    }

    private void UpdateStats()
    {
        //this primarily serves a purpose of being more visually appealing and readable than values changing every frame
        var culture = CultureInfo.InvariantCulture;
        messageInRate = ClientStats.Instance.MessagesIn.GetWindowRate().ToString("N3", culture);
        byteInRate = ClientStats.Instance.BytesIn.GetWindowRate().ToString("N3", culture);
        reconciliationRate = ClientStats.Instance.Reconciliations.GetWindowRate().ToString("N3", culture);
        confirmationRate = ClientStats.Instance.Confirmations.GetWindowRate().ToString("N3", culture);
    }

    private void OnGUI()
    {
        if (debugViewEnabled)
        {
            MakeDebugView();
        }

        //temp implementation of scoreboard
        MakeScoreboard();

        //temp implementation of name signs
        MakeNameSigns();
    }

    private void MakeDebugView()
    {
        if (client == null)
        {
            return;
        }

        GUILayout.Label("Network Debug View (F1 to toggle)");
        GUILayout.Label("Connection status: " + client.ConnectionState);

        foreach (var endpoint in client.Client.RemoteEndPoints)
        {
            GUILayout.Label("Remote: " + endpoint);
        }

        GUILayout.Label("RTT: " + client.Client.RoundTripTime.LatestRtt);
        GUILayout.Label("In messages/s: " + messageInRate);
        GUILayout.Label("In bytes/s: " + byteInRate);
        GUILayout.Label("Reconciliations/s: " + reconciliationRate);
        GUILayout.Label("Confirmations/s: " + confirmationRate);
        GUILayout.Label("Reconciliation history: " + ClientStats.Instance.ReconciliationHistorySize);
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
            {
                return a.Deaths.CompareTo(b.Deaths);
            }

            return b.Kills.CompareTo(a.Kills);
        });
        foreach (var player in players)
        {
            GUILayout.Label(player.PlayerName + "   " + player.Kills + " kills  " + player.Deaths + " deaths");
        }
        
        GUILayout.EndArea();
    }

    private void MakeNameSigns()
    {
        if (GameManager.Instance.OwnPlayer == null)
        {
            return;
        }

        Camera camera = GameManager.Instance.OwnPlayer.GetComponent<FirstPersonController>().camera;

        foreach (ClientPlayer player in GameManager.Instance.Players)
        {
            if (player.IsOwn)
            {
                continue;
            }

            Vector3 signPosition = player.transform.position;
            signPosition += Vector3.up * 0.9f;

            var direction = signPosition - camera.transform.position;
            if (Vector3.Dot(direction, camera.transform.forward) < 0)
            {
                continue;
            }

            if (Physics.Raycast(camera.transform.position, direction, out RaycastHit hitInfo, direction.magnitude, 1, QueryTriggerInteraction.Ignore))
            {
                //Debug.DrawLine(camera.transform.position, hitInfo.point, Color.red, 10);
                continue;
            }

            signPosition = camera.WorldToScreenPoint(signPosition);
            signPosition.y = Screen.height - signPosition.y;

            var label = new GUIContent(player.PlayerName);
            Vector2 size = GUIStyle.none.CalcSize(label);

            GUILayout.BeginArea(new Rect(signPosition.x - size.x / 2, signPosition.y - 2 - size.y, 200, 30));
            GUILayout.Label(label);
            GUILayout.EndArea();
        }
    }
}
