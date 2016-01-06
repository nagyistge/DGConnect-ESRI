// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdxComms.cs" company="DigitalGlobe">
//   Copyright 2015 DigitalGlobe
//   
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//   
//          http://www.apache.org/licenses/LICENSE-2.0
//   
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetworkConnections
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Text;

    using Logging;

    using RestSharp;

    /// <summary>
    /// The Gbdx cloud comms.
    /// </summary>
    public class GbdxComms : IGbdxComms
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static Logger logger = null;

        /// <summary>
        /// Username to be used with OAuth authentication
        /// </summary>
        private string localUsername;

        /// <summary>
        /// Password to be used with OAuth Authentication.
        /// </summary>
        private string localPassword;
        
        /// <summary>
        /// Authentication endpoint for CAS
        /// </summary>
        private string authenticationEndpoint;

        /// <summary>
        /// The RestClient to handle network communications.
        /// </summary>
        private IRestClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbdxComms"/> class.
        /// </summary>
        /// <param name="logFile">
        /// The log file.
        /// </param>
        /// <param name="useConsole">
        /// The use console.
        /// </param>
        public GbdxComms(string logFile, bool useConsole)
        {
            logger = new Logger(logFile, useConsole);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GbdxComms"/> class. 
        /// When no parameters are part of the constructor then logging will not be possible for the network calls.
        /// </summary>
        public GbdxComms()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GbdxComms"/> class. Allows a class implementing the IRestClient interface to be used
        /// instead of the default RestSharp class.  Particularly useful for unit tests.
        /// </summary>
        /// <param name="useThisClient">
        /// The use this client.
        /// </param>
        public GbdxComms(IRestClient useThisClient)
        {
            this.client = useThisClient;
        }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        private string AccessToken { get; set; }

        /// <summary>
        /// Used to make staged requests.
        /// </summary>
        /// <param name="netObject">
        /// contains settings and other misc items needed for the network communications
        /// </param>
        /// <param name="formParms">
        /// the page id is so large that it has to be passed in the form parameters.
        /// </param>
        /// <returns>
        /// true if the request was successful.  false if there was a problem.
        /// </returns>
        public bool StagedRequest(ref NetObject netObject, NameValueCollection formParms)
        {
            try
            {
                // Check the settings if valid processing will continue.
                if (!this.CheckSettings(
                    ref this.client,
                    ref netObject,
                    ref this.localUsername,
                    ref this.localPassword,
                    ref this.authenticationEndpoint))
                {
                    return false;
                }

                var request = new RestRequest(netObject.AddressUrl, Method.POST);
                request.AddHeader("Authorization", string.Format("Bearer {0}", this.AccessToken));

                foreach (var item in formParms.Keys)
                {
                    request.AddParameter(item.ToString(), formParms[item.ToString()]);
                }

                var response = this.client.Execute(request);
                netObject.Result = response.Content;

                // get the paging id from the response headers
                var retrievedPageId = response.Headers.FirstOrDefault(id => id.Name == "Vector-Paging-Id");
                if (retrievedPageId != null)
                {
                    netObject.PageId = retrievedPageId.Value.ToString();
                }

                // get the number of vector items in the page from the response headers.
                var retrievedItemCount = response.Headers.FirstOrDefault(item => item.Name == "Vector-Item-Count");
                if (retrievedItemCount != null)
                {
                    netObject.PageItemCount =
                        Convert.ToInt32(retrievedItemCount.Value.ToString());
                }

                netObject = this.CheckForErrors(response, netObject);
            }
            catch (Exception excp)
            {
                logger.Error(excp);
                netObject.ErrorOccurred = true;
                return false;
            }

            if (netObject.ErrorOccurred)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Request that uses the Push method.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="bodyPostData">
        /// The body post data.
        /// </param>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        public NetObject PushRequest(NetObject netObject, string bodyPostData)
        {
            try
            {
                if (
                    !this.CheckSettings(
                        ref this.client,
                        ref netObject,
                        ref this.localUsername,
                        ref this.localPassword,
                        ref this.authenticationEndpoint))
                {
                    return netObject;
                }

                var request = new RestRequest(netObject.AddressUrl, Method.POST);
                request.AddHeader("Authorization", string.Format("Bearer {0}", this.AccessToken));
                var postData = Encoding.ASCII.GetBytes(bodyPostData);
                request.AddParameter("application/octet-stream", postData, ParameterType.RequestBody);

                var response = this.client.Execute(request);
                netObject.Result = response.Content;
                netObject = this.CheckForErrors(response, netObject);
            }
            catch (Exception error)
            {
                logger.Error(error);
                netObject.ErrorOccurred = true;
            }

            return netObject;
        }

        /// <summary>
        /// The request.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        public NetObject Request(NetObject netObject)
        {
            try
            {
                // Check the settings if valid processing will continue;
                if (!this.CheckSettings(
                    ref this.client,
                    ref netObject,
                    ref this.localUsername,
                    ref this.localPassword,
                    ref this.authenticationEndpoint))
                {
                    return netObject;
                }

                var request = new RestRequest(netObject.AddressUrl);
                request.AddHeader("Authorization", string.Format("Bearer {0}", this.AccessToken));
                var response = this.client.Execute(request);
                netObject.Result = response.Content;
                netObject = this.CheckForErrors(response, netObject);
            }
            catch (Exception excp)
            {
                logger.Error(excp);
                netObject.ErrorOccurred = true;
            }

            return netObject;
        }

        /// <summary>
        /// Generic Post method.  Whatever type is in the T will be returned from the post.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="jsonDataPayLoad">
        /// The JSON data pay load.
        /// </param>
        /// <typeparam name="T"> Generic type to be returned
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public T Post<T>(NetObject netObject, string jsonDataPayLoad)
        {
            // Check the settings if valid processing will continue;
            if (!this.CheckSettings(
                ref this.client,
                ref netObject,
                ref this.localUsername,
                ref this.localPassword,
                ref this.authenticationEndpoint))
            {
                // return null in generic method terms.
                return default(T);
            }

            var request = new RestRequest(netObject.AddressUrl, Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.AccessToken);
            request.AddParameter("application/json", jsonDataPayLoad, ParameterType.RequestBody);

            var result = this.client.Execute<List<T>>(request);

            if (result.Data.Count > 0)
            {
                return result.Data[0];
            }

            return default(T);
        }

        /// <summary>
        /// Get the RestSharp client used in the wrapper interface.
        /// </summary>
        /// <returns>
        /// The <see cref="IRestClient"/>.
        /// </returns>
        public IRestClient GetClient()
        {
            return this.client;
        }

        /// <summary>
        /// Returns the GBD access token.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAccessToken()
        {
            return this.AccessToken;
        }

        /// <summary>
        /// Handle the delete request of a stored query workspace.
        /// </summary>
        /// <param name="netObject">
        /// contains settings and other misc items needed for the network communications
        /// </param>
        /// <returns>
        /// net object containing the result of the request.
        /// </returns>
        public NetObject DeleteRequest(NetObject netObject)
        {
            return netObject;
        }

        /// <summary>
        /// Wrapper function for the private Authenticate method.
        /// </summary>
        /// <param name="nobj">
        /// contains settings and other misc items needed for the network communications
        /// </param>
        /// <returns>
        /// true if the authentication process is successful.
        /// </returns>
        public bool AuthenticateNetworkObject(ref NetObject nobj)
        {
            var result = Authenticate(ref nobj, ref this.client);

            // If authorized then lets save the authorization token.
            if (result)
            {
                this.AccessToken = nobj.AuthorizationToken;
            }

            return result;
        }

        /// <summary>
        /// Will authenticate the NetObject's cookie container.  
        /// This is a useful way to pre-authenticate prior to sending a request.  
        /// Note this is required before a delete request can be sent. 
        /// </summary>
        /// <param name="nobj">
        /// contains settings and other misc items needed for the network communications
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// true if the authentication process is successful.
        /// </returns>
        private static bool Authenticate(ref NetObject nobj, ref IRestClient client)
        {
            if (client == null || client.BaseUrl == null ||client.BaseUrl != new Uri(nobj.BaseUrl))
            {
                client = new RestClient(nobj.BaseUrl);
            }

            IRestRequest request = new RestRequest(nobj.AuthEndpoint, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // Convert userpass to 64 base string.  i.e. username:password => 64 base string representation
            var passBytes = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", nobj.User, nobj.Password));
            var string64 = Convert.ToBase64String(passBytes);

            // Add the 64 base string representation to the header
            request.AddHeader("Authorization", string.Format("Basic {0}", string64));

            request.AddParameter("grant_type", "password");
            request.AddParameter("username", nobj.User);
            request.AddParameter("password", nobj.Password);

            IRestResponse<AccessToken> response = client.Execute<AccessToken>(request);

            // Set the status code.
            nobj.ResponseStatusCode = response.StatusCode;

            // if the request was ok get the access token.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                nobj.AuthorizationToken = response.Data.access_token;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check and set settings for the RestClient prior to url execution.
        /// </summary>
        /// <param name="restClient">
        /// The rest client.
        /// </param>
        /// <param name="netObject">
        /// The net object containing latest settings.
        /// </param>
        /// <param name="user">
        /// The username.
        /// </param>
        /// <param name="pass">
        /// The password.
        /// </param>
        /// <param name="authEndpoint">
        /// The auth Endpoint.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CheckSettings(ref IRestClient restClient, ref NetObject netObject, ref string user, ref string pass, ref string authEndpoint)
        {
            if (restClient == null || restClient.BaseUrl == null || restClient.BaseUrl != new Uri(netObject.BaseUrl))
            {
                restClient = new RestClient(netObject.BaseUrl);
                this.client = restClient;
                user = netObject.User;
                pass = netObject.Password;
                authEndpoint = netObject.AuthEndpoint;
                return this.AuthenticateNetworkObject(ref netObject);
            }

            // Check to make sure the username andpasword hasn't changed.
            if (!string.Equals(pass, netObject.Password)
                && !string.Equals(user, netObject.User)
                && !string.Equals(authEndpoint, netObject.AuthEndpoint))
            {
                user = netObject.User;
                pass = netObject.Password;
                authEndpoint = netObject.AuthEndpoint;
                return this.AuthenticateNetworkObject(ref netObject);
            }

            return true;
        }

        /// <summary>
        /// Check the IRestResponse for errors.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="obj">
        /// The network object for the request
        /// </param>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        private NetObject CheckForErrors(IRestResponse response, NetObject obj)
        {
            obj.ResponseStatusCode = response.StatusCode;
            if (response.StatusCode == HttpStatusCode.Accepted ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.OK)
            {
                return obj;
            }

            obj.ErrorOccurred = true;
            logger.Error(response.Content);
            return obj;
        }
    }
}