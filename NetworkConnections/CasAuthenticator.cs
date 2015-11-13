// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CasAuthenticator.cs" company="DigitalGlobe">
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
//   The cas authenticator.  Implements the IAuthenticator interface making it compatible with RestSharp
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetworkConnections
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    using RestSharp;

    /// <summary>
    /// The cas authenticator.  Implements the IAuthenticator interface making it compatible with RestSharp
    /// </summary>
    public class CasAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Class variable containing the username.
        /// </summary>
        private string localUsername;

        /// <summary>
        /// Class variable containing the password.
        /// </summary>
        private string localPassword;

        /// <summary>
        /// Time of last authentication
        /// </summary>
        private DateTime lastAuthenticated;

        /// <summary>
        /// The last request received
        /// </summary>
        private IRestRequest lastRequest;

        /// <summary>
        /// Ticket granting ticket endpoint for CAS.
        /// </summary>
        private string ticketEndpoint;

        /// <summary>
        /// The authentication endpoint for CAS.
        /// </summary>
        private string authEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="CasAuthenticator"/> class.
        /// </summary>
        /// <param name="user">
        /// The username
        /// </param>
        /// <param name="pass">
        /// The password associated with the user.
        /// </param>
        /// <param name="ticket">
        /// The ticket.
        /// </param>
        /// <param name="auth">
        /// The auth.
        /// </param>
        public CasAuthenticator(string user, string pass, string ticket, string auth)
        {
            this.localPassword = pass;
            this.localUsername = user;
            this.ticketEndpoint = ticket;
            this.authEndpoint = auth;
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password
        {
            get
            {
                return this.localPassword;
            }

            set
            {
                this.localPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get
            {
                return this.localUsername;
            }

            set
            {
                this.localUsername = value;
            }
        }

        /// <summary>
        /// The authenticate method implemented from the IAuthenticator interface.  
        /// </summary>
        /// <param name="client">
        /// client that is making a call which needs authentication.
        /// </param>
        /// <param name="request">
        /// The actual request that the client is processing.
        /// </param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (this.lastRequest == null)
            {
                this.lastRequest = request;
            }
            else
            {
                // Check to see if the same request is coming in.  If the same request is identical then assume something happened
                // with the authentication check so lets re-authenticate.
                if (this.lastRequest.Equals(request))
                {
                    client.CookieContainer = null;
                }

                this.lastRequest = request;
            }

            // If the cookie container isn't null check to see if the last time authenticated was within the last 29 minutes
            if (client.CookieContainer != null)
            {
                var currentDate = DateTime.Now.Subtract(this.lastAuthenticated);

                if (currentDate.TotalMinutes <= 29)
                {
                    return;
                }
            }

            // get the ticket granting ticket
            var tgt = this.GetTicketGrantingTicket(
                this.ticketEndpoint,
                this.Username,
                this.Password);

            // get the service ticket
            var st = this.GetServiceTicket(
                this.ticketEndpoint,
                tgt,
                this.authEndpoint);

            // finally authenticate the service.
            client.CookieContainer = this.AuthenticateService(
                st,
                this.authEndpoint);

            // set the time that the service was authenticated.
            this.lastAuthenticated = DateTime.Now;
        }

        /// <summary>
        /// The get ticket granting ticket from CAS
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <returns>
        /// the ticket granting ticket from the CAS server
        /// </returns>
        public string GetTicketGrantingTicket(string server, string username, string password)
        {
            // check the input parameters to make sure that values have been supplied although
            // not neccesarily valid values.
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }

            try
            {
                using (var client = new WebClient())
                {
                    var data = new NameValueCollection();
                    data["username"] = username;
                    data["password"] = password;

                    // If unit test populate response with passed in unit test data else contact server to get data.
                    var response = client.UploadValues(server, "POST", data);

                    // response data null check
                    if (response == null)
                    {
                        return string.Empty;
                    }

                    // Converts to string from byte[]
                    var strResponse = Encoding.UTF8.GetString(response);

                    // Regular Expression to get the code from the html response.
                    var getPostCode = new Regex("<title>(?<code>.*?) ");

                    // The match result from the regular expression
                    var mat = getPostCode.Match(strResponse);

                    // Gets the response code from the capture group named in the regular expression
                    var code = mat.Groups["code"].ToString();

                    switch (code)
                    {
                        // Everything is all good!
                        case "201":

                            // Create regular expression to extract the ticket granting ticket
                            var ticketRegularExpression = new Regex(".*action=\".*/(?<ticket>.*?)\".*");

                            // The match result from the ticketRegularExpression
                            var ticketMatch = ticketRegularExpression.Match(strResponse);

                            // Return the data in the "ticket" capture group named in the regular expression
                            return ticketMatch.Groups["ticket"].ToString();
                    }
                }
            }
            catch (WebException)
            {
            }

            // If there is an issue and the username, password, or url is invalid return empty string to signal a problem
            return string.Empty;
        }

        /// <summary>
        /// The get service ticket.
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        /// <param name="ticketGrantingTicket">
        /// The ticket granting ticket.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetServiceTicket(string server, string ticketGrantingTicket, string service)
        {
            // If the string parameters are null or empty return empy string.
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(ticketGrantingTicket) ||
                string.IsNullOrEmpty(service))
            {
                return string.Empty;
            }

            try
            {
                var url = string.Format("{0}/{1}", server, ticketGrantingTicket);
                var data = System.Web.HttpUtility.ParseQueryString(string.Empty);
                data.Add("service", service);

                var postDataString = data.ToString();
                var postDataByte = Encoding.ASCII.GetBytes(postDataString);

                var uriB = new UriBuilder(url);
                var request = (HttpWebRequest)WebRequest.Create(uriB.Uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postDataByte.Length;
                request.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                request.CookieContainer = new CookieContainer();

                // If not a unit test create a web response and get the response stream.
                var postStream = request.GetRequestStream();
                postStream.Write(postDataByte, 0, postDataByte.Length);
                postStream.Flush();
                postStream.Close();

                var response = request.GetResponse();
                var responseStream = response.GetResponseStream();
                
                if (responseStream != null)
                {
                    var responseReader = new StreamReader(responseStream);
                    var stringResponse = responseReader.ReadToEnd();
                    return stringResponse;
                }
            }
            catch
            {
                // If there is an error return an empty string
            }

            return string.Empty;
        }

        /// <summary>
        /// Authenticate the service ticket with the authentication endpoint.
        /// </summary>
        /// <param name="serviceTicket">
        /// The service ticket.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="CookieContainer"/>.
        /// </returns>
        public CookieContainer AuthenticateService(string serviceTicket, string url)
        {
            var cookieJar = new CookieContainer();
            var request = (HttpWebRequest)WebRequest.Create(new Uri(url + "?ticket=" + serviceTicket));
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookieJar;
            request.UserAgent =
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            var response = (HttpWebResponse)request.GetResponse();
            response.Close();

            return cookieJar;
        }
    }
}