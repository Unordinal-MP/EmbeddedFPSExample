using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Unordinal.Discoverability.DiscoverabilityClientApi;

namespace Unordinal.Discoverability
{
    public class ServerBrowserItem : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField]
        Text serverName;
        [SerializeField]
        Text ipAddress;

        public ServerInfo MyServerInfo { get; private set; }

        public void SetValues(in ServerInfo serverInfo)
        {
            MyServerInfo = serverInfo;
            serverName.text = serverInfo.serverName;
            ipAddress.text = serverInfo.IpAddress;
        }

        /// <summary>
        /// This is for the button on the UI
        /// Use any multiplayer server and call their connect method inside this
        /// </summary>
        public void ConnectToServer()
        {
            Connect.Invoke(MyServerInfo);
        }
    }
}