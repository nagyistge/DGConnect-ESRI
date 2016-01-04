using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    using RestSharp.Contrib;
    using RestSharp.Deserializers;


    [TestClass()]
    public class DgxCommsTests
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
        public void AuthenticateNetworkObjectTest()
        {
            var netObj = new NetObject
            {
                BaseUrl = "https://iipbeta.digitalglobe.com",
                Password = "sys.service",
                User = "sys.service",
                AuthEndpoint = "/cas/oauth/token",
            };

            var temp = new RestResponse<AccessToken>
            {
                Data =
                    this.deserial.Deserialize<AccessToken>(new RestResponse
                    {
                        Content = Resource1.AuthenticateNetworkObjTestAccessGranted
                    }),
                StatusCode = HttpStatusCode.OK,
                Content = Resource1.AuthenticateNetworkObjTestAccessGranted,
                ContentLength =
                    Resource1.AuthenticateNetworkObjTestAccessGranted.Length
            };

            var mockRepo = MockRepository.GenerateStub<IRestClient>();
            mockRepo.BaseUrl = new Uri("https://iipbeta.digitalglobe.com");
            mockRepo.Stub(comms => comms.Execute<AccessToken>(Arg<IRestRequest>.Is.Anything))
                .Return(temp);

            // Remove the mockRepo from constructor to hit actual endpoint.
            var testClass = new DgxComms(mockRepo);
            var result = testClass.AuthenticateNetworkObject(ref netObj);


            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void AuthenticateNetworkObjectTestMissingCharacterFromUser()
        {
            var netObj = new NetObject
            {
                BaseUrl = "https://iipbeta.digitalglobe.com",
                Password = "sys.service",
                User = "sys.servic",
                AuthEndpoint = "/cas/oauth/token",
            };

            var temp = new RestResponse<AccessToken>
            {
                Data = null,
                StatusCode = HttpStatusCode.Unauthorized,
                Content = Resource1.AuthenticateNetworkObjTestMissingCharacterFromUser,
                ContentLength =
                    Resource1.AuthenticateNetworkObjTestMissingCharacterFromUser.Length
            };

            var mockRepo = MockRepository.GenerateStub<IRestClient>();
            mockRepo.BaseUrl = new Uri("https://iipbeta.digitalglobe.com");
            mockRepo.Stub(comms => comms.Execute<AccessToken>(Arg<IRestRequest>.Is.Anything))
                .Return(temp);

            // Remove the mockRepo from constructor to hit actual endpoint.
            var testClass = new DgxComms(mockRepo);
            var result = testClass.AuthenticateNetworkObject(ref netObj);


            Assert.IsFalse(result);
        }


        [TestMethod()]
        public void RequestTest()
        {
            var netObj = new NetObject
            {
                BaseUrl = "https://iipbeta.digitalglobe.com",
                Password = "sys.service",
                User = "sys.service",
                AuthEndpoint = "/cas/oauth/token",
                AddressUrl = "/insight-vector/api/esri/sources?left=-86.897&right=-77.036&upper=38.06&lower=33.613"
            };

            var temp = new RestResponse<AccessToken>
            {
                Data =
                    this.deserial.Deserialize<AccessToken>(new RestResponse
                    {
                        Content = Resource1.AuthenticateNetworkObjTestAccessGranted
                    }),
                StatusCode = HttpStatusCode.OK,
                Content = Resource1.AuthenticateNetworkObjTestAccessGranted,
                ContentLength =
                    Resource1.AuthenticateNetworkObjTestAccessGranted.Length
            };

            var sourceData = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = Resource1.GetRequestNormalResponse,
                ContentLength = Resource1.GetRequestNormalResponse.Length,
            };
            var mockRepo = MockRepository.GenerateStub<IRestClient>();
            mockRepo.BaseUrl = new Uri("https://iipbeta.digitalglobe.com");
            mockRepo.Stub(comms => comms.Execute<AccessToken>(Arg<IRestRequest>.Is.Anything))
                .Return(temp);

            mockRepo.Stub(comms => comms.Execute(Arg<IRestRequest>.Matches(arg => arg.Resource == netObj.AddressUrl)))
                .Return(sourceData);

            // Remove the mockRepo from constructor to hit actual endpoint.
            var testClass = new DgxComms(mockRepo);

            var result = testClass.Request(netObj);
            Assert.IsTrue(result.Result == Resource1.GetRequestNormalResponse);
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

            var testClass = new DgxComms();

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
