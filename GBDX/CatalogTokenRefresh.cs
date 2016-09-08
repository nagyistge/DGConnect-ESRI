using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Encryption;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GISClient;
using GbdxSettings.Properties;
using GbdxTools;
using NetworkConnections;
using RestSharp;


namespace Gbdx
{
    public class CatalogTokenRefresh : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        /// <summary>
        /// GBDX Authentication Token
        /// </summary>
        private static string token = string.Empty;

        /// <summary>
        /// Pattern to identify a Idaho ID's
        /// </summary>
        private static Regex IdahoIdPattern =
            new Regex("([A-Za-z0-9]){8,}-([A-Za-z0-9]){4,}-([A-Za-z0-9]){4,}-([A-Za-z0-9]){4,}-([A-Za-z0-9]){12,}");

        /// <summary>
        /// Pattern to identify Catalog Ids
        /// </summary>
        private static Regex CatIdPattern = new Regex("(PAN|MS)\\s([A-Fa-f0-9]){16,}");

        private static bool working = false;

        public CatalogTokenRefresh()
        {
            GetToken();
        }

        protected override void OnClick()
        {

            if (!working)
            {
                working = true;

                try
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        GetToken();
                    }

                    UpdateTokens(ArcMap.Document.FocusMap);
                }
                catch (Exception error)
                {
                    Jarvis.Logger.Error(error);
                }
                finally
                {
                    MessageBox.Show("Layer refresh complete");
                    working = false;
                }
            }
        }

        protected override void OnUpdate()
        {
        }

        /// <summary>
        /// Get GBDX Authentication Token to be used in refreshing the WMS layers
        /// </summary>
        private static void GetToken()
        {
            IRestClient restClient = new RestClient(Settings.Default.AuthBase);

            string password;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out password);

            if (!result)
            {
                Jarvis.Logger.Warning("PASSWORD FAILED DECRYPTION");
                return;
            }

            var request = new RestRequest(Settings.Default.authenticationServer, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", string.Format("Basic {0}", Settings.Default.apiKey));
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", Settings.Default.username);
            request.AddParameter("password", password);

            var resp = restClient.Execute<AccessToken>(request);
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
            {
                token = resp.Data.access_token;
            }
            else
            {
                token = string.Empty;
            }
        }

        private static void UpdateTokens(IMap map)
        {
            var catIDictionary = CheckLayers(map);


            List<IGroupLayer> grpLayers = new List<IGroupLayer>();
            foreach (var key in catIDictionary.Keys)
            {
                //RemoveLayer(key);

                grpLayers.Add(RefreshLayer(key, catIDictionary[key]));

            }

        }

        private static WMSMapLayerClass CreateWmsMapLayer(string id, int attempt = 0)
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
            propSet.SetProperty("VERSION","1.3.0");
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
                
                if(attempt <= 5)
                {
                    Thread.Sleep(500);
                    attempt += 1;
                    return CreateWmsMapLayer(id, attempt);
                }
                return null;
            }

            return wmsMapLayer;
        }

        private static IGroupLayer RefreshLayer(string catId, List<string> idahoIds)
        {
            IGroupLayer groupLayer = new GroupLayerClass();
            groupLayer.Name = catId;
            foreach (var id in idahoIds)
            {
                var wmsMapLayer = CreateWmsMapLayer(id);

                if(wmsMapLayer == null)
                {
                    Jarvis.Logger.Info("something bad happened");
                    continue;
                }
                //// create and configure wms connection name, this is used to store the connection properties
                //IWMSConnectionName pConnName = new WMSConnectionNameClass();
                //IPropertySet propSet = new PropertySetClass();

                //// create the idaho wms url
                //var idahoUrl = string.Format(
                //    "http://idaho.geobigdata.io/v1/wms/idaho-images/{0}/{1}/mapserv?",
                //    id,
                //    token);

                //Jarvis.Logger.Info("Adding WMS Layer to: " + idahoUrl);

                //// setup the arcmap connection properties
                //propSet.SetProperty("URL", idahoUrl);
                //pConnName.ConnectionProperties = propSet;

                ////uses the name information to connect to the service
                //IDataLayer dataLayer = wmsMapLayer;
                //try
                //{
                //    dataLayer.Connect((IName)pConnName);
                //}
                //catch (Exception e)
                //{
                //    Jarvis.Logger.Error("Problems connecting to WMS: " + e.Message);
                //    if (attempts <= 5)
                //    {
                //        RefreshLayer(catId,idahoIds,token, attempts++);
                //    }
                //    return;
                //}

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
            // turn on sub layers, add it to arcmap and move it to top of TOC
        //    ArcMap.Document.AddLayer(groupLayer);
        //    ArcMap.Document.FocusMap.MoveLayer(groupLayer, 0);
            return groupLayer;
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

        /// <summary>
        /// Sort through the layers and find which ones came from adding Idaho IDs to the map.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private static Dictionary<string, List<string>> CheckLayers(IMap map)
        {
            var catIDictionary = new Dictionary<string, List<string>>();

            for (int i = 0; i < map.LayerCount; i++)
            {
                var layer = map.Layer[i];

                if (CatIdPattern.IsMatch(layer.Name))
                {
                    var subLayers = CheckSubLayers(layer as ICompositeLayer);

                    if (subLayers.Count > 0)
                    {
                        catIDictionary.Add(layer.Name, subLayers);
                    }
                }
            }
            return catIDictionary;
        }

        /// <summary>
        /// Search for and delete provided layer.
        /// </summary>
        /// <param name="layerName"></param>
        private static void RemoveLayer(string layerName)
        {
            int index = -1;

            ILayer targetLayer = null;

            // search through the layer names looking for the proper layer to delete.
            for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
            {
                targetLayer = ArcMap.Document.FocusMap.Layer[i];
                if (targetLayer.Name == layerName)
                {
                    break;
                }
            }

            // if the layer was found delete it
            if (targetLayer != null)
            {
                ArcMap.Document.FocusMap.DeleteLayer(targetLayer);
            }
        }

        /// <summary>
        /// Check for sub layers that have a idaho id for a layer name
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private static List<string> CheckSubLayers(ICompositeLayer layer)
        {
            var output = new List<string>();
            if (layer.Count > 1)
            {
                for (int i = 0; i < layer.Count; i++)
                {
                    var tempLayer = layer.Layer[i];

                    if (IdahoIdPattern.IsMatch(tempLayer.Name))
                    {
                        output.Add(tempLayer.Name);
                    }
                }
            }
            return output;
        }
    }
}

