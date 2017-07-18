using Microsoft.VisualStudio.TestTools.UnitTesting;
using GBD;
using Newtonsoft.Json;

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdJarvisTests.cs" company="DigitalGlobe">
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
// --------------------------------------------------------------------------------------------------------------------

namespace GBD.Tests
{
    using ESRI.ArcGIS;
    using ESRI.ArcGIS.esriSystem;

    using GBDTests;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The gbd jarvis tests.
    /// </summary>
    [TestClass()]
    public class GbdJarvisTests
    {

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

        /// <summary>
        /// The convert wkt to esri test.
        /// </summary>
        [TestMethod()]
        public void GetPointsFromWKT()
        {
            //var output = GbdJarvis.ConvertWktToEsri(TestResource.wkt);

//            var output = GbdJarvis.GetPointsFromWkt(TestResource.wkt);

//            Assert.IsTrue(output.Count == 21);
        }

        [TestMethod()]
        public void GetWKTPointsTest()
        {
            var resp = JsonConvert.DeserializeObject<VectorServices.CatalogResponse>(TestResource.CatalogResponseItem);
            var result = GbdJarvis.GetWKTPoints(resp.geometry.coordinates);
            Assert.IsTrue(result.Count == 5 );
        }
    }
}
