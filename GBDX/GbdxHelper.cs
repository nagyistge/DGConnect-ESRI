// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdxHelper.cs" company="DigitalGlobe">
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
//   Defines the GbdxHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx
{
    using System.Net;
    using System.Windows;

    using Encryption;

    using Gbdx;

    using GbdxSettings.Properties;

    using GbdxTools;

    using NetworkConnections;

    using RestSharp;

    /// <summary>
    /// The gbdx cloud helper.
    /// </summary>
    public static class GbdxHelper
    {
        /// <summary>
        /// Manages which authentication endpoint gets returned based on user settings
        /// </summary>
        /// <param name="userSettings">user settings</param>
        /// <returns>authentication endpoint in string form</returns>
        internal static string GetAuthenticationEndpoint(GbdxSettings.Properties.Settings userSettings)
        {
            if (string.IsNullOrEmpty(userSettings.baseUrl))
            {
                return userSettings.authenticationServer;
            }

            return userSettings.authenticationServer;
        }

        /// <summary>
        /// Manages which base url gets returned based on user settings.
        /// </summary>
        /// <param name="userSettings">user settings</param>
        /// <returns>returns base url in string form</returns>
        internal static string GetEndpointBase(GbdxSettings.Properties.Settings userSettings)
        {
            if (string.IsNullOrEmpty(userSettings.baseUrl))
            {
                return userSettings.DefaultBaseUrl;
            }

            return userSettings.baseUrl;
        }
    }
}
