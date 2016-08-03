// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorIndexHelperTests.cs" company="DigitalGlobe">
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

namespace Gbdx.Vector_Index.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using ESRI.ArcGIS;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The vector index helper tests.
    /// </summary>
    [TestClass()]
    public class VectorIndexHelperTests
    {
        /// <summary>
        /// The syria local roads large file.
        /// </summary>
        private const string SyriaLocalRoadsLargeFile = "\\Testing Data\\syria_local_roads_all_pages.txt";

        /// <summary>
        /// The syria local roads large array.
        /// </summary>
        private string[] syriaLocalRoadsLargeArray;

        /// <summary>
        /// The init test.
        /// </summary>
        [TestInitialize()]
        public void InitTest()
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            AoInitialize aoInit = new AoInitializeClass();
            aoInit.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            syriaLocalRoadsLargeArray =
                File.ReadAllLines(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + SyriaLocalRoadsLargeFile);
        }

        /// <summary>
        /// The get source type test using type json.
        /// </summary>
        [TestMethod()]
        public void GetSourceTypeTestUsingTypeJSON()
        {
            const string json =
                "{\"shards\":40,\"data\":[{\"name\":\"Polygon\",\"count\":7081},{\"name\":\"PolyLine\",\"count\":1377},{\"name\":\"Point\",\"count\":9869}]}";
            var result = VectorIndexHelper.GetSourceType(json);

            bool goodToGo = true;

            if (!string.Equals("Polygon", result.Data[0].Name) || result.Data[0].Count != 7081)
            {
                goodToGo = false;
            }

            if (!string.Equals("PolyLine", result.Data[1].Name) || result.Data[1].Count != 1377)
            {
                goodToGo = false;
            }

            if (!string.Equals("Point", result.Data[2].Name) || result.Data[2].Count != 9869)
            {
                goodToGo = false;
            }

            if (result.Shards != 40)
            {
                goodToGo = false;
            }

            Assert.IsTrue(goodToGo);
        }

        /// <summary>
        /// The get source type test using type empty string.
        /// </summary>
        [TestMethod()]
        public void GetSourceTypeTestUsingTypeEmptyString()
        {
            string json = string.Empty;
            var result = VectorIndexHelper.GetSourceType(json);

            Assert.IsTrue(result == null);
        }

        /// <summary>
        /// The get source type test using type garbage string.
        /// </summary>
        [TestMethod()]
        public void GetSourceTypeTestUsingTypeGarbageString()
        {
            const string json =
                "[{\"name\":\"Type1\",\"count\":7},{\"name\":\"Type2\",\"count\":10},{\"name\":\"Type3\",\"count\":5}hns4erw35yt3hetahsrtjaewrfw24]";
            var result = VectorIndexHelper.GetSourceType(json);

            Assert.IsTrue(result == null);
        }

        /// <summary>
        /// The table join test.
        /// </summary>
        [TestMethod()]
        public void TableJoinTest()
        {
            const string json1 =
                "{\"displayFieldName\":\"\",\"features\":[{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:01Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.06571332 | Secondary\",\"vector.itemDate\":\"2015-02-03T18:31:50Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.39850000000007,35.264133868000044],[36.39830000000006,35.250133868000034],[36.394900000000064,35.198533868000084]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:58Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.20211339 | Secondary\",\"vector.itemDate\":\"2015-02-03T18:31:47Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[37.06150000000008,35.01393386800004],[37.06770000000006,35.02153386800006],[37.07310000000007,35.032133868000074],[37.08650000000006,35.07063386800007],[37.118100000000084,35.11613386800008],[37.141000000000076,35.136333868000065],[37.148100000000056,35.138633868000056],[37.18690000000004,35.13553386800004],[37.192900000000066,35.139733868000064]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:07Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.069533795 | Track/Trail\",\"vector.itemDate\":\"2015-02-03T18:31:58Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[37.08090000000004,35.679933868000035],[37.07350000000008,35.702533868000046],[37.06610000000006,35.710733868000034],[37.05370000000005,35.731033868000054],[37.04710000000006,35.73973386800009]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:56Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.019461615 | Track/Trail\",\"vector.itemDate\":\"2015-02-03T18:31:45Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.192300000000046,35.00003386800006],[36.20130000000006,35.00203386800007],[36.21140000000008,35.00033386800004]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:09Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.21097364 | Tertiary\",\"vector.itemDate\":\"2015-02-03T18:32:00Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.68070000000006,35.645533868000086],[36.66190000000006,35.67103386800005],[36.654100000000085,35.69613386800006],[36.652900000000045,35.70643386800009],[36.656300000000044,35.73413386800007],[36.65710000000007,35.766133868000054],[36.65710000000007,35.77663386800009],[36.672900000000084,35.81773386800006],[36.672900000000084,35.83363386800005],[36.66590000000008,35.84373386800007]]],\"spatialReference\":{\"wkid\":4326}}}],\"spatialReference\":{\"wkid\":4326},\"fields\":[{\"alias\":\"Item Date\",\"name\":\"vector.itemDate\",\"length\":50,\"type\":\"esriFieldTypeDate\"},{\"alias\":\"Ingest Date\",\"name\":\"vector.ingestDate\",\"length\":50,\"type\":\"esriFieldTypeDate\"},{\"alias\":\"Ingest Source\",\"name\":\"vector.ingestSource\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Name\",\"name\":\"vector.name\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Item Type\",\"name\":\"vector.itemType\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Format\",\"name\":\"vector.format\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Source\",\"name\":\"vector.source\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Original CRS\",\"name\":\"vector.originalCrs\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Text\",\"name\":\"vector.text\",\"length\":500,\"type\":\"esriFieldTypeString\"}],\"geometryType\":\"esriGeometryPolyline\"}";
            const string json2 =
                "{\"displayFieldName\":\"\",\"features\":[{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:55Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.055088654 | Tertiary\",\"vector.itemDate\":\"2015-02-03T18:31:44Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[37.20990000000006,34.98313386800004],[37.157500000000084,35.000133868000034]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:57Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.19256282 | Secondary\",\"vector.itemDate\":\"2015-02-03T18:31:46Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.51490000000007,34.96113386800005],[36.50730000000004,34.96803386800008],[36.498300000000086,34.97373386800007],[36.484300000000076,34.97573386800008],[36.46530000000007,34.98513386800005],[36.45330000000007,34.99113386800008],[36.42930000000007,35.000133868000034],[36.42630000000008,35.00113386800007],[36.394300000000044,35.008733868000036],[36.38430000000005,35.016133868000054],[36.36990000000009,35.02363386800005],[36.35390000000007,35.033033868000075],[36.343300000000056,35.042133868000064]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:08Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.27576804 | Primary\",\"vector.itemDate\":\"2015-02-03T18:31:59Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.61770000000007,35.81563386800008],[36.60090000000008,35.81073386800006],[36.572700000000054,35.788733868000065],[36.54890000000006,35.783033868000075],[36.539100000000076,35.783033868000075],[36.53210000000007,35.78573386800008],[36.52010000000007,35.78273386800004],[36.51430000000005,35.783033868000075],[36.50270000000006,35.790733868000075],[36.49530000000004,35.79513386800005],[36.487700000000075,35.79673386800005],[36.47830000000005,35.79173386800005],[36.459700000000055,35.77573386800009],[36.43930000000006,35.764733868000064],[36.42110000000008,35.758733868000036],[36.39570000000003,35.75513386800009],[36.386100000000056,35.76063386800007],[36.368100000000084,35.76003386800005]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:01Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.018284967 | Tertiary\",\"vector.itemDate\":\"2015-02-03T18:31:49Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.60170000000005,35.23033386800006],[36.586200000000076,35.22063386800005]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:01Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.059374582 | Tertiary\",\"vector.itemDate\":\"2015-02-03T18:31:50Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.36190000000005,35.265033868000046],[36.357300000000066,35.25053386800005],[36.35430000000008,35.219133868000085],[36.350100000000054,35.20723386800006]]],\"spatialReference\":{\"wkid\":4326}}}],\"spatialReference\":{\"wkid\":4326},\"fields\":[{\"alias\":\"Item Date\",\"name\":\"vector.itemDate\",\"length\":50,\"type\":\"esriFieldTypeDate\"},{\"alias\":\"Ingest Date\",\"name\":\"vector.ingestDate\",\"length\":50,\"type\":\"esriFieldTypeDate\"},{\"alias\":\"Ingest Source\",\"name\":\"vector.ingestSource\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Name\",\"name\":\"vector.name\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Item Type\",\"name\":\"vector.itemType\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Format\",\"name\":\"vector.format\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Source\",\"name\":\"vector.source\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Original CRS\",\"name\":\"vector.originalCrs\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Text\",\"name\":\"vector.text\",\"length\":500,\"type\":\"esriFieldTypeString\"}],\"geometryType\":\"esriGeometryPolyline\"}";
            const string json3 =
                "{\"displayFieldName\":\"\",\"features\":[{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:53Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.077347346 | Track/Trail\",\"vector.itemDate\":\"2015-02-03T18:31:43Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.74490000000009,34.95863386800005],[36.735100000000045,34.945133868000084],[36.72350000000006,34.934033868000085],[36.721100000000035,34.92613386800008],[36.72610000000009,34.909733868000046],[36.73610000000008,34.89333386800007]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:54Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.08151657 | Secondary\",\"vector.itemDate\":\"2015-02-03T18:31:43Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.56830000000008,34.90473386800005],[36.53590000000008,34.95213386800009],[36.52890000000008,34.95673386800007],[36.51870000000008,34.957133868000085],[36.51490000000007,34.96113386800005]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:06Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.06328892 | Tertiary\",\"vector.itemDate\":\"2015-02-03T18:31:58Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.22970000000004,35.71273386800004],[36.237100000000055,35.71613386800004],[36.25430000000006,35.716533868000056],[36.26430000000005,35.71153386800006],[36.27830000000006,35.70753386800004],[36.29030000000006,35.70973386800006]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:30:57Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.03134597 | Primary\",\"vector.itemDate\":\"2015-02-03T18:31:46Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.34690000000006,35.06313386800008],[36.373300000000086,35.080033868000044]]],\"spatialReference\":{\"wkid\":4326}}},{\"attributes\":{\"vector.itemType\":\"Roads\",\"vector.ingestDate\":\"2015-02-03T18:31:05Z\",\"vector.text\":\"SYR | Offfice of Humanitarian Affairs | 0.32352784 | Tertiary\",\"vector.itemDate\":\"2015-02-03T18:31:56Z\",\"vector.format\":\"FileGDB\",\"vector.ingestSource\":\"HGIS\",\"vector.source\":\"Offfice of Humanitarian Affairs\",\"vector.originalCrs\":\"EPSG:4326\",\"vector.name\":\"null\"},\"geometry\":{\"paths\":[[[36.187100000000044,35.53053386800008],[36.171700000000044,35.53573386800008],[36.15370000000007,35.540733868000075],[36.13210000000004,35.54373386800006],[36.11570000000006,35.54963386800006],[36.094700000000046,35.548133868000036],[36.06570000000005,35.54373386800006],[36.044500000000085,35.53963386800007],[36.022500000000036,35.534733868000046],[35.998500000000035,35.528733868000074],[35.97950000000009,35.522733868000046],[35.95650000000006,35.516733868000074],[35.93050000000005,35.51173386800008],[35.90650000000005,35.50473386800007],[35.88210000000004,35.49813386800008],[35.874100000000055,35.49273386800007]]],\"spatialReference\":{\"wkid\":4326}}}],\"spatialReference\":{\"wkid\":4326},\"fields\":[{\"alias\":\"Item Date\",\"name\":\"vector.itemDate\",\"length\":50,\"type\":\"esriFieldTypeDate\"},{\"alias\":\"Ingest Date\",\"name\":\"vector.ingestDate\",\"length\":50,\"type\":\"esriFieldTypeDate\"},{\"alias\":\"Ingest Source\",\"name\":\"vector.ingestSource\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Name\",\"name\":\"vector.name\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Item Type\",\"name\":\"vector.itemType\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Format\",\"name\":\"vector.format\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Source\",\"name\":\"vector.source\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Original CRS\",\"name\":\"vector.originalCrs\",\"length\":50,\"type\":\"esriFieldTypeString\"},{\"alias\":\"Text\",\"name\":\"vector.text\",\"length\":500,\"type\":\"esriFieldTypeString\"}],\"geometryType\":\"esriGeometryPolyline\"}";

            //var workspace =
            //    VectorIndexHelper.OpenWorkspace(
            //        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            //        "test.gdb");
            var qf = new QueryFilterClass();
            var table1 = VectorIndexHelper.GetTable(json1); //, "Streets", ref workspace);
            var table2 = VectorIndexHelper.GetTable(json2); //, "Streets1", ref workspace);
            var table3 = VectorIndexHelper.GetTable(json3); //, "Streets2", ref workspace);
            var tableList = new List<IRecordSet2> { table1, table2, table3 };

            VectorIndexHelper.CombineTables(ref tableList);
            var outputCount = ((IFeatureClass)tableList[0].Table).FeatureCount(qf);

            Assert.IsTrue(15 == outputCount);

            //var dataset = (IDataset) tableList[0];
            //dataset.Delete();
        }

        /// <summary>
        /// The data deserialization test.
        /// </summary>
        [TestMethod()]
        public void DataDeserializationTest()
        {
            if (Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\test2.gdb"))
            {
                Directory.Delete(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\test2.gdb", true);
            }

            const string json =
                "{\"displayFieldName\": \"\",\"features\": [{\"attributes\": {\"foo\": \"bar\",\"item.ADDRESS\": \"UNK\",\"item.CITY\": \"Damascus\",\"item.CONST\": \"National\",\"item.ISO3\": \"SYR\",\"item.LAT\": \"33.491367\",\"item.LAT.dbl\": 33.491367,\"item.LOC\": \"Exact\",\"item.LONG\": \"36.292465\",\"item.LONG.dbl\": 36.292465,\"item.NAME\": \"Syria Times\",\"item.PUB\": \"\u00a0Al-Wahda\",\"item.SOURCE\": \"ABYZnewslinks.com\",\"item.TYPE\": \"Newspaper\",\"vector.itemType\": \"Media Outlets\",\"vector.name\": \"Syria Times\",\"vector.source\": \"ABYZnewslinks.com\"},\"geometry\": {\"spatialReference\": {\"wkid\": 4326},\"x\": 36.29246600000005,\"y\": 33.491366000000085}},{\"attributes\": {\"item.ADDRESS\": \"UNK\",\"item.CITY\": \"Damascus\",\"item.CONST\": \"National\",\"item.ISO3\": \"SYR\",\"item.LAT\": \"33.491367\",\"item.LAT.dbl\": 33.491367,\"item.LOC\": \"Exact\",\"item.LONG\": \"36.292465\",\"item.LONG.dbl\": 36.292465,\"item.NAME\": \"Tishreen\",\"item.PUB\": \"\u00a0Al-Wahda\",\"item.SOURCE\": \"ABYZnewslinks.com\",\"item.TYPE\": \"Newspaper\",\"vector.itemType\": \"Media Outlets\",\"vector.name\": \"Tishreen\",\"vector.source\": \"ABYZnewslinks.com\"},\"geometry\": {\"spatialReference\": {\"wkid\": 4326},\"x\": 36.29246600000005,\"y\": 33.491366000000085}}],\"fields\": [{\"alias\": \"Foo\",\"length\": 50,\"name\": \"foo\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"Name\",\"length\": 50,\"name\": \"vector.name\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"Item Type\",\"length\": 50,\"name\": \"vector.itemType\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"Source\",\"length\": 50,\"name\": \"vector.source\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"attributes\",\"length\": 50,\"name\": \"attributes\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"LAT\",\"length\": 50,\"name\": \"item.LAT\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"SOURCE\",\"length\": 50,\"name\": \"item.SOURCE\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"NAME\",\"length\": 50,\"name\": \"item.NAME\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"LAT.dbl\",\"length\": 50,\"name\": \"item.LAT.dbl\",\"type\": \"esriFieldTypeDouble\"},{\"alias\": \"LONG.dbl\",\"length\": 50,\"name\": \"item.LONG.dbl\",\"type\": \"esriFieldTypeDouble\"},{\"alias\": \"ADDRESS\",\"length\": 50,\"name\": \"item.ADDRESS\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"LONG\",\"length\": 50,\"name\": \"item.LONG\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"ISO3\",\"length\": 50,\"name\": \"item.ISO3\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"CONST\",\"length\": 50,\"name\": \"item.CONST\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"CITY\",\"length\": 50,\"name\": \"item.CITY\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"TYPE\",\"length\": 50,\"name\": \"item.TYPE\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"PUB\",\"length\": 50,\"name\": \"item.PUB\",\"type\": \"esriFieldTypeString\"},{\"alias\": \"LOC\",\"length\": 50,\"name\": \"item.LOC\",\"type\": \"esriFieldTypeString\"}],\"geometryType\": \"esriGeometryPoint\",\"spatialReference\": {\"wkid\": 4326}}";

            var table1 = VectorIndexHelper.GetTable(json);
        }


        /// <summary>
        /// The data variation merging test 2.
        /// </summary>
        [TestMethod()]
        public void DataVariationMergingTest2()
        {
            //const string file = "\\Testing Data\\ReligionJson.txt";
            const string file = "\\Testing Data\\LanguagesJson.txt";
            var jsonData = File.ReadAllLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + file);

            var table1 = VectorIndexHelper.GetTable(jsonData[0]);
            var table2 = VectorIndexHelper.GetTable(jsonData[1]);
            var table3 = VectorIndexHelper.GetTable(jsonData[2]);
            var tableList = new List<IRecordSet2> { table1, table2, table3 };

            VectorIndexHelper.CombineTables(ref tableList);
        }
    }
}
