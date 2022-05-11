using System.Threading.Tasks;
using UnityEngine;
using static Unordinal.Discoverability.DiscoverabilityClientApi;

namespace Unordinal.Discoverability
{
    public class ServerBrowser : MonoBehaviour
    {
        [Header("UI ELEMENTS")]
        [SerializeField]
        [Tooltip("Prefab to show different servers")]
        GameObject serverUIPrefab;
        [SerializeField]
        [Tooltip("Parent object containing all the available servers")]
        RectTransform containerGameobject;

        [Header("SERVER")]
        [Tooltip("Do you want the client to connect automatically to the best server?")]
        [SerializeField]
        bool autoConnectToServer;
        [Tooltip("The complete list of servers returned as a list")]
        [SerializeField]
        ListOfGameServers serverList = new ListOfGameServers();

        private void Start()
        {
            RefreshServers();
        }

        /// <summary>
        /// Get a list of all the available servers registered with the discoverability backend
        /// </summary>
        public async void RefreshServers()
        {
            if (serverList.gameServers.Count > 1)
                serverList.gameServers.Clear();
            serverList = await GetServerListAsync(Utility.LoadGameId());
            DisplayUI();
        }

        /// <summary>
        /// Display the servers on the UI
        /// </summary>
        public void DisplayUI()
        {
            if (serverList.gameServers.Count < 1)
                return;
            foreach (Transform child in containerGameobject)
                Destroy(child.gameObject);
            foreach (ServerInfo info in serverList.gameServers)
            {
                GameObject go = Instantiate(serverUIPrefab, containerGameobject);
                ServerBrowserItem prefabUi = go.GetComponent<ServerBrowserItem>();
                prefabUi.SetValues(in info);
            }
        }
    }
}