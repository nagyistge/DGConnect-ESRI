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
                var errorOccurred = false;
                working = true;

                try
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        GetToken();
                    }

                    UpdateTokens2(ArcMap.Document.FocusMap);
                }
                catch (Exception error)
                {
                    errorOccurred = true;
                    Jarvis.Logger.Error(error);
                }
                finally
                {
                    working = false;
                }
                if(!errorOccurred)
                {
                    MessageBox.Show("Layer refresh complete");
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

        private static void UpdateTokens2(IMap map)
        {
            for (var i = 0; i < map.LayerCount; i++)
            {
                ReplaceIdahoIds(null, map.Layer[i], 0);
            }

        }

        private static void UpdateTokens(IMap map)
        {
            var catIDictionary = CheckLayers(map);
            bool error = false;

            List<IGroupLayer> grpLayers = new List<IGroupLayer>();

            // Don't remove layers in this loop because if there is an error we don't want to take data away from the users that wouldn't be easily recoverable from.
            foreach (var key in catIDictionary.Keys)
            {
                var tempLayer = RefreshLayer(key, catIDictionary[key], out error);

                if (error)
                {
                    MessageBox.Show("An error occurred while trying to refresh the WMS layers. \n Due to this error NO changes were made.\n  Please try again.");
                    throw new Exception("An error occurred while attempting to refresh WMS");
                }
                grpLayers.Add(tempLayer);
            }

            // remove the existing layers now that we have a suitable replacment for all
            foreach (var key in catIDictionary.Keys)
            {
                RemoveLayer(key);
            }

            // add the refreshed wms back to the mxd
            foreach (var layer in grpLayers)
            {
                ArcMap.Document.AddLayer(layer);
                ArcMap.Document.FocusMap.MoveLayer(layer, 0);
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

        private static IGroupLayer RefreshLayer(string catId, List<string> idahoIds, out bool unRecoverableError)
        {
            IGroupLayer groupLayer = new GroupLayerClass();
            groupLayer.Name = catId;
            foreach (var id in idahoIds)
            {
                var wmsMapLayer = CreateWmsMapLayer(id);

                if(wmsMapLayer == null)
                {
                    Jarvis.Logger.Info("An unknown issue occurred trying to establish a WMS layer link");
                    unRecoverableError = true;
                    return null;
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

            // No error occurred
            unRecoverableError = false;
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

        private static Dictionary<string, List<WmsLayerInfo>> CheckLayers2(IMap map)
        {
            Dictionary<string, List<WmsLayerInfo>> output = new Dictionary<string, List<WmsLayerInfo>>();

            for (int i = 0; i < map.LayerCount; i++)
            {
                var layer = map.Layer[i];

                var compLayer = (ICompositeLayer) layer;

                // if composite layer has more than 1 layer check the sub layers
                if (compLayer.Count > 1)
                {
                    //CheckSubLayers2(compLayer);
                }

            }
            return output;
        }

        private static List<WmsLayerInfo> CheckSubLayers2(ILayer layer, Dictionary<string, List<WmsLayerInfo>> layerDictionary)
        {
            var output = new List<WmsLayerInfo>();
            var tempLayer = (ICompositeLayer) layer;

            if (tempLayer.Count > 1)
            {
                for (int i = 0; i < tempLayer.Count; i++)
                {
                    var temp = tempLayer.Layer[i];
                    if (IdahoIdPattern.IsMatch(temp.Name))
                    {
                        output.Add(new WmsLayerInfo(i, temp.Name));
                    }
                }

            layerDictionary.Add(layer.Name, output);
            }
            else
            {
                
            }
            

            return output;
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


        private static int FindLayer(ILayer layer)
        {
            for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
            {
                var templayer = ArcMap.Document.FocusMap.Layer[i];
                if (templayer.Name != layer.Name)
                {
                    return i;
                }
            }
            return -1;
        }

        private static void ReplaceIdahoIds(ILayer parentLayer, ILayer childLayer, int count)
        {
            // idaho id layer name found.  so let's swap it out
            if (IdahoIdPattern.IsMatch(childLayer.Name))
            {
                IGroupLayer refreshedGroupLayer = null;

                if (parentLayer == null)
                {
                    refreshedGroupLayer = Refreshlayers(childLayer);
                    var index = FindLayer(childLayer);
                    ArcMap.Document.FocusMap.DeleteLayer(childLayer);
                    ArcMap.Document.FocusMap.MoveLayer(refreshedGroupLayer,index);
                }
                else
                {
                    refreshedGroupLayer = Refreshlayers(parentLayer);
                    var index = FindLayer(parentLayer);
                    ArcMap.Document.FocusMap.DeleteLayer(parentLayer);
                    ArcMap.Document.FocusMap.MoveLayer(refreshedGroupLayer, index);
                }

                if (refreshedGroupLayer == null)
                {
                    return;
                }
                
                ArcMap.Document.FocusMap.AddLayer(refreshedGroupLayer);

                return;
            }

            var childCompLayer = childLayer as ICompositeLayer;

            // check to see if the child has any children
            if (childCompLayer != null && childCompLayer.Count > 1)
            {
                // check children layers of current child
                ReplaceIdahoIds(childLayer, childCompLayer.Layer[0], 0);
            }
            else
            {
                // Keep checking the other layers of the parent layer
                var compLayer = (ICompositeLayer) parentLayer;
                if (count + 1 < compLayer.Count)
                {
                    ReplaceIdahoIds(parentLayer, compLayer.Layer[count + 1], count + 1);
                }
            }
        }

        private static ILayer RefreshIdahoId(string idahoID)
        {

            var wmsMapLayer = CreateWmsMapLayer(idahoID);

            if (wmsMapLayer == null)
            {
                return null;
            }

            var serviceDesc = wmsMapLayer.IWMSGroupLayer_WMSServiceDescription;
            ILayer wmsLayer = null;

            // add layers for the wms currently there will only be one
            for (var i = 0; i <= serviceDesc.LayerDescriptionCount - 1; i++)
            {
                var layerDesc = serviceDesc.LayerDescription[i];

                var grpLayer = wmsMapLayer.CreateWMSGroupLayers(layerDesc);
                for (var j = 0; j <= grpLayer.Count - 1; j++)
                {
                    wmsLayer = wmsMapLayer;
                    wmsMapLayer.Name = idahoID;
                }
            }

            SublayerVisibleOn(wmsLayer);
            return wmsLayer;
        }
        
        private static List<ILayer> GetLayers(ILayer parent)
        {
            var compLayer = parent as ICompositeLayer2;
            var output = new List<ILayer>();

            if (compLayer == null)
            {
                return output;
            }

            for (int i = 0; i <= compLayer.Count; i++)
            {
                output.Add(compLayer.Layer[i]);
            }

            return output;
        }

        private static IGroupLayer Refreshlayers(ILayer parent)
        {
            var compLayer = parent as ICompositeLayer2;
            List<ILayer> layerList;
            if (compLayer!= null && compLayer.Count > 1)
            {
                layerList = GetLayers(parent);

                // go through the layers and refresh them if they are idaho id named layers
                for (int i = 0; i < layerList.Count; i++)
                {
                    var layer = layerList[i];
                    if (IdahoIdPattern.IsMatch(layer.Name))
                    {
                        layerList[i] = RefreshIdahoId(layer.Name);
                    }
                }

                IGroupLayer groupLayer = new GroupLayerClass();
                groupLayer.Name = parent.Name;

                for (var j = 0; j < layerList.Count; j++)
                {
                    groupLayer.Add(layerList[j]);
                }

                return groupLayer;
            }

            return null;
        }
    }

    public class WmsLayerInfo
    {
        public int LayerIndex { get; set; }
        public string IdahoId { get; set; }

        public WmsLayerInfo(int index, string id)
        {
            this.LayerIndex = index;
            this.IdahoId = id;
        }
    }
}

