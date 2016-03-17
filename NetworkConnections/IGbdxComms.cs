// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGbdxComms.cs" company="DigitalGlobe">
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
//   The GbdxComms interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetworkConnections
{
    using System.Collections.Specialized;
    using System.Net;

    using RestSharp;

    /// <summary>
    /// The IGbdxComms interface.
    /// </summary>
    public interface IGbdxComms
    {
        /// <summary>
        /// The request.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        NetObject Request(NetObject netObject);

        /// <summary>
        /// The staged request for data retrieval from the UVI service
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="formParms">
        /// Required form parameters.
        /// </param>
        /// <returns>
        /// True if the request was successful.
        /// </returns>
        bool StagedRequest(ref NetObject netObject, NameValueCollection formParms);

        /// <summary>
        /// The push request.
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
        NetObject PushRequest(NetObject netObject, string bodyPostData);

        NetObject PushRequest(NetObject netObject);

        /// <summary>
        /// The delete request.
        /// </summary>
        /// <param name="netobject">
        /// The net object.
        /// </param>
        /// <returns>
        /// The <see cref="NetObject"/>.
        /// </returns>
        NetObject DeleteRequest(NetObject netobject);

        /// <summary>
        /// The authenticate network object.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool AuthenticateNetworkObject(ref NetObject netObject);

        /// <summary>
        /// Generic Post method.  The Generic type will be returned when the data received is in JSON.
        /// </summary>
        /// <param name="netObject">
        /// The net object.
        /// </param>
        /// <param name="jsonDataPayLoad">
        /// The JSON data pay load.
        /// </param>
        /// <typeparam name="T">Generic Type that will be returned.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        T Post<T>(NetObject netObject, string jsonDataPayLoad);

        IRestClient GetClient();

        /// <summary>
        /// Get the GBD access token.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetAccessToken();

        /// <summary>
        /// Upload file to vector services
        /// </summary>
        /// <param name="netObject">contains authentication information required to perform the upload</param>
        /// <param name="filepath">path of the file to be uploaded</param>
        /// <returns></returns>
        HttpStatusCode UploadFile(NetObject netObject, string filepath);
    }
}