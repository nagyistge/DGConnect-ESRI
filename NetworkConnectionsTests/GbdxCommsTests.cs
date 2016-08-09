using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NetworkConnections;
using Rhino.Mocks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace NetworkConnections.Tests
{
    using System.IO;
    using System.Net;
    using System.Reflection;

    using ESRI.ArcGIS;
    using ESRI.ArcGIS.esriSystem;

    using Gbdx.Vector_Index;

    using NetworkConnectionsTests;

    using Newtonsoft.Json;

    using RestSharp;
    using RestSharp.Deserializers;


    [TestClass()]
    public class GbdxCommsTests
    {
        private JsonDeserializer deserial = new JsonDeserializer();

        [TestInitialize()]
        public void InitTest()
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            AoInitialize aoInit = new AoInitializeClass();
            aoInit.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
        }

        [TestMethod()]
        public void StagedRequestTest()
        {
            var netObj = new NetObject
            {
                BaseUrl = "https://iipbeta.digitalglobe.com",
                Password = "sys.service",
                User = "sys.service",
                AuthEndpoint = "/cas/oauth/token",
                AddressUrl =
                    "/insight-vector/api/esri/OSM/Polygon/Building/paging?left=36.2845510828066&upper=35.6019997390785&right=37.529485081057&lower=34.6768653000117&ttl=1m&count=100"
            };

            var testClass = new GbdxComms();

            var result = testClass.Request(netObj);

            var pageID = VectorIndexHelper.GetPageId(result.Result);

            netObj.PageId = pageID;
            netObj.AddressUrl = "/insight-vector/api/esri/paging";

            // Set the form parameters for paged requests.
            var formParams = HttpUtility.ParseQueryString(string.Empty);
            formParams.Add("ttl", "1m");
            formParams.Add("fields", "attributes");
            formParams.Add("pagingId", netObj.PageId);

            var result2 = testClass.StagedRequest(ref netObj, formParams);

        }
}
}
