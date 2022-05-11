using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Unordinal.Discoverability.DiscoverabilityServerApi;

namespace Unordinal.Discoverability
{
    [System.Serializable]
    public struct ServerDetails
    {
        [Tooltip("Name of your server")]
        public string serverName;
        [HideInInspector]
        public string serverIP;
        public int maxPlayers;
        [Tooltip("Application port number")]
        public string portNumber;
    }

    public class DiscoverableServer : MonoBehaviour
    {
        [Tooltip("Server details, ip, max players and the project id")]
        [SerializeField]
        ServerDetails serverDetails = new ServerDetails();

        void Awake()
        {
            if (serverDetails.portNumber == "")
                serverDetails.portNumber = "7777";
            if (serverDetails.serverName == "")
            {
                serverDetails.serverName = Utility.NameGenerator.GetRandomName();
            }
        }

        private async void Start()
        {
            for (int retryInMs = 1000; ; retryInMs *= 2)
            {
                serverDetails.serverIP = await GetExternalIpAsync();

                if (serverDetails.serverIP != null && serverDetails.serverIP != "")
                    break;

                await Task.Delay(retryInMs);
            }
            
            Debug.Log("My public IP: " + serverDetails.serverIP);
            await DiscoverabilityServerApi.RegisterServerWithDiscoverabilityAsync(CreateServerInfo(serverDetails));
        }

        /// <summary>
        /// Create a new server details class for the low level API to parse
        /// </summary>
        /// <param name="details"> The upper level struct to keep all the values of the 
        /// server</param>
        /// <returns></returns>
        private static ServerInfo CreateServerInfo(ServerDetails details)
        {
            string uri = details.serverIP + ":" + details.portNumber;
            ServerInfo info = new ServerInfo(uri, details.maxPlayers, details.serverName);
            return info;
        }

        private static async Task<string> GetExternalIpAsync()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get("https://api.ipify.org");
            var op = webRequest.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Delay(1);
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return webRequest.downloadHandler.text;
            }

            Debug.Log("Error getting IP");
            return null;
        }
    }
}