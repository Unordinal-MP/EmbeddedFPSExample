using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public Slider MouseSensitivitySlider;

    private bool _debugViewEnabled = true;

    private UnityClient _client;

    private string _messageInRate;
    private string _byteInRate;

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
        if (GameManager.Instance == null)
            return;

        //this primarily serves a purpose of being more visually appealing and readable than values changing every frame
        _messageInRate = GameManager.Instance.MessageStat.GetWindowRate().ToString("N3", CultureInfo.InvariantCulture);
        _byteInRate = GameManager.Instance.ByteStat.GetWindowRate().ToString("N3", CultureInfo.InvariantCulture);
    }

    private void OnGUI()
    {
        if (_client != null && _debugViewEnabled)
            MakeDebugView();
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
    }
}
