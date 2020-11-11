/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RevitInventorExchange.Utilities;

namespace RevitInventorExchange.CoreBusinessLayer
{
    /// <summary>
    /// Abstract class providing basic infrastructure for HTTP calls to Forge
    /// </summary>
    public abstract class ForgeClientHandler
    {
        private RestClient Client { get; set; } = null;

        public string Authorization => ((ForgeAuthenticator)Client.Authenticator).Token.GetHeader();

        protected ForgeClientHandler(string baseURL, string clientId, string clientSecret, string authScope)
        {
            NLogger.LogText("Entered ForgeClientHandler");

            Uri baseUri = new Uri(baseURL);
            string domain = baseUri.GetLeftPart(UriPartial.Authority);
            bool isLocal = baseUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

            Client = new RestClient(baseURL)
            {
                Authenticator = isLocal ? null : new ForgeAuthenticator(domain, clientId, clientSecret, authScope)                 
            };

            //NLogger.LogText($"Set HTTP CLient timeout: {ConfigUtilities.GetAsyncHTTPCallWaitTime()} milliseconds");
            NLogger.LogText($"Set HTTP CLient domain: {domain}");
            NLogger.LogText($"Set HTTP CLient isLocal: {isLocal}");

            //Client.Timeout = ConfigUtilities.GetAsyncHTTPCallWaitTime();

            NLogger.LogText("Exit ForgeClientHandler");
        }

        public void SetBaseURL(string baseURL)
        {
            Uri baseUri = new Uri(baseURL);
            Client.BaseUrl = baseUri;
        }

        public void GetToken()
        {
            ((ForgeAuthenticator)Client.Authenticator).GetTokenHeader();
        }

        /// <summary>
        /// Prepares Request and perform Async call to Forge
        /// </summary>
        /// <param name="path"></param>
        /// <param name="payload"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task<ForgeRestResponse> RequestAsync(string path, string payload, Method method)
        {
            NLogger.LogText($"Entered RequestAsync with url {Client.BaseUrl}{path}, method {method.ToString()}");

            if (!string.IsNullOrEmpty(payload))
            {
                var loggedPayload = Utility.HideTokenInJson(payload);
                NLogger.LogText($"Entered RequestAsync with payload {loggedPayload}");
            }

            RestRequest request = new RestRequest(path, method);
            request.AddParameter("application/json", payload, ParameterType.RequestBody);

            NLogger.LogText("Execute Async HTTP call");
            IRestResponse response = await Client.ExecuteAsync(request);

            NLogger.LogText("Exit RequestAsync");

            return new ForgeRestResponse(response);
        }

        /// <summary>
        /// Prepares Request and perform Async call to Forge
        /// </summary>
        /// <param name="path"></param>
        /// <param name="payload"></param>
        /// <param name="application"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task<ForgeRestResponse> RequestAsync(string path, string payload, string application, Method method)
        {
            NLogger.LogText($"Entered RequestAsync with url: {Client.BaseUrl}{path}, method: {method.ToString()}, application: {application}");

            if (!string.IsNullOrEmpty(payload))
                NLogger.LogText($"Entered RequestAsync with payload {payload}");

            RestRequest request = new RestRequest(path, method);
            request.AddParameter(application, payload, ParameterType.RequestBody);

            NLogger.LogText("Execute Async HTTP call");
            IRestResponse response = await Client.ExecuteAsync(request);

            NLogger.LogText("Exit RequestAsync");

            return new ForgeRestResponse(response);
        }
    }

    /// <summary>
    /// Class handling the interaction with Design Automation API
    /// </summary>
    public class ForgeDAClient : ForgeClientHandler
    {
        public ForgeDAClient(string baseURL, string clientId, string clientSecret, string authScope = "code:all data:read data:write") :
            base(baseURL, clientId, clientSecret, authScope)
        {
            NLogger.LogText("ForgeDAClient constructor");
        }

        public async Task<ForgeRestResponse> PostWorkItem(string payload)
        {
            return await RequestAsync("workitems", payload, Method.POST);
        }

        public async Task<ForgeRestResponse> CheckWorkItemStatus(string workItemId)
        {
            return await RequestAsync($"workitems/{workItemId}", "", Method.GET);
        }        
    }


    /// <summary>
    /// Class handling the interaction with Design Automation API
    /// </summary>
    public class ForgeDMClient : ForgeClientHandler
    {
        public ForgeDMClient(string baseURL, string clientId, string clientSecret, string authScope = "code:all") :
            base(baseURL, clientId, clientSecret, authScope)
        {
            NLogger.LogText("ForgeDMClient constructor");
        }

        public async Task<ForgeRestResponse> GetHub(Dictionary<string, string> parameters)
        {
            var ret = await RequestAsync($"hubs", string.Empty, Method.GET);            

            return ret;
        }

        internal async Task<ForgeRestResponse> GetProject(Dictionary<string, string> parameters)
        //internal async Task<ForgeRestResponse> GetProject(string hubId)
        {
            var ret = await RequestAsync($"hubs/{parameters["hubId"]}/projects", string.Empty, Method.GET);
            return ret;
        }

        //internal async Task<ForgeRestResponse> GetTopFolder(string hubId, string topFolderId)
        internal async Task<ForgeRestResponse> GetTopFolder(Dictionary<string, string> parameters)
        {
            var ret = await RequestAsync($"hubs/{parameters["hubId"]}/projects/{parameters["projectId"]}/topFolders", string.Empty, Method.GET);
            return ret;
        }

        //internal async Task<ForgeRestResponse> GetFolderContent(string projectId, string folderId)
        //{
        //    var ret = await RequestAsync($"projects/{projectId}/folders/{folderId}/contents", string.Empty, Method.GET);
        //    return ret;
        //}
        internal async Task<ForgeRestResponse> GetFolderContent(Dictionary<string, string> parameters)
        {
            var ret = await RequestAsync($"projects/{parameters["projectId"]}/folders/{parameters["parentFolderId"]}/contents", string.Empty, Method.GET);
            return ret;
        }

        internal async Task<ForgeRestResponse> CreateStorageObject(string projectId, string payload)
        {
            var ret = await RequestAsync($"projects/{projectId}/storage", payload, "application/vnd.api+json", Method.POST);
            return ret;
        }

        internal async Task<ForgeRestResponse> CreateStorageObject(Dictionary<string, string> parameters)
        {
            var ret = await RequestAsync($"projects/{parameters["projectId"]}/storage", parameters["payload"], "application/vnd.api+json", Method.POST);
            return ret;
        }

        internal async Task<ForgeRestResponse> CreateFileFirstVersion(string projectId, string payload)
        {
            var ret = await RequestAsync($"projects/{projectId}/items", payload, "application/vnd.api+json", Method.POST);
            return ret;
        }

        internal async Task<ForgeRestResponse> CreateFileFirstVersion(Dictionary<string, string> parameters)
        {
            var ret = await RequestAsync($"projects/{parameters["projectId"]}/items", parameters["payload"], "application/vnd.api+json", Method.POST);
            return ret;
        }

        internal async Task<ForgeRestResponse> CreateFileAdditionalVersion(string projectId, string payload)
        {
            var ret = await RequestAsync($"projects/{projectId}/versions", payload, "application/vnd.api+json", Method.POST);
            return ret;
        }

        internal async Task<ForgeRestResponse> CreateFileAdditionalVersion(Dictionary<string, string> parameters)
        {
            var ret = await RequestAsync($"projects/{parameters["projectId"]}/versions", parameters["payload"], "application/vnd.api+json", Method.POST);
            return ret;
        }
    }

    /// <summary>
    /// Class handling the authentication and 2-legged Token generator with Forge
    /// </summary>
    public class ForgeAuthenticator : IAuthenticator
    {
        //  Output generated Token
        public class ForgeToken
        {
            [JsonProperty("token_type")]
            public string TokenType { get; set; } = string.Empty;

            [JsonProperty("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonProperty("expires_in")]
            public double ExpiresIn { get; set; } = 0.0;

            public DateTime Created { get; } = DateTime.Now;

            public bool IsValid()
            {
                return (Created + TimeSpan.FromSeconds(ExpiresIn - 5.0)) > DateTime.Now;
            }

            public string GetHeader()
            {
                return TokenType + " " + AccessToken;
            }
        }

        private string Url { get; }
        private string Key { get; }
        private string Secret { get; }
        private string AuthScope { get; }

        public ForgeToken Token = new ForgeToken();

        public ForgeAuthenticator(string url, string key, string secret, string authScope)
        {
            NLogger.LogText("ForgeAuthenticator constructor");

            Url = url;
            Key = key;
            Secret = secret;
            AuthScope = authScope;
        }

        //  If Request parameter does not have token (2-legged) assigned, get it, or regenerate it is expired
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (!request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            {
                request.AddParameter("Authorization", GetTokenHeader(), ParameterType.HttpHeader);
            }
        }

        public string GetTokenHeader()
        {
            if (Token.IsValid())
                return Token.GetHeader();

            var client = new RestClient(Url);

            RestRequest request = new RestRequest("authentication/v1/authenticate", Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", Key);
            request.AddParameter("client_secret", Secret);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", AuthScope);

            IRestResponse response = client.Execute(request);
            Token = JsonConvert.DeserializeObject<ForgeToken>(response.Content);

            if (Token == null)
            {
                //  TODO: LOG
                //  Console.WriteLine("Call to get an Access token failed. Please check the enviroment variables FORGE_CLIENT_ID and/or FORGE_CLIENT_SECRET and also the configuration files for a proper end points.");
                return "";
            }

            if (!Token.IsValid())
            {
                //  TODO: LOG
                //  Console.WriteLine("Access token is not valid.\nThat usually means that enviroment variables FORGE_CLIENT_ID and/or FORGE_CLIENT_SECRET are not set properly.");
            }

            return Token.GetHeader();
        }
    }

    /// <summary>
    /// Class handling the response received from Forge
    /// </summary>
    public class ForgeRestResponse
    {
        public HttpStatusCode Status => Response.StatusCode;
        public string ResponseContent => Response.Content;

        public IRestResponse RetResponse => Response;

        private IRestResponse Response;
        public ForgeRestResponse(IRestResponse response)
        {
            Response = response;
        }
        public bool IsSuccessStatusCode()
        {
            return ((int)Status >= 200) && ((int)Status <= 299);
        }

        public string GetResponseContentProperty(string key)
        {
            try
            {
                Dictionary<string, object> content = JsonConvert.DeserializeObject<Dictionary<string, object>>(ResponseContent);
                return content[key]?.ToString();
            }
            catch (JsonReaderException)
            {
                //  TODO: LOG
                return null;
            }
        }

        public void ReportError(string strErrorMessage)
        {
            //  TODO: LOG
            //Console.WriteLine($"Error reported: {strErrorMessage}");
            //Console.WriteLine($"Response status: {Status}");
            //Console.WriteLine($"Response details: {ResponseContent}");
        }

        public bool ReportIfError(string strErrorMessage)
        {
            bool bError = !IsSuccessStatusCode();
            if (bError)
                ReportError(strErrorMessage);

            return bError;
        }
    }


}
