// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetObject.cs" company="DigitalGlobe">
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
//   The net object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetworkConnections
{
    using System;
    using System.Net;

    /// <summary>
    /// The net object.
    /// </summary>
    public class NetObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetObject"/> class.
        /// </summary>
        public NetObject()
        {
            this.ErrorOccurred = false;
        }
        
        /// <summary>
        /// Gets or sets the authorization token. 
        /// </summary>
        public string AuthorizationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether error occurred.
        /// </summary>
        public bool ErrorOccurred { get; set; }

        /// <summary>
        /// Gets or sets the page item count.
        /// </summary>
        public int PageItemCount { get; set; }

        /// <summary>
        /// Gets or sets the address url.
        /// </summary>
        public string AddressUrl { get; set; }

        /// <summary>
        /// Gets or sets the cookie jar.
        /// </summary>
        public CookieContainer CookieJar { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the ticket endpoint.
        /// </summary>
        public string TicketEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the auth endpoint.
        /// </summary>
        public string AuthEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use DEV authentication.
        /// </summary>
        public bool UseDevAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the timeouts.
        /// </summary>
        public int Timeouts { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Gets or sets the timeout setting.
        /// </summary>
        public int TimeoutSetting { get; set; }

        /// <summary>
        /// Gets or sets the page id.
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// Gets or sets the response status code.
        /// </summary>
        public HttpStatusCode ResponseStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the base url.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the auth url.
        /// </summary>
        public string AuthUrl { get; set; }
    }
}