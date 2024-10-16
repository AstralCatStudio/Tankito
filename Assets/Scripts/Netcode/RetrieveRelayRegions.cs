#region DEDICATED_SERVER

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Net.Http;
using System.Linq;

namespace Tankito.Netcode
{
    public static class RelayRegionFetcher
    {
    //                                       __    _                                   
    //                              _wr""        "-q__                             
    //                           _dP                 9m_     
    //                         _#P                     9#_                         
    //                        d#@                       9#m                        
    //                       d##                         ###                       
    //                      J###                         ###L                      
    //                      {###K                       J###K                      
    //                      ]####K      ___aaa___      J####F                      
    //                  __gmM######_  w#P""   ""9#m  _d#####Mmw__                  
    //               _g##############mZ_         __g##############m_               
    //             _d####M@PPPP@@M#######Mmp gm#########@@PPP9@M####m_             
    //            a###""          ,Z"#####@" '######"\g          ""M##m            
    //           J#@"             0L  "*##     ##@"  J#              *#K           
    //           #"               `#    "_gmwgm_~    dF               `#_          
    //          7F                 "#_   ]#####F   _dK                 JE          
    //          ]                    *m__ ##### __g@"                   F          
    //                                 "PJ#####LP"                                 
    //           `                       0######_                      '           
    //                                 _0########_                                   
    //               .               _d#####^#####m__              ,              
    //                "*w_________am#####P"   ~9#####mw_________w*"                  
    //                    ""9@#####@M""           ""P@#####@M""                    
    //          
        // Set this to your Relay Authentication Token or API Key.
    //    private string accessToken = "YOUR_RELAY_ACCESS_TOKEN"; //  <--------- La linea de codigo mas peligrosa del universo,
    //                 ____________________                                    no se puede quedar nuestra clave de API en NADA
    //               / NO DEJEMOS ESTO ASI \                                   remotamente cercano a publico en internet !!!!!
    //              !    ES UN PELIGRO     !
    //              !     - BERNAT        !
    //              \____________________/
    //                       !  !
    //                       !  !
    //                       L_ !
    //                      / _)!
    //                     / /__L
    //  __________________/ (____)
    //                      (____)
    //  __________________  (____)
    //                    \_(____)
    //                       !  !
    //                       !  !
    //                       \__/  

        private static readonly HttpClient client = new HttpClient();

        // The Relay Regions API endpoint.
        private const string regionsEndpoint = "https://relay-allocations.services.api.unity.com/v1/regions";

        // Async function to fetch Relay regions and return them as a list of strings.
        public static async Task<List<(string id, string desc)>> FetchRelayRegionsAsync(string accessToken)
        {
            string url = "https://relay-allocations.services.api.unity.com/v1/regions";

            try
            {
                // Set up the request with the authorization header.
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throws if not a 200-299 response

                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize JSON to the RelayRegionsResponse object
                var regionsResponse = JsonConvert.DeserializeObject<RelayRegionsResponse>(responseBody);

                return regionsResponse.data.regions.Select( r => (r.id, r.description)).ToList(); // Return both tuple list
            }
            catch (HttpRequestException e)
            {
                Debug.LogWarning("Request error: " + e.Message);
                return new List<(string,string)>(); // Return an empty list on error
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

#endregion