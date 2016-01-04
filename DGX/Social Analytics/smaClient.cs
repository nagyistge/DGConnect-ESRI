// --------------------------------------------------------------------------------------------------------------------
// <copyright file="smaClient.cs" company="DigitalGlobe">
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

using GbdxTools;

namespace Gbdx
{
    using System;

    using Gbdx.Properties;

    using Encryption;

    using ESRI.ArcGIS.Geometry;

    using NetworkConnections;

    /// <summary>
    /// The sma client.
    /// </summary>
    public class SmaClient
    {
        /// <summary>
        /// The _rows.
        /// </summary>
        private readonly int rows = 10;

        /// <summary>
        /// The _env.
        /// </summary>
        private readonly IEnvelope env = null;

        /// <summary>
        /// The _keywords.
        /// </summary>
        private string keywords = null;

        /// <summary>
        /// The _arc 2 circle.
        /// </summary>
        private ICircularArc arc2Circle = null;

        /// <summary>
        /// The _is rectangular.
        /// </summary>
        private bool isRectangular = false;

        /// <summary>
        /// The _start.
        /// </summary>
        private DateTime start;

        /// <summary>
        /// The _stop.
        /// </summary>
        private DateTime stop;

        /// <summary>
        /// The _tweet query.
        /// </summary>
        private bool tweetQuery = true;

        /// <summary>
        /// The _url.
        /// </summary>
        private string url = string.Empty;

        /// <summary>
        /// Wrapper class for the network code.
        /// </summary>
        private SmaComms comms = new SmaComms(Jarvis.LogFile, false);

        /// <summary>
        /// Initializes a new instance of the <see cref="SmaClient"/> class.
        /// </summary>
        /// <param name="keywords">
        /// The keywords.
        /// </param>
        /// <param name="rows">
        /// The rows.
        /// </param>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        public SmaClient(string keywords, int rows, IEnvelope env, DateTime start, DateTime stop)
        {
            this.keywords = keywords.Trim();
            this.rows = rows;
            this.env = env;
            this.isRectangular = true;
            this.start = start;
            this.stop = stop;
            this.AdjustUrl();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmaClient"/> class.
        /// </summary>
        /// <param name="keywords">
        /// The keywords.
        /// </param>
        /// <param name="rows">
        /// The rows.
        /// </param>
        /// <param name="circleArc">
        /// The circle arc.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        /// <param name="stop">
        /// The stop.
        /// </param>
        public SmaClient(string keywords, int rows, ICircularArc circleArc, DateTime start, DateTime stop)
        {
            this.keywords = keywords.Trim();
            this.rows = rows;
            this.arc2Circle = circleArc;
            this.start = start;
            this.stop = stop;
            this.AdjustUrl();
        }

        /// <summary>
        /// The set url.
        /// </summary>
        /// <param name="queryTweets">
        /// The query tweets.
        /// </param>
        public void SetUrl(bool queryTweets = true)
        {
            this.tweetQuery = queryTweets;
            this.AdjustUrl();
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <returns>
        /// The <see cref="NetworkResponse"/>.
        /// </returns>
        public NetObject Run()
        {
            return this.RunOrbit();
        }

        /// <summary>
        /// The adjust url.
        /// </summary>
        private void AdjustUrl()
        {
            if (this.tweetQuery)
            {
                this.url = this.url + "/app/broker/sma/sma/twitter/tweets?pageSize=";
            }
            else
            {
                this.url = this.url + "/app/broker/sma/sma/rss/sentences?pageSize=";
            }
        }

        /// <summary>
        /// The run orbit.
        /// </summary>
        /// <returns>
        /// The <see cref="NetworkResponse"/>.
        /// </returns>
        private NetObject RunOrbit()
        {
            var responseFromServer = new NetObject();
            responseFromServer.BaseUrl = DgxHelper.GetCasBaseEndpoint(GbdxSettings.Properties.Settings.Default);
            responseFromServer.AuthEndpoint = DgxHelper.GetCasAuthenticationEndpoint(GbdxSettings.Properties.Settings.Default);
            responseFromServer.TicketEndpoint = DgxHelper.GetCasTicketEndpoint(GbdxSettings.Properties.Settings.Default);
            responseFromServer.User = GbdxSettings.Properties.Settings.Default.username;

            var unEncryptedPassword = string.Empty;
            var success = Aes.Instance.Decrypt128(GbdxSettings.Properties.Settings.Default.password, out unEncryptedPassword);
            if (success)
            {
                responseFromServer.Password = unEncryptedPassword;
            }

            this.keywords = this.keywords.Replace(" ", "%20");
            if (this.isRectangular)
            {
                this.url = this.url + this.rows + "&bbox=" + this.env.XMin + "," +
                       this.env.YMin + "," +
                       this.env.XMax + "," +
                       this.env.YMax + "&datetimerange=" +
                       this.start.ToString("s") + ".000Z," +
                       this.stop.ToString("s") + ".000Z&query=" + this.keywords;
            }
            else
            {
                this.url = this.url + this.rows + "&point=" + this.arc2Circle.CenterPoint.Y + "," +
                       this.arc2Circle.CenterPoint.X + "&radius=" + this._getCircleRadius() + "km&datetimerange=" +
                       this.start.ToString("s") + ".000Z," +
                       this.stop.ToString("s") + ".000Z&query=" + this.keywords;
            }

            responseFromServer.AddressUrl = this.url;
            responseFromServer = this.comms.Request(responseFromServer);
            return responseFromServer;
        }

        /// <summary>
        /// The _get circle radius.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int _getCircleRadius()
        {
            IPolyline pline = new PolylineClass();

            IPoint fp = new PointClass();
            fp.PutCoords(this.arc2Circle.CenterPoint.X, this.arc2Circle.CenterPoint.Y);
            IPoint tp = new PointClass();
            tp.PutCoords(this.arc2Circle.Envelope.XMax, this.arc2Circle.CenterPoint.Y);

            pline.FromPoint = fp;
            pline.ToPoint = tp;
            ISpatialReferenceFactory spatialReferenceFactory =
                new SpatialReferenceEnvironment() as ISpatialReferenceFactory;
            IGeographicCoordinateSystem geographicCoordinateSystem =
                spatialReferenceFactory.CreateGeographicCoordinateSystem((int) esriSRGeoCSType.esriSRGeoCS_WGS1984);
            pline.SpatialReference = geographicCoordinateSystem;

            int result = (int) ArcUtility.GetLength(pline);
            if (result > 0)
            {
                return result;
            }

            return 1;
        }
    }
}
