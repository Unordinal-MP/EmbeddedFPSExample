using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unordinal.Discoverability
{
    public static class DiscoverabilityServerApi
    {
        private const bool signWithKey = true;

        [System.Serializable]
        public class ServerInfo
        {
            public string serverUri;
            public int maxPlayers;
            public string gameId;
            public string serverName;

            public ServerInfo(string serverUri, int maxPlayers, string serverName = "")
            {
                this.serverUri = serverUri;
                this.maxPlayers = maxPlayers;
                this.serverName = serverName;
                if (serverName == "")
                    this.serverName = Utility.NameGenerator.GetRandomName();
                this.gameId = Utility.LoadGameId();
            }
        }

        [System.Serializable]
        public struct ServerId
        {
            public string serverId;
        }

        public struct PlayerCount
        {
            public int playerCount;
        }

        public delegate int GetNumberOfPlayersDelegate();
        public static GetNumberOfPlayersDelegate GetNumberOfPlayers { get; set; } = GetNumberOfPlayersDefault;

        private static int GetNumberOfPlayersDefault() { return -1; }

        public static async Task<ServerId> RegisterServerWithDiscoverabilityAsync(ServerInfo serverInfo)
        {
            Debug.Log("Discoverability: Registering this server.");
            var result = await DiscoverabilityApi.CallDiscoverabilityApiAsync(HttpMethod.Post,
                "server",
                serverInfo,
                signWithKey);
            
            if (result != null)
            {
                string id = JsonUtility.FromJson<ServerId>(result).serverId;
                await PingDiscoverabilityApiInLoopAsync(id);
            }
            return JsonUtility.FromJson<ServerId>(result);
        }

        public static async Task PingDiscoverabilityApiAsync(string serverId)
        {
            await DiscoverabilityApi.CallDiscoverabilityApiAsync(HttpMethod.Put,
                $"server/{serverId}",
                new PlayerCount { playerCount = GetNumberOfPlayers() },
                signWithKey);
        }

        private static async Task PingDiscoverabilityApiInLoopAsync(string serverId, CancellationToken token = default(CancellationToken))
        {
            const int frequencyInMs = 10000;
            while (!token.IsCancellationRequested)
            {
                var pingTask = PingDiscoverabilityApiAsync(serverId);
                try
                {
                    var delayTask = Task.Delay(frequencyInMs, token);

                    await Task.WhenAll(pingTask, delayTask);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}