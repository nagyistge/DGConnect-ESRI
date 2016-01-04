// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoredQuery.cs" company="DigitalGlobe">
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

namespace StoredQueries
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using NetworkConnections;

    using Newtonsoft.Json;

    /// <summary>
    /// Implements the IStoredQuery interface.
    /// </summary>
    public class StoredQuery:IStoredQuery
    {
        /// <summary>
        /// Class implementing the IGbdxComms interface to be used for 
        /// </summary>
        private IGbdxComms cloudComms;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredQuery"/> class.
        /// </summary>
        /// <param name="comms">
        /// Interface that defines how to talk with GBDX Cloud services. <see cref="IGbdxComms"/> 
        /// </param>
        public StoredQuery (IGbdxComms comms)
        {
            this.cloudComms = comms;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredQuery"/> class.
        /// </summary>
        public StoredQuery()
        {
            this.cloudComms = new GbdxComms();
        }

        #region IStoryQuery Implementation

        /// <summary>
        /// Get the queries stored in the stored query service.
        /// </summary>
        /// <param name="comms">
        /// <see cref="IGbdxComms"/> used for communicating with the stored query service.
        /// </param>
        /// <param name="netObject">
        /// the network object to be used in communicating with stored query service
        /// </param>
        /// <returns>
        /// List of queries stored in the service.
        /// </returns>
        public List<SavedQuery> GetQueries(IGbdxComms comms, NetObject netObject)
        {
            var output = comms.Request(netObject);

            // Request was good but no data was found.
            if (output.ResponseStatusCode == HttpStatusCode.NoContent || output.ErrorOccurred)
            {
                return null;
            }

            var queries = JsonConvert.DeserializeObject<List<SavedQuery>>(output.Result);
            queries.ForEach(item => item.SetDateTime());
            return queries;
        }

        /// <summary>
        /// Get the queries stored in the stored query service.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <returns>
        /// List of queries stored in the service.
        /// </returns>
        public List<SavedQuery> GetQueries(NetObject netObject)
        {
            return this.GetQueries(this.cloudComms, netObject);
        }

        /// <summary>
        /// Add/Update a query with the stored query service.
        /// </summary>
        /// <param name="comms">
        /// <see cref="IGbdxComms"/> used for communicating with the stored query service.
        /// </param>
        /// <param name="netObject">
        /// the network object to be used in communicating with stored query service
        /// </param>
        /// <param name="itemToAddUpdate">
        /// The item to add update.
        /// </param>
        /// <returns>
        /// True if the update/add was successful.
        /// </returns>
        public bool UpdateQuery(IGbdxComms comms, NetObject netObject, SavedQuery itemToAddUpdate)
        {
            try
            {
                // Serialize the workspace
                var serializedWorkspace = JsonConvert.SerializeObject(itemToAddUpdate,Formatting.None);

                // Send the request to the stored query service.
                var response = comms.PushRequest(netObject, serializedWorkspace);

                // Check to see if the response object is null.
                if (response == null)
                {
                    return false;
                }

                // Return the status of the update to the user.
                return response.ResponseStatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Add/Update a query with the stored query service.
        /// </summary>
        /// <param name="netObject">
        /// the network object to be used in communicating with stored query service
        /// </param>
        /// <param name="itemToAddUpdate">
        /// The item to add update.
        /// </param>
        /// <returns>
        /// True if the update/add was successful.
        /// </returns>
        public bool UpdateQuery(NetObject netObject, SavedQuery itemToAddUpdate)
        {
            return this.UpdateQuery(this.cloudComms, netObject,itemToAddUpdate);
        }

        /// <summary>
        /// Delete a query from the stored service.
        /// </summary>
        /// <param name="comms">
        /// <see cref="IGbdxComms"/> used for communicating with the stored query service.
        /// </param>
        /// <param name="netObject">
        /// the network object to be used in communicating with stored query service
        /// </param>
        /// <returns>
        /// True if the delete query method is successful.
        /// </returns>
        public bool DeleteQuery(IGbdxComms comms, NetObject netObject)
        {
            try
            {
                // Login in prior to sending a delete 
                if (!comms.AuthenticateNetworkObject(ref netObject))
                {
                    return false;
                }

                // Send the delete request to the service.
                var output = comms.DeleteRequest(netObject);

                // Check the response status code
                if (output.ResponseStatusCode != HttpStatusCode.NoContent && output.ResponseStatusCode != HttpStatusCode.OK)
                {
                    return false;
                }
            }
            catch
            {
                // Any errors occur return false.
                return false;
            }

            // Everything is all good.
            return true;
        }

        /// <summary>
        /// Delete a query from the stored service.
        /// </summary>
        /// <param name="netObject">
        /// the network object to be used in communicating with stored query service
        /// </param>
        /// <returns>
        /// True if the delete query method is successful.
        /// </returns>
        public bool DeleteQuery(NetObject netObject)
        {
            return this.DeleteQuery(this.cloudComms,netObject);
        }

        #endregion
    }
}
