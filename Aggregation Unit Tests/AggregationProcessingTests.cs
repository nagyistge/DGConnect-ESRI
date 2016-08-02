using System;
using System.Text;
using System.Collections.Generic;
using Aggregations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using RestSharp.Deserializers;

namespace Aggregation_Unit_Tests
{
    using System.Linq;

    using ESRI.ArcGIS;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.Geodatabase;

    /// <summary>
    /// Summary description for AggregationProcessingTests
    /// </summary>
    [TestClass]
    public class AggregationProcessingTests
    {
        private JsonDeserializer deserial = new JsonDeserializer();

        public AggregationProcessingTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        /// <summary>
        /// The init test.
        /// </summary>
        [TestInitialize()]
        public void InitTest()
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            AoInitialize aoInit = new AoInitializeClass();
            aoInit.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
        }

        [TestMethod]
        public void TestProcessAggregations()
        {
            AggregationHelper testTarget = new AggregationHelper();

            var mog = deserial.Deserialize<MotherOfGodAggregations>(new RestResponse<MotherOfGodAggregations>
            {
                Content = TestResources.MogJson
            });
            }

        [TestMethod]
        public void TestJsonDeserializer()
        {
            var jsonGeometryPoint = TestResources.testData;
            //string jsonGeometryPoint = "{\"x\" : -118.15, \"y\" : 33.80, \"spatialReference\" : {\"wkid\" : 4326}}";
            var jsonReader = new JSONReaderClass();
            jsonReader.ReadFromString(jsonGeometryPoint);
            var jsonDeserializer = new JSONDeserializerGdbClass();
            jsonDeserializer.InitDeserializer(jsonReader, null);
            IGeometry geometry = ((IExternalDeserializerGdb)jsonDeserializer).ReadGeometry(esriGeometryType.esriGeometryPolygon);
            IPolygon point = (IPolygon)geometry;
            //Console.Write("X:{0} - Y:{1} - Factory Code: {2}", point.X, point.Y, point.SpatialReference.FactoryCode);
            //Console.Read();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestSingleValueAggregation()
        {
            var mog = this.deserial.Deserialize<MotherOfGodAggregations>(
                    new RestResponse<MotherOfGodAggregations> { Content = TestResources.sentimentAggregation });
            var output = new Dictionary<string, Dictionary<string, double>>();
            var fieldNames = new Dictionary<string, string>();
            AggregationHelper.ProcessAggregations(mog.aggregations,0,ref output,string.Empty,false,ref fieldNames);

        }

        [TestMethod]
        public void TestNestedAggregations()
        {
            var mog =
                this.deserial.Deserialize<MotherOfGodAggregations>(
                    new RestResponse<MotherOfGodAggregations> { Content = TestResources.doubleAggregation });
            var output = new Dictionary<string, Dictionary<string, double>>();
            var fieldNames = new Dictionary<string,string>();
            AggregationHelper.ProcessAggregations(mog.aggregations,0,ref output, string.Empty, false, ref fieldNames);

            bool success = false;
            foreach (var item in output.Keys)
            {
                if (output[item].Keys.Any(subItem => subItem == "HGIS"))
                {
                    success = true;
                }

                if (success)
                {
                    break;
                }
            }

            Assert.IsTrue(success);
        }
    }
}
