// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStoredQuery.cs" company="DigitalGlobe">
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
//   Defines the IStoredQuery type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace StoredQueries
{
    using System.Collections.Generic;

    using NetworkConnections;

    /// <summary>
    /// The StoredQuery interface.
    /// </summary>
    public interface IStoredQuery
    {
        /// <summary>
        /// Get queries from the stored query service.
        /// </summary>
        /// <param name="comms">
        /// Class to be used with communicating with the service. <see cref="IGbdxComms"/>.
        /// </param>
        /// <param name="netObject">
        /// The <see cref="NetObject"/> to be used to communicate with the query service
        /// </param>
        /// <returns>
        /// List of queries from the stored query service.
        /// </returns>
        List<SavedQuery> GetQueries(IGbdxComms comms, NetObject netObject);

        /// <summary>
        /// Get queries from the stored query service.
        /// </summary>
        /// <param name="netObject">
        /// The <see cref="NetObject"/> to be used to communicate with the query service
        /// </param>
        /// <returns>
        /// List of queries from the stored query service.
        /// </returns>
        List<SavedQuery> GetQueries(NetObject netObject);

        /// <summary>
        /// The update query.
        /// </summary>
        /// <param name="comms">
        /// The comms.
        /// </param>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="itemToAddUpdate">
        /// The item to add update.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool UpdateQuery(IGbdxComms comms, NetObject netObject, SavedQuery itemToAddUpdate);

        /// <summary>
        /// Update a query that has been stored by the stored query service
        /// </summary>
        /// <param name="netObject">
        /// The <see cref="NetObject"/> to be used to communicate with the query service
        /// </param>
        /// <param name="itemToAddUpdate">
        /// The item to add/update.
        /// </param>
        /// <returns>
        /// True if the update was successful.
        /// </returns>
        bool UpdateQuery(NetObject netObject, SavedQuery itemToAddUpdate);

        /// <summary>
        /// Delete a query contained within the stored query service
        /// </summary>
        /// <param name="comms">
        /// Class to be used with communicating with the stored query service. <see cref="IGbdxComms"/>.
        /// </param>
        /// <param name="netObject">
        /// The <see cref="NetObject"/> to be used to communicate with the query service
        /// </param>
        /// <returns>
        /// True if the query was successfully deleted.
        /// </returns>
        bool DeleteQuery(IGbdxComms comms, NetObject netObject);

        /// <summary>
        /// Delete a query contained within the stored query service
        /// </summary>
        /// <param name="netObject">
        /// The <see cref="NetObject"/> to be used to communicate with the query service
        /// </param>
        /// <returns>
        /// Returns true if the query was successfully deleted.
        /// </returns>
        bool DeleteQuery(NetObject netObject);
    }
}