// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessToken.cs" company="DigitalGlobe">
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
//   The access token.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace NetworkConnections
{
    using System.Diagnostics.CodeAnalysis;
    
    /// <summary>
    /// The access token.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
    public class AccessToken
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        public string token_type { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        public string refresh_token { get; set; }

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        public int expires_in { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        public string scope { get; set; }
    }
}