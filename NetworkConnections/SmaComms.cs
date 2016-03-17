// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmaComms.cs" company="DigitalGlobe">
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
    using System.Collections.Specialized;
    using System.Net;
    using System.Text;

    using Logging;

    using RestSharp;

    /// <summary>
    /// Implments the IGbdxComms interface(s)
    /// </summary>
    public class SmaComms : IGbdxComms
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static Logger logger = null;

        /// <summary>
        /// Username to be used with CAS authentication
        /// </summary>
        private string localUsername;

        /// <summary>
        /// Password to be used with CAS Authentication.
        /// </summary>
        private string localPassword;

        /// <summary>
        /// Ticket granting ticket endpoint for CAS
        /// </summary>
        private string ticketEndpoint;

        /// <summary>
        /// Authentication endpoint for CAS
        /// </summary>
        private string authenticationEndpoint;

        /// <summary>
        /// The RestClient to handle network communications.
        /// </summary>
        private RestClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmaComms"/> class. 
        /// Initializes a new instance of the <see cref="GbdxComms"/> class.
        /// </summary>
        /// <param name="logFile">
        /// The log file.
        /// </param>
        /// <param name="useConsole">
        /// The use console.
        /// </param>
        public SmaComms(string logFile, bool useConsole)
        {
            logger = new Logger(logFile, useConsole);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmaComms"/> class. 
        /// Initializes a new instance of the <see cref="GbdxComms"/> class. 
        /// When no parameters are part of the constructor then logging will not be possible for the network calls.
        /// </summary>
        public SmaComms()
        {
        }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        public RestClient Client
        {
            get
            {
                return this.client;
            }

            set
            {
                this.client = value;
            }
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
                this.CheckSettings(ref this.client, netObject, ref this.localUsername, ref this.localPassword, ref this.ticketEndpoint, ref this.authenticationEndpoint);
                var request = new RestRequest(netObject.AddressUrl);
                var response = this.Client.Execute(request);
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
        /// The staged request.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="formParms">
        /// The form parameters.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException"> Currently Not implemented in this class because it isn't required
        /// </exception>
        public bool StagedRequest(ref NetObject netObject, NameValueCollection formParms)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method is what is used for a push request.  Currently only used in the stored queries calls.
        /// </summary>
        /// <param name="netObject">
        /// contains settings and other misc items needed for the network communications
        /// </param>
        /// <param name="bodyPostData">
        /// string form for the body of the request.
        /// </param>
        /// <returns>
        /// net object containing the result of the request
        /// </returns>
        public NetObject PushRequest(NetObject netObject, string bodyPostData)
        {
            try
            {
                this.CheckSettings(ref this.client, netObject, ref this.localUsername, ref this.localPassword, ref this.ticketEndpoint, ref this.authenticationEndpoint);
                var request = new RestRequest(netObject.AddressUrl, Method.POST);

                var postData = Encoding.ASCII.GetBytes(bodyPostData);
                request.AddParameter("application/octet-stream", postData, ParameterType.RequestBody);

                var response = this.Client.Execute(request);
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
            try
            {
                this.CheckSettings(ref this.client, netObject, ref this.localUsername, ref this.localPassword, ref this.ticketEndpoint, ref this.authenticationEndpoint);
                var request = new RestRequest(netObject.AddressUrl, Method.DELETE);
                var response = this.Client.Execute(request);
                netObject.Result = response.Content;
                netObject = this.CheckForErrors(response, netObject);
                return netObject;
            }
            catch (Exception excp)
            {
                logger.Error(excp);
                netObject.ErrorOccurred = true;
            }

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
            this.Client = new RestClient(nobj.BaseUrl)
            {
                Authenticator =
                    new CasAuthenticator(
                    nobj.User,
                    nobj.Password,
                    nobj.TicketEndpoint,
                    nobj.AuthEndpoint),
                PreAuthenticate = true
            };
            var result = Authenticate(ref nobj);

            if(result)
            {
                this.Client.CookieContainer = nobj.CookieJar;
            }

            return result;
        }

        /// <summary>
        /// The post.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="jsonDataPayLoad">
        /// The JSON
        ///  data pay load.
        /// </param>
        /// <typeparam name="T">
        /// Generic type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">Not implemented because this class does not require this functionality.
        /// </exception>
        public T Post<T>(NetObject netObject, string jsonDataPayLoad)
        {
            throw new NotImplementedException();
        }

        public IRestClient GetClient()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public string GetAccessToken()
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode UploadFile(NetObject netObject, string filepath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Will authenticate the NetObject's cookie container.  
        /// This is a useful way to pre-authenticate prior to sending a request.  
        /// Note this is required before a delete request can be sent. 
        /// </summary>
        /// <param name="nobj">
        /// contains settings and other misc items needed for the network communications
        /// </param>
        /// <returns>
        /// true if the authentication process is successful.
        /// </returns>
        private static bool Authenticate(ref NetObject nobj)
        {
            if (nobj == null)
            {
                return false;
            }

            var tgt = DgAuth.Instance.GetTicketGrantingTicket(nobj.TicketEndpoint, nobj.User, nobj.Password);
            if (string.IsNullOrEmpty(tgt))
            {
                return false;
            }

            var st = DgAuth.Instance.GetServiceTicket(nobj.TicketEndpoint, tgt, nobj.AuthEndpoint);

            if (string.IsNullOrEmpty(st))
            {
                return false;
            }

            try
            {
                nobj.CookieJar = DgAuth.Instance.AuthenticateService(st, nobj.AuthEndpoint);
            }
            catch(WebException error)
            {
                nobj.ErrorOccurred = true;
                nobj.Error = error;
                nobj.ResponseStatusCode = ((HttpWebResponse)error.Response).StatusCode;
                return false;
            }

            return true;
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
        /// <param name="ticketGrantingEndpoint">
        /// The ticket Granting Endpoint.
        /// </param>
        /// <param name="authEndpoint">
        /// The auth Endpoint.
        /// </param>
        private void CheckSettings(ref RestClient restClient, NetObject netObject, ref string user, ref string pass, ref string ticketGrantingEndpoint, ref string authEndpoint)
        {
            if (restClient == null || netObject.BaseUrl != this.Client.BaseUrl.ToString())
            {
                restClient = new RestClient(netObject.BaseUrl)
                                 {
                                     Authenticator =
                                         new CasAuthenticator(
                                         netObject.User,
                                         netObject.Password,
                                         netObject.TicketEndpoint,
                                         netObject.AuthEndpoint),
                                     PreAuthenticate = true
                                 };
                user = netObject.User;
                pass = netObject.Password;
                ticketGrantingEndpoint = netObject.TicketEndpoint;
                authEndpoint = netObject.AuthEndpoint;
            }

            // Check to make sure the username andpasword hasn't changed.
            if (!string.Equals(pass, netObject.Password)
                && !string.Equals(user, netObject.User)
                && !string.Equals(ticketGrantingEndpoint, netObject.TicketEndpoint)
                && !string.Equals(authEndpoint, netObject.AuthEndpoint))
            {
                restClient.Authenticator = new CasAuthenticator(netObject.User, netObject.Password, netObject.TicketEndpoint, netObject.AuthEndpoint);
                user = netObject.User;
                pass = netObject.Password;
                ticketGrantingEndpoint = netObject.TicketEndpoint;
                authEndpoint = netObject.AuthEndpoint;
            }
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


        public NetObject PushRequest(NetObject netObject)
        {
            throw new NotImplementedException();
        }
    }
}