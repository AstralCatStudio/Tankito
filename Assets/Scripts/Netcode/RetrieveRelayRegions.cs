using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;

namespace Tankito.Netcode
{
    public static class RelayRegionFetcher
    {
        // The Relay Regions API endpoint
        private const string regionsEndpoint = "https://relay-allocations.services.api.unity.com/v1/regions";

        // Async function to fetch Relay regions and return them as a list of tuples (id, description)
        public static async Task<List<(string id, string desc)>> FetchRelayRegionsAsync(string accessToken)
        {
            // Create a new UnityWebRequest to fetch the regions
            UnityWebRequest request = UnityWebRequest.Get(regionsEndpoint);
            request.SetRequestHeader("Authorization", "Bearer " + accessToken); // Set the authorization header

            // Send the request and wait for the response
            var operation = request.SendWebRequest();

            // Wait until the request has completed
            while (!operation.isDone)
            {
                await Task.Yield(); // Prevent blocking the main thread
            }

            // Check for network errors or response issues
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error fetching relay regions: " + request.error);
                return new List<(string, string)>(); // Return an empty list on error
            }

            try
            {
                // Parse the JSON response into the RelayRegionsResponse model
                string responseBody = request.downloadHandler.text;
                var regionsResponse = JsonConvert.DeserializeObject<RelayRegionsResponse>(responseBody);

                // Return the regions as a list of tuples (id, description)
                return regionsResponse.data.regions.Select(r => (r.id, r.description)).ToList();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error parsing relay regions response: " + e.Message);
                return new List<(string, string)>(); // Return an empty list if parsing fails
            }
        }
    }

    // Model for deserializing the JSON response
    public class RelayRegionsResponse
    {
        public DataResponse data { get; set; }
    }

    public class DataResponse
    {
        public List<Region> regions { get; set; }
    }

    public class Region
    {
        public string id { get; set; }
        public string description { get; set; }
    }
}
