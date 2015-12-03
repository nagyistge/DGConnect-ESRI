// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DgAuth.cs" company="DigitalGlobe">
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
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    /// <summary>
    /// Handles Authentication via implementation of the ICas interface.
    /// </summary>
    public class DgAuth : ICas
    {
        /// <summary>
        /// the private instance of the DgAuth class being used in the singleton design pattern.
        /// </summary>
        private static DgAuth instance;

        /// <summary>
        /// Boolean value telling the class if it was instantiated via a unit test.
        /// </summary>
        private readonly bool isUnitTest;

        /// <summary>
        /// Unit test data to be passed in.
        /// </summary>
        private readonly byte[] unitTestData;

        /// <summary>
        /// Certain scenarios such as bad username/password throw a WebException all this does is if this value is true
        /// is simulate the exception.
        /// </summary>
        private readonly bool throwWebException;

        /// <summary>
        /// The logged in.
        /// </summary>
        private bool localLoggedIn = false;

        /// <summary>
        /// Service ticket to be used with other calls that require the CAS authentication.
        /// </summary>
        private string localServiceTicket = string.Empty;

        /// <summary>
        /// Cookie Container that will hold the cookies that the DG Auth Class will use.
        /// </summary>
        private CookieContainer localCookieJar = new CookieContainer();

        /// <summary>
        /// Initializes a new instance of the <see cref="DgAuth"/> class. 
        /// Unit Test constructor
        /// </summary>
        /// <param name="isTest">
        /// determines if this test is a unit test.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="throwException">
        /// Should a specific exception be thrown to simulate other real world scenarios.
        /// </param>
        public DgAuth(bool isTest, byte[] data, bool throwException)
        {
            this.isUnitTest = isTest;
            this.unitTestData = data;
            this.throwWebException = throwException;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DgAuth"/> class from being created. 
        /// Private default constructor being used in the singleton pattern implementation.
        /// </summary>
        private DgAuth()
        {
        }

        /// <summary>
        /// Gets the instance of the DGAuth class.
        /// </summary>
        /// <returns></returns>
        public static DgAuth Instance
        {
            get
            {
                return instance ?? (instance = new DgAuth());
            }
        }

        /// <summary>
        /// Gets or sets the cookie container.
        /// </summary>
        public CookieContainer CookieJar
        {
            get
            {
                return this.localCookieJar;
            }

            set
            {
                this.localCookieJar = value;
            }
        }

        /// <summary>
        /// Gets or sets the service ticket.
        /// </summary>
        public string ServiceTicket
        {
            get
            {
                return this.localServiceTicket;
            }

            set
            {
                this.localServiceTicket = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is logged in.
        /// </summary>
        public bool LoggedIn
        {
            get
            {
                return this.localLoggedIn;
            }

            set
            {
                this.localLoggedIn = value;
            }
        }

        #region ICas Implementation

        /// <summary>
        /// Gets the Ticket Granting Ticket which is needed in generating a service ticket
        /// </summary>
        /// <param name="server">
        /// Ticket server URL
        /// </param>
        /// <param name="username">
        /// Username for the Ticket Server
        /// </param>
        /// <param name="password">
        /// Password associated with the username for the ticket server
        /// </param>
        /// <returns>
        /// Ticket Granting Ticket to be used with granting a service ticket
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

                    // Uploads the post data to the specified server and retrieves the byte[] response
                    if (this.isUnitTest && this.throwWebException)
                    {
                        // Simulates a bad username/password.
                        throw new WebException();
                    }

                    // If unit test populate response with passed in unit test data else contact server to get data.
                    var response = this.isUnitTest ? this.unitTestData : client.UploadValues(server, "POST", data);

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
        /// Gets the service ticket.
        /// </summary>
        /// <param name="server">
        /// Main CAS Ticket Server
        /// </param>
        /// <param name="ticketGrantingTicket">
        /// Ticket Granted from GetTicketGrantingTicket method.
        /// </param>
        /// <param name="service">
        /// "The URL of the service being authorized"
        /// </param>
        /// <returns>
        /// The ticket for the service
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
                var data = HttpUtility.ParseQueryString(string.Empty);
                data.Add("service", service);

                var postDataString = data.ToString();
                var postDataByte = Encoding.ASCII.GetBytes(postDataString);

                // Code only excutes in a unit test environment and if the throwWebException flag is set true 
                if (this.isUnitTest && this.throwWebException)
                {
                    throw new WebException();
                }

                var uriB = new UriBuilder(url);
                var request = (HttpWebRequest)WebRequest.Create(uriB.Uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postDataByte.Length;
                request.UserAgent =
                    "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                request.CookieContainer = this.CookieJar;

                Stream responseStream;
                if (!this.isUnitTest)
                {
                    // If not a unit test create a web response and get the response stream.
                    var postStream = request.GetRequestStream();
                    postStream.Write(postDataByte, 0, postDataByte.Length);
                    postStream.Flush();
                    postStream.Close();

                    var response = request.GetResponse();
                    responseStream = response.GetResponseStream();
                }
                else
                {
                    // Is a unit test so transform the byte array data to a stream.
                    responseStream = new MemoryStream(this.unitTestData);
                }

                if (responseStream != null)
                {
                    var responseReader = new StreamReader(responseStream);
                    var stringResponse = responseReader.ReadToEnd();
                    return stringResponse;
                }
            }
            catch (Exception error)
            {
                // If there is an error return an empty string
            }

            return string.Empty;
        }

        /// <summary>
        /// The authenticate service.
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

        #endregion ICas Implementation
    }
}