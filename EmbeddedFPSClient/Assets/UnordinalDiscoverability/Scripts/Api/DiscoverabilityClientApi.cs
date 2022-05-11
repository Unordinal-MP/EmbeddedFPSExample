using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Unordinal.Discoverability
{
    public static class DiscoverabilityClientApi
    {
        [System.Serializable]
        public struct ServerInfo
        {
            [Tooltip("Server details needed to connect")]
            public string serverUri;
            [HideInInspector]
            public string serverId;
            public string serverName;

            public string IpAddress => serverUri.Split(':').FirstOrDefault();
        }

        [System.Serializable]
        public struct ListOfGameServers
        {
            public List<ServerInfo> gameServers;
        }

        public delegate void ConnectDelegate(ServerInfo server);
        public static ConnectDelegate Connect { get; set; } = info => Debug.LogWarning($"You want to connect to ip = {info.IpAddress}, but no function is provided.");

        public static async Task<ListOfGameServers> GetServerListAsync(bool autoConnect=false)
        {
            return await GetServerListAsync(Utility.LoadGameId(), autoConnect);
        }

        public static async Task<ListOfGameServers> GetServerListAsync(string gameId, bool autoConnect=false)
        {
            Debug.Log("Discoverability: Receiving the server list.");
            var result = await DiscoverabilityApi.CallDiscoverabilityApiAsync(HttpMethod.Get,
                $"client?gameId={gameId}&autoConnect={autoConnect}");
            return JsonUtility.FromJson<ListOfGameServers>(result);
        }
    }
}