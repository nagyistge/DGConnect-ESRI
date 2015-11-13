using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkConnections;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using StoredQueriesTests;

namespace StoredQueries.Tests
{
    [TestClass()]
    public class StoredQueryTests
    {
        private NetObject networkObject;

        [TestInitialize()]
        public void TestInitialize()
        {
            networkObject = new NetObject()
            {
                UseDevAuthentication = true,
                CookieJar = new CookieContainer(),
                AuthEndpoint = "https://jboss-dev.geoeyeanalytics.ec2:8443/monocle-3/j_spring_cas_security_check",
                TicketEndpoint = "https://insightcloud.digitalglobe.com/cas/v1/tickets",
                TimeoutSetting = 8000,
                User = "sys.service",
                Password = "sys.service",
            };
        }

        [TestMethod()]
        public void TestGetQueries_NoneAvailable()
        {
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();
            mockRepo.Stub(comms => comms.Request(Arg<NetObject>.Is.Anything))
                .Return(new NetObject {ResponseStatusCode = HttpStatusCode.NoContent});

            var target = new StoredQuery(mockRepo);

            var output = target.GetQueries(mockRepo, networkObject);
            
            Assert.IsTrue(output == null);
        }

        [TestMethod()]
        public void TestGetQueries_TwoItemsReturned()
        {
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();
            mockRepo.Stub(comms =>
                comms.Request(Arg<NetObject>.Is.Anything))
                .Return(new NetObject
                {
                    ResponseStatusCode = HttpStatusCode.OK,
                    Result = TestData.TwoQueryReturned
                });
            var target = new StoredQuery(mockRepo);

            var output = target.GetQueries(mockRepo, networkObject);
            Assert.IsTrue(output[0].name == "Default Workspace" && output[1].name == "Ebola");
        }

        [TestMethod()]
        public void TestUpdateQueries_SendingNewItem()
        {
            var addMeQuery = new SavedQuery()
            {
                name = "AddedTest",
                query = "I am new here",
                timeline = new Timeline()
                {
                    Start = new DateTime(2015, 1, 1).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
                    End = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
                },
                date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
                properties = new Properties()
                {
                    EsriAddin = new EsriAddin()
                    {
                        DateLimit = "Custom",
                        QueryLimit = "5000"
                    }
                }
            };
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();

            mockRepo.Stub(comms => comms.PushRequest(Arg<NetObject>.Is.Anything, Arg<string>.Is.Anything))
                .Return(
                    new NetObject
                    {
                        ResponseStatusCode = HttpStatusCode.OK,
                        Result = TestData.OneItemAddedReturn
                    });

            var target = new StoredQuery(mockRepo);

            var output = target.UpdateQuery(mockRepo, networkObject, addMeQuery);
            Assert.IsTrue(output);
        }

        [TestMethod()]
        public void TestUpdateQueries_UpdatingItem()
        {
            var updateMeQuery = new SavedQuery()
            {
                name = "AddedTest",
                query = "I was new",
                timeline = new Timeline()
                {
                    Start = new DateTime(2015, 1, 1).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
                    End = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
                },
                date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
                properties = new Properties()
                {
                    EsriAddin = new EsriAddin()
                    {
                        DateLimit = "Custom",
                        QueryLimit = "5000"
                    }
                }
            };
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();

            mockRepo.Stub(comms => comms.PushRequest(Arg<NetObject>.Is.Anything, Arg<string>.Is.Anything))
                .Return(
                    new NetObject
                    {
                        ResponseStatusCode = HttpStatusCode.OK,
                        Result = TestData.OneItemUpdatedReturn
                    });

            var target = new StoredQuery(mockRepo);

            var output = target.UpdateQuery(mockRepo, networkObject, updateMeQuery);
            Assert.IsTrue(output);
        }

        [TestMethod()]
        public void TestDeleteQuery_DeleteItem()
        {
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();

            mockRepo.Stub(comms => comms.DeleteRequest(Arg<NetObject>.Is.Anything))
                .Return(
                    new NetObject
                    {
                        ResponseStatusCode = HttpStatusCode.OK,
                        Result = TestData.TwoQueryReturned
                    });

            mockRepo.Stub(comms => comms.AuthenticateNetworkObject(ref Arg<NetObject>.Ref(Is.Anything(), networkObject).Dummy)).Return(true);


            var target = new StoredQuery(mockRepo);
            var output = target.DeleteQuery(mockRepo, networkObject);
            Assert.IsTrue(output);
        }

        [TestMethod()]
        public void TestDeleteQuery_400DeleteItem()
        {
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();

            mockRepo.Stub(comms => comms.DeleteRequest(Arg<NetObject>.Is.Anything))
                .Return(
                    new NetObject
                    {
                        ResponseStatusCode = HttpStatusCode.BadRequest,
                        Result =""
                    });
            mockRepo.Stub(comms => comms.AuthenticateNetworkObject(ref Arg<NetObject>.Ref(Is.Anything(), networkObject).Dummy)).Return(true);
           
            var target = new StoredQuery(mockRepo);

            var output = target.DeleteQuery(mockRepo, networkObject);
            Assert.IsFalse(output);
        }

        [TestMethod()]
        public void TestDeleteQuery_204DeleteItem()
        {
            var mockRepo = MockRepository.GenerateStub<IDgxComms>();

            mockRepo.Stub(comms => comms.DeleteRequest(Arg<NetObject>.Is.Anything))
                .Return(
                    new NetObject
                    {
                        ResponseStatusCode = HttpStatusCode.NoContent,
                        Result = ""
                    });
            mockRepo.Stub(comms => comms.AuthenticateNetworkObject(ref Arg<NetObject>.Ref(Is.Anything(), networkObject).Dummy)).Return(true);

            var target = new StoredQuery(mockRepo);
            var output = target.DeleteQuery(mockRepo, networkObject);
            Assert.IsTrue(output);
        }
    }
}
