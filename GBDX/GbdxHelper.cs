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
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Windows;
    using System.Windows.Forms;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.GISClient;

    using Gbdx;

    using GbdxSettings;
    using GbdxSettings.Properties;

    using GbdxTools;

    using NetworkConnections;

    using RestSharp;

    using MessageBox = System.Windows.Forms.MessageBox;

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
        /// <summary>
        ///     Recursively iterate through the layers and turn their visbility to true
        /// </summary>
        /// <param name="layer">layer to check for sub layers</param>
        private static void SublayerVisibleOn(ILayer layer)
        {
            var compLayer = layer as ICompositeLayer;

            if (compLayer == null)
            {
                return;
            }

            for (var i = 0; i < compLayer.Count; i++)
            {
                var subLayer = compLayer.Layer[i];

                // turn visibility on
                subLayer.Visible = true;

                // check to see if the layer has sub-layers
                var subComp = subLayer as ICompositeLayer;

                // if there are sub layers then enable them.
                if (subComp != null && subComp.Count > 0)
                {
                    SublayerVisibleOn(subComp as ILayer);
                }
            }
        }
        public static void AddIdahoWms(string idahoId, string groupLayerName, string token)
        {
            IGroupLayer groupLayer = new GroupLayerClass();
            groupLayer.Name = groupLayerName;

            var wmsMapLayer = new WMSMapLayerClass();

            // create and configure wms connection name, this is used to store the connection properties
            IWMSConnectionName pConnName = new WMSConnectionNameClass();
            IPropertySet propSet = new PropertySetClass();

            // create the idaho wms url
            var idahoUrl = string.Format(
                "http://idaho.geobigdata.io/v1/wms/idaho-images/{0}/{1}/mapserv?",
                idahoId, token);

            // setup the arcmap connection properties
            propSet.SetProperty("URL", idahoUrl);
            pConnName.ConnectionProperties = propSet;

            //uses the name information to connect to the service
            IDataLayer dataLayer = wmsMapLayer;
            try
            {
                dataLayer.Connect((IName)pConnName);
            }
            catch (Exception e)
            {
                Jarvis.Logger.Error("Problems connecting to WMS: " + e.Message);
            }

            // get wms service description
            var serviceDesc = wmsMapLayer.IWMSGroupLayer_WMSServiceDescription;

            ILayer wmsLayer = null;

            // add layers for the wms currently there will only be one.
            for (var i = 0; i <= serviceDesc.LayerDescriptionCount - 1; i++)
            {
                var layerDesc = serviceDesc.LayerDescription[i];

                var grpLayer = wmsMapLayer.CreateWMSGroupLayers(layerDesc);
                for (var j = 0; j <= grpLayer.Count - 1; j++)
                {
                    wmsLayer = wmsMapLayer;
                    wmsMapLayer.Name = idahoId;
                }
            }

            // turn on sub layers, add it to arcmap and move it to top of TOC
//            SublayerVisibleOn(wmsLayer);
            groupLayer.Add(wmsLayer);

            // turn on sub layers, add it to arcmap and move it to top of TOC
            ArcMap.Document.AddLayer(groupLayer);
            ArcMap.Document.FocusMap.MoveLayer(groupLayer, 0);
        }

        public static void AddIdahoWms(List<string> idahoIds, string groupLayerName, string token)
        {
            // Don't do anything if the aren't any idaho id's to work with
            if (idahoIds.Count <= 0)
            {
                return;
            }

            IGroupLayer groupLayer = new GroupLayerClass();
            groupLayer.Name = groupLayerName;
            foreach (var id in idahoIds)
            {
                var wmsMapLayer = new WMSMapLayerClass();

                // create and configure wms connection name, this is used to store the connection properties
                IWMSConnectionName pConnName = new WMSConnectionNameClass();
                IPropertySet propSet = new PropertySetClass();

                // create the idaho wms url
                var idahoUrl = string.Format(
                    "http://idaho.geobigdata.io/v1/wms/idaho-images/{0}/{1}/mapserv?",
                    id,
                    token);

                Jarvis.Logger.Info("Adding WMS Layer to: " + idahoUrl);

                // setup the arcmap connection properties
                propSet.SetProperty("URL", idahoUrl);
                pConnName.ConnectionProperties = propSet;

                //uses the name information to connect to the service
                IDataLayer dataLayer = wmsMapLayer;
                try
                {
                    dataLayer.Connect((IName)pConnName);
                }
                catch (Exception e)
                {
                    Jarvis.Logger.Error("Problems connecting to WMS: " + e.Message);
                }

                // get wms service description
                var serviceDesc = wmsMapLayer.IWMSGroupLayer_WMSServiceDescription;

                ILayer wmsLayer = null;

                // add layers for the wms currently there will only be one.
                for (var i = 0; i <= serviceDesc.LayerDescriptionCount - 1; i++)
                {
                    var layerDesc = serviceDesc.LayerDescription[i];

                    var grpLayer = wmsMapLayer.CreateWMSGroupLayers(layerDesc);
                    for (var j = 0; j <= grpLayer.Count - 1; j++)
                    {
                        wmsLayer = wmsMapLayer;
                        wmsMapLayer.Name = id;
                    }
                }

                // turn on sub layers, add it to arcmap and move it to top of TOC
                SublayerVisibleOn(wmsLayer);
                groupLayer.Add(wmsLayer);
            }

            // Check to see if the spatial refrence matches WGS84.  If not throw up a warning.
            if (ArcMap.Document.FocusMap.SpatialReference.FactoryCode != 4326)
            {
                var dialogResult = MessageBox.Show(
                    GbdxResources.projectionMismatchWarning,
                    "WARNING",
                    MessageBoxButtons.YesNo);

                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }

                // turn on sub layers, add it to arcmap and move it to top of TOC
                ArcMap.Document.AddLayer(groupLayer);
                ArcMap.Document.FocusMap.MoveLayer(groupLayer, 0);
            }
            else
            {
                // turn on sub layers, add it to arcmap and move it to top of TOC
                ArcMap.Document.AddLayer(groupLayer);
                ArcMap.Document.FocusMap.MoveLayer(groupLayer, 0);
            }
        }

    }
}
