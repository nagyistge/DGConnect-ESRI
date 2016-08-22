using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using Encryption;
using ESRI.ArcGIS.Carto;
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
        private string token;

        public CatalogTokenRefresh()
        {
            GetToken();
        }

        protected override void OnClick()
        {
            try
            {
                UpdateToken();
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        protected override void OnUpdate()
        {
        }

        /// <summary>
        /// Get GBDX Authentication Token to be used in refreshing the WMS layers
        /// </summary>
        private void GetToken()
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

            restClient.ExecuteAsync<AccessToken>(
                request,
                resp =>
                {
                    Jarvis.Logger.Info(resp.ResponseUri.ToString());

                    if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                    {
                        this.token = resp.Data.access_token;
                    }
                    else
                    {
                        this.token = string.Empty;
                    }
                });
        }

        private List<ILayer>  GetLayers()
        {
            List<ILayer> output = new List<ILayer>();

            for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
            {
                output.Add(ArcMap.Document.FocusMap.Layer[i]);
            }

            return output;
        }

        private void UpdateToken()
        {

            for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
            {
                var layer = ArcMap.Document.FocusMap.Layer[i];
                var compLayer = (ICompositeLayer) layer;

                if (compLayer == null)
                    continue;

                for (int j = 0; j < compLayer.Count; j++)
                {
                    var tempLayer = (ILayer) compLayer.Layer[j];

                    var dataLayer = (IDataLayer2) tempLayer;
                    Jarvis.Logger.Info( dataLayer.DataSourceName.NameString);

                    var tempLayer2 = (WMSGroupLayer)tempLayer;
                    
                    Jarvis.Logger.Info(" ");
                    Jarvis.Logger.Info(" ");
                    //Jarvis.Logger.Info(tempLayer2.WMSServiceDescription.);
                    Jarvis.Logger.Info(tempLayer2.WMSServiceDescription.WMSTitle);
                    Jarvis.Logger.Info(tempLayer2.WMSServiceDescription.WMSAbstract);
                    Jarvis.Logger.Info(tempLayer2.WMSServiceDescription.WMSName);
                    Jarvis.Logger.Info(tempLayer2.WMSServiceDescription.ToString());
                    //var childLayer = (IWMSLayer2)tempLayer2;

                    //if (childLayer == null)
                    //    continue;
                    //Jarvis.Logger.Info(" ");
                    //Jarvis.Logger.Info(" ");
                    //Jarvis.Logger.Info(childLayer.WMSServiceDescription.WMSTitle);
                    //Jarvis.Logger.Info(childLayer.WMSServiceDescription.WMSAbstract);
                    //Jarvis.Logger.Info(childLayer.WMSServiceDescription.WMSName);
                    //Jarvis.Logger.Info(childLayer.WMSServiceDescription.ToString());
                }
            }
        }
    }
}

