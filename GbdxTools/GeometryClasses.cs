/*
 * Author: Russell Wittmer
 * Date: 08/02/2016
 */
namespace GbdxTools
{
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Newtonsoft.Json;

    public class GeometryClasses
    {
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

        private static string GeojsonToEsriJson(PolygonGeoJson poly)
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

        private static IPolygon GeoJsonToEsriPolygon(string geoJson)
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

        private static IPolygon EsriJsonToEsriPolygon(string esriJson)
        {
            var jsonReader = new JSONReaderClass();
            jsonReader.ReadFromString(esriJson);
            var jsonDeserializer = new JSONDeserializerGdbClass();
            jsonDeserializer.InitDeserializer(jsonReader, null);
            IGeometry geometry = ((IExternalDeserializerGdb)jsonDeserializer).ReadGeometry(esriGeometryType.esriGeometryPolygon);
            IPolygon newPolygon = (IPolygon)geometry;
            return newPolygon;
        }

        private static List<string> MultiPolygonObjectToEsriJson(MultiPolygonGeoJson multi)
        {
            List<string> esriJsonList = new List<string>();
            foreach (List<List<List<double>>> poly in multi.coordinates)
            {
                // Convert the individual polygons within multipolygon
                var newPoly = new PolygonGeoJson { type = "Polygon", coordinates = poly };

                // check coordinates to make sure they match ESRI requirement for outer vs inner rings
                var convertedPoly = ConvertGeoJsonCoordsToEsriCoords(newPoly);
                var esriPolygonJsonObject = new EsriPolygonJsonObject
                                                {
                                                    spatialReference = new SpatialReference(4326),
                                                    rings = convertedPoly.coordinates
                                                };
                var esriJson = JsonConvert.SerializeObject(esriPolygonJsonObject);
                if (!string.IsNullOrEmpty(esriJson))
                {
                    esriJsonList.Add(esriJson);
                }
            }
            return esriJsonList;
        }

        public static List<string> GeoJsonObjectToEsriJsonList(string aoiGeoJson)
        {
            List<string> output = new List<string>();

            var generalObject = JsonConvert.DeserializeObject<GeoJsonBaseObject>(aoiGeoJson);

            switch (generalObject.type)
            {
                case "Polygon":
                    var poly = JsonConvert.DeserializeObject<PolygonGeoJson>(aoiGeoJson);
                    output.Add(GeojsonToEsriJson(poly));
                    break;
                case "MultiPolygon":
                    var multi = JsonConvert.DeserializeObject<MultiPolygonGeoJson>(aoiGeoJson);
                    output.AddRange(MultiPolygonObjectToEsriJson(multi));
                    break;
            }
            return output;
        }

        public static List<IPolygon> AoiGeoJsonToEsriPolygons(string aoiGeoJson)
        {
            var polyList = GeoJsonObjectToEsriJsonList(aoiGeoJson);

            List<IPolygon> output = new List<IPolygon>();
            foreach (var poly in polyList)
            {
                output.Add(EsriJsonToEsriPolygon(poly));
            }
            return output;
        }
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

    public class MultiPolygonGeoJson
    {
        public string type { get; set; }
        public List<List<List<List<double>>>> coordinates { get; set; }
    }

    public class PolygonGeoJson : GeoJsonBaseObject
    {
        public string type { get; set; }
        public List<List<List<double>>> coordinates { get; set; }
    }

    public class GeoJsonBaseObject
    {
        public string type { get; set; }

        public List<object> coordinates;
    }
}