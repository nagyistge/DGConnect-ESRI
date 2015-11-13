// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICas.cs" company="DigitalGlobe">
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
//   Defines the ICas type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetworkConnections
{
    /// <summary>
    /// The Cas interface.
    /// </summary>
    public interface ICas
    {
        /// <summary>
        /// The get ticket granting ticket.
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
        /// The <see cref="string"/>.
        /// </returns>
        string GetTicketGrantingTicket(string server, string username, string password);

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
        string GetServiceTicket(string server, string ticketGrantingTicket, string service);
    }
}
