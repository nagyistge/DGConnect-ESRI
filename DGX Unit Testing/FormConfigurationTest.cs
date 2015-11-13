using Dgx.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SMA_Unit_Testing
{


    /// <summary>
    ///This is a test class for FormConfigurationTest and is intended
    ///to contain all FormConfigurationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FormConfigurationTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
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
        ///A test for ValidUsername method to validate a username of only alpha numeric values and the . character
        ///</summary>
        [TestMethod()]
        public void ValidUsernameTest()
        {
            const string username = "test.user";
            var actual = FormConfiguration.ValidUsername(username);
            Assert.IsTrue(actual);
        }
        /// <summary>
        ///A test for ValidUsername method using white space
        ///</summary>
        [TestMethod()]
        public void ValidUsernameTestWithWhitespace()
        {
            const string username = "test .user";
            var actual = FormConfiguration.ValidUsername(username);
            Assert.IsFalse(actual);
        }
        /// <summary>
        ///A test for ValidUsername method with numbers in the user name
        ///</summary>
        [TestMethod()]
        public void ValidUsernameTestWithNumbers()
        {
            const string username = "testuser123456";
            var actual = FormConfiguration.ValidUsername(username);
            Assert.IsFalse(actual);
        }
        /// <summary>
        ///A test for ValidUsername method using Special characters.
        ///</summary>
        [TestMethod()]
        public void ValidUsernameTestWithSpecialCharacters()
        {
            const string username = "testuser#&@^&*%#&*&@*&%&(*$&&#^$!^*$%^!%";
            var actual = FormConfiguration.ValidUsername(username);
            Assert.IsFalse(actual);
        }
    }
}
