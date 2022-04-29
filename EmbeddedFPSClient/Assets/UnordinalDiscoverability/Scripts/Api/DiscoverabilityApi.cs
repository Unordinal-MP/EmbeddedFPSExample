using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unordinal.Discoverability
{
    internal static class DiscoverabilityApi
    {
        static readonly HttpClient client = new HttpClient();
        const string DiscoverabilityUrl = "https://api.unordinal.com/api/v1/matching/";
        const string resourcePathToKey = "Unordinal/private";

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, object body, bool signWithKey)
        {
            var request = new HttpRequestMessage(method, DiscoverabilityUrl + path);
            if (body != null)
            {
                request.Content = new StringContent(JsonUtility.ToJson(body), System.Text.Encoding.UTF8, "application/json");
            }
            if (signWithKey)
            {
                var asset = Resources.Load<TextAsset>(resourcePathToKey);
                if (asset == null)
                {
                    Debug.LogError("Discoverability: you should create a key first. Please go to your dashboard at Unordinal.com to create one.");
                    return request;
                }
                var key = asset.text;
                request.Headers.Add("x-api-Key", key);
            }
            return request;
        }

        internal static async Task<string> CallDiscoverabilityApiAsync(HttpMethod method, string path, object body = null, bool signWithKey = false, CancellationToken token = default)
        {
            HttpRequestMessage request = BuildRequest(method, path, body, signWithKey);
            var response = await client.SendAsync(request, token);
            return await HandleResponseAsync(path, response);
        }

        private static async Task<string> HandleResponseAsync(string path, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var c = await response.Content.ReadAsStringAsync();
                //Debug.Log($"Discoverability: successful connection to Discoverability backend! {c}.");
                //Debug.Log($"Discoverability: the response: {c}.");
                return c;
            }
            else
            {
                var c = response.ToString();
                Debug.LogError("Discoverability: Something went wrong while connecting to the Discoverability backend at " + path);
                Debug.LogError(c);
                return c;
            }
        }
    }
}
