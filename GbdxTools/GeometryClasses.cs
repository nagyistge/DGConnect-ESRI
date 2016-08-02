/*
 * Author: Russell Wittmer
 * Date: 08/02/2016
 */
namespace GbdxTools
{
    using System.Collections.Generic;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Newtonsoft.Json;

    public class GeometryClasses
    {
        public class MultiPolygonGeoJson
        {
            public string type { get; set; }
            public List<List<List<List<double>>>> coordinates { get; set; }
        }

        public class PolygonGeoJson
        {
            public string type { get; set; }
            public List<List<List<double>>> coordinates { get; set; }
        }

        public class GeoJsonBaseObject
        {
            public string type { get; set; }
            public List<object> coordinates { get; set; }
        }

        public class SpatialReference
        {
            public SpatialReference(int id)
            {
                this.wkid = id;
            }
            public int wkid { get; set; }
        }

        public class EsriPolygonJsonObject
        {
            public SpatialReference spatialReference { get; set; }
            public List<List<List<double>>> rings { get; set; }
        }

        private static bool IsClockwise(List<List<double>> ringToTest )
        {
            var output = false;

            double total = 0;
            int i = 0;
            List<double> pt1 = ringToTest[i];
            List<double> pt2;

            for (i = 0; i < ringToTest.Count - 1; i++)
            {
                pt2 = ringToTest[i + 1];
                total += (pt2[0] - pt1[0]) * (pt2[1] + pt1[1]);
                pt1 = pt2;
            }
            return (total >= 0);
        }
        
        private static PolygonGeoJson ConvertGeoJsonCoordsToEsriCoords(PolygonGeoJson poly)
        {
            for (int i = 0; i <= poly.coordinates.Count - 1; i ++)
            {
                // if the outer polygon isn't clock wise reverse the order of the coordinates.
                if (i == 0 && !IsClockwise(poly.coordinates[i]))
                {
                    poly.coordinates[i].Reverse();
                    continue;
                }
                // if the holes in the polygon are not counter-clockwise then revese the order.
                if (i > 0 && IsClockwise(poly.coordinates[i]))
                {
                    poly.coordinates[i].Reverse();
                }

            }
            return poly;
        }

        public static string GeojsonToEsriJson(PolygonGeoJson poly)
        {
            var convertedPoly = ConvertGeoJsonCoordsToEsriCoords(poly);

            var esriJsonObject = new EsriPolygonJsonObject
                                     {
                                         spatialReference = new SpatialReference(4326),
                                         rings = convertedPoly.coordinates
                                     };

            var outputString = JsonConvert.SerializeObject(esriJsonObject);

            return outputString;
        }

        public static IPolygon GeoJsonToEsriPolygon(string geoJson)
        {
            var poly = JsonConvert.DeserializeObject<PolygonGeoJson>(geoJson);
            var convertedPoly = GeojsonToEsriJson(poly);

            var jsonReader = new JSONReaderClass();
            jsonReader.ReadFromString(convertedPoly);
            var jsonDeserializer = new JSONDeserializerGdbClass();
            jsonDeserializer.InitDeserializer(jsonReader, null);
            IGeometry geometry = ((IExternalDeserializerGdb)jsonDeserializer).ReadGeometry(esriGeometryType.esriGeometryPolygon);
            IPolygon newPolygon = (IPolygon)geometry;
            return newPolygon;
        }
    }
}