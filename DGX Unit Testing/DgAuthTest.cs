using NetworkConnections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace SMA_Unit_Testing
{
    /// <summary>
    ///This is a test class for DgAuthTest and is intended
    ///to contain all DgAuthTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DgAuthTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetTicketGrantingTicket
        ///</summary>
        [TestMethod()]
        public void GetTicketGrantingTicketTest()
        {
            // Data to be used to test the rest of the function.  
            const string stringData = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\"><html><head><title>201 The request has been fulfilled and resulted in a new resource being created</title></head><body><h1>TGT Created</h1><form action=\"https://jboss-cas.geoeyeanalytics.ec2:8443/cas/v1/tickets/TGT-663-alSn0acbOfj6oVf3Lq1MobsdrtrQgZLwCIlK95hbkEG0TmXpdn-cas.spadac.com\" method=\"POST\">Service:<input type=\"text\" name=\"service\" value=\"\"><br><input type=\"submit\" value=\"Submit\"></form></body></html>";
            var data =  Encoding.ASCII.GetBytes(stringData);
            const bool isUnitTest = true;
            const bool throwWebException = false;

            // To test using the actual server instead of the pre arranged data change 
            // isUnitTest to false and supply valid username/password.  The expected value will 
            // no longer match keep in mind.

            var target = new DgAuth(isUnitTest, data, throwWebException);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string username = "russell.wittmer";
            const string password = "!nsightC!oud";
            const string expected = "TGT-663-alSn0acbOfj6oVf3Lq1MobsdrtrQgZLwCIlK95hbkEG0TmXpdn-cas.spadac.com";
            var actual = target.GetTicketGrantingTicket(server, username, password);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for GetTicketGrantingTicket
        ///</summary>
        [TestMethod()]
        public void GetTicketGrantingTicketTestPostCodeNot201()
        {
            // Data to be used to test the rest of the function.  
            const string stringData = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\"><html><head><title>301 The request has been fulfilled and resulted in a new resource being created</title></head><body><h1>TGT Created</h1><form action=\"https://jboss-cas.geoeyeanalytics.ec2:8443/cas/v1/tickets/TGT-663-alSn0acbOfj6oVf3Lq1MobsdrtrQgZLwCIlK95hbkEG0TmXpdn-cas.spadac.com\" method=\"POST\">Service:<input type=\"text\" name=\"service\" value=\"\"><br><input type=\"submit\" value=\"Submit\"></form></body></html>";
            var data = Encoding.ASCII.GetBytes(stringData);
            const bool isUnitTest = true;
            const bool throwWebException = false;

            // To test using the actual server instead of the pre arranged data change 
            // isUnitTest to false and supply valid username/password.  The expected value will 
            // no longer match keep in mind.

            var target = new DgAuth(isUnitTest, data, throwWebException);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string username = "some.user";
            const string password = "!nsightC!oud";
            var actual = target.GetTicketGrantingTicket(server, username, password);
            Assert.AreEqual(string.Empty, actual);
        }
        /// <summary>
        ///A test for GetTicketGrantingTicket
        ///</summary>
        [TestMethod()]
        public void GetTicketGrantingTicketTestNullData()
        {
            var target = new DgAuth(true, null, false);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string username = "";
            const string password = "";
            var actual = target.GetTicketGrantingTicket(server, username, password);
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        ///A test for GetTicketGrantingTicket that throws a WebException.  The WebException is what get's thrown
        /// when the Username/Password is invalid.
        ///</summary>
        [TestMethod()]
        public void GetTicketGrantingTicketTestThrowWebException()
        {
            var target = new DgAuth(true, null, false);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string username = "";
            const string password = "";
            var actual = target.GetTicketGrantingTicket(server, username, password);
            Assert.AreEqual(string.Empty, actual);
        }


        /// <summary>
        ///A test for GetServiceTicket
        ///</summary>
        [TestMethod()]
        public void GetServiceTicketTest()
        {
            const bool isTest = true;
            var  data = Encoding.ASCII.GetBytes("ST-30186-kbe73Hi6XGaQSFS1HSQ2-cas.spadac.com");
            const bool throwException = false;
            var target = new DgAuth(isTest, data, throwException);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string ticketGrantingTicket = "TGT-663-alSn0acbOfj6oVf3Lq1MobsdrtrQgZLwCIlK95hbkEG0TmXpdn-cas.spadac.com";
            const string service = "https://www.someService.com";
            const string expected = "ST-30186-kbe73Hi6XGaQSFS1HSQ2-cas.spadac.com";
            var actual = target.GetServiceTicket(server, ticketGrantingTicket, service);
            Assert.AreEqual(expected, actual);
        }
        
        /// <summary>
        ///A test for GetServiceTicket.  A test was done with no granting ticket which causes the 
        /// Upload data function to throw a Exception.  This test simulates that by throwing the same 
        /// type of exception at the place of execution.
        ///</summary>
        [TestMethod()]
        public void GetServiceTicketTestNoGrantingTicket()
        {
            const bool isTest = true;
            var data = Encoding.ASCII.GetBytes("ST-30186-kbe73Hi6XGaQSFS1HSQ2-cas.spadac.com");
            const bool throwException = true;
            var target = new DgAuth(isTest, data, throwException);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string ticketGrantingTicket = "";
            const string service = "https://www.someService.com";
            var actual = target.GetServiceTicket(server, ticketGrantingTicket, service);
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        ///A test for GetServiceTicket.  A test was done with no granting ticket which causes the 
        /// Upload data function to throw a Exception.  This test simulates that by throwing the same 
        /// type of exception at the place of execution.
        ///</summary>
        [TestMethod()]
        public void GetServiceTicketTestNullData()
        {
            const bool isTest = true;
            const bool throwException = false;
            var target = new DgAuth(isTest, null, throwException);
            const string server = "https://insightcloud.digitalglobe.com/cas/v1/tickets";
            const string ticketGrantingTicket = "";
            const string service = "https://www.someService.com";
            var actual = target.GetServiceTicket(server, ticketGrantingTicket, service);
            Assert.AreEqual(string.Empty, actual);
        }
    }
}
