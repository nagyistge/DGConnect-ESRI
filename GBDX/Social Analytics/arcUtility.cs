// --------------------------------------------------------------------------------------------------------------------
// <copyright file="arcUtility.cs" company="DigitalGlobe">
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
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;

    using ESRI.ArcGIS.ArcMapUI;
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Display;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.LocationUI;

    using Path = System.IO.Path;

    /// <summary>
    /// The arc utility.
    /// </summary>
    internal class ArcUtility
    {
        /// <summary>
        /// The hit regex.
        /// </summary>
        public static readonly Regex HitRegex = new Regex("\"hits\":{\"total\":(?<hits>.*?),");

        /// <summary>
        /// The number random.
        /// </summary>
        private static readonly Random NumberRandom = new Random();

        #region Viewable Colors

        /// <summary>
        /// The colors.
        /// </summary>
        private static readonly IList<Color> Colors = new[]
        {
            Color.Magenta,
            Color.Indigo,
            Color.Aquamarine,
            Color.OrangeRed,
            Color.Blue,
            Color.Lime,
            Color.MediumBlue,
            Color.DarkMagenta,
            Color.Crimson,
            Color.YellowGreen,
            Color.DodgerBlue,
            Color.Brown,
            Color.MediumVioletRed,
            Color.DarkSalmon,
            Color.Aqua,
            Color.MediumSeaGreen,
            Color.Navy,
            Color.Orange,
            Color.Yellow,
            Color.BlueViolet,
            Color.LightSkyBlue,
            Color.DeepSkyBlue,
            Color.CadetBlue,
            Color.DarkSeaGreen,
            Color.DarkRed,
            Color.Gold,
            Color.Salmon,
            Color.MediumSlateBlue,
            Color.Cyan,
            Color.DarkTurquoise,
            Color.MediumPurple,
            Color.MediumAquamarine,
            Color.LightCoral,
            Color.Coral,
            Color.SaddleBrown,
            Color.LawnGreen,
            Color.GreenYellow,
            Color.Fuchsia,
            Color.DarkSlateBlue,
            Color.Chartreuse,
            Color.Black,
            Color.CornflowerBlue,
            Color.DarkCyan,
            Color.SpringGreen,
            Color.DarkOrange,
            Color.DeepPink,
            Color.IndianRed,
            Color.DarkOrchid,
            Color.RoyalBlue,
            Color.Tomato,
            Color.DarkBlue,
            Color.Red,
            Color.SeaGreen,
            Color.ForestGreen,
            Color.DarkOliveGreen,
            Color.MediumOrchid,
            Color.MediumSpringGreen,
            Color.Orchid,
            Color.DarkViolet,
            Color.Teal,
            Color.Green,
            Color.HotPink,
            Color.MediumTurquoise
        };

        #endregion
        /// <summary>
        /// The validate WGS 84 polygon.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ValidateWgs84Polygon(IEnvelope env)
        {
            if (env.IsEmpty || env.XMin < -180 || env.XMax > +180 || env.YMin < -90 || env.YMax > +90)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The validate rounded WGS 84 polygon.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ValidateRoundedWgs84Polygon(IEnvelope env)
        {
            if (env.IsEmpty || Math.Round(env.XMin, 6) < -180 || Math.Round(env.XMax, 6) > +180 ||
                Math.Round(env.YMin, 6) < -90 || Math.Round(env.YMax, 6) > +90)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts screen coordinates to esri point coordinates
        /// </summary>
        /// <param name="screenPoint">
        /// the point on the screen
        /// </param>
        /// <param name="activeView">
        /// esri active view
        /// </param>
        /// <returns>
        /// Esri point transformed from screen coordinates
        /// </returns>
        public static IPoint GetMapCoordinatesFromScreenCoordinates(IPoint screenPoint, IActiveView activeView)
        {
            if (screenPoint == null || screenPoint.IsEmpty || activeView == null)
            {
                return null;
            }

            var screenDisplay = activeView.ScreenDisplay;
            var displayTransformation = screenDisplay.DisplayTransformation;

            return displayTransformation.ToMapPoint((int) screenPoint.X, (int) screenPoint.Y);
        }

        /// <summary>
        /// Deletes the targeted Element from the maps graphics container.
        /// </summary>
        /// <param name="activeView">
        /// active view of the map
        /// </param>
        /// <param name="target">
        /// element that is to be deleted.
        /// </param>
        public static void DeleteElementFromGraphicContainer(IActiveView activeView, IElement target)
        {
            // Delete element from graphics container.
            var graphicsContainer = activeView.GraphicsContainer;
            try
            {
                graphicsContainer.DeleteElement(target);

                // Do a partial refresh of only the layer that has been updated in this case
                // ViewGraphics.
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
            catch
            {
                // We had a problem in attempting the deletion of the target element.  
            }
        }
        
        /// <summary>
        /// The create table name.
        /// </summary>
        /// <param name="layerName">
        /// The layer name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string CreateTableName(string layerName)
        {
            var output = string.Format("SMA_{0}_{1}", layerName, DateTime.Now.ToString("ddMMMHHmmss"));
            string regexSearch = new string(Path.GetInvalidFileNameChars()) +
                                 new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            output = r.Replace(output, string.Empty);
            output = output.Replace(" ", "_");
            output = output.Replace("-", "_");
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            output = rgx.Replace(output, string.Empty);
            return output;
        }

        /// <summary>
        /// Returns the distance between the two points
        /// </summary>
        /// <param name="point1">
        /// Point 1
        /// </param>
        /// <param name="point2">
        /// Point 2
        /// </param>
        /// <returns>
        /// The distance between the two points
        /// </returns>
        public static double DistanceTo(IPoint point1, IPoint point2)
        {
            var a = point2.X - point1.X;
            var b = point2.Y - point1.Y;

            return Math.Sqrt((a*a) + (b*b));
        }

        /// <summary>
        /// The add XY event layer.
        /// </summary>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="tweetShow">
        /// The tweet show.
        /// </param>
        public static void AddXyEventLayer(ITable table, string query, bool tweetShow = true)
        {
            var mxdoc = ArcMap.Application.Document as IMxDocument;
            if (mxdoc != null)
            {
                var map = mxdoc.FocusMap;

                // Get the table named XYSample.txt
                var stTableCollection = map as IStandaloneTableCollection;

                // Get the table name object
                var dataset = table as IDataset;
                var tableName = dataset.FullName;

                // Specify the X and Y fields
                var xyEvent2FieldsProperties = new XYEvent2FieldsProperties() as IXYEvent2FieldsProperties;
                if (xyEvent2FieldsProperties != null)
                {
                    xyEvent2FieldsProperties.XFieldName = "x";
                    xyEvent2FieldsProperties.YFieldName = "y";
                    xyEvent2FieldsProperties.ZFieldName = string.Empty;

                    // Specify the projection
                    var spatialReferenceFactory = new SpatialReferenceEnvironment() as ISpatialReferenceFactory;
                    var projectedCoordinateSystem =
                        spatialReferenceFactory.CreateGeographicCoordinateSystem(
                            (int) esriSRGeoCSType.esriSRGeoCS_WGS1984);

                    // Create the XY name object as set it's properties
                    var xyEventSourceName = new XYEventSourceName() as IXYEventSourceName;
                    xyEventSourceName.EventProperties = xyEvent2FieldsProperties;
                    xyEventSourceName.SpatialReference = projectedCoordinateSystem;
                    xyEventSourceName.EventTableName = tableName;

                    IName xyName = xyEventSourceName as IName;
                    IXYEventSource xyEventSource = xyName.Open() as IXYEventSource;

                    // Create a new Map Layer
                    IFeatureLayer featureLayer = new FeatureLayer() as IFeatureLayer;
                    featureLayer.FeatureClass = xyEventSource as IFeatureClass;
                    featureLayer.Name = query;

                    // Add the layer extension (this is done so that when you edit
                    // the layer's Source properties and click the Set Data Source
                    // button, the Add XY Events Dialog appears)
                    ILayerExtensions layerExtensions = featureLayer as ILayerExtensions;
                    XYDataSourcePageExtension resPageExtension = new XYDataSourcePageExtension();
                    layerExtensions.AddExtension(resPageExtension);
                    
                    IGeoFeatureLayer geoLayer = (IGeoFeatureLayer) featureLayer;
                    ISimpleRenderer simpleRenderer = (ISimpleRenderer) geoLayer.Renderer;

                    var randomNumber = NumberRandom.Next(0, Colors.Count - 1);
                    var color = Colors[randomNumber];
                    
                    IRgbColor rgbColor = new RgbColorClass();
                    rgbColor.Blue = color.B;
                    rgbColor.Red = color.R;
                    rgbColor.Green = color.G;

                    IMarkerSymbol markerSymbol = new SimpleMarkerSymbolClass();
                    markerSymbol.Color = rgbColor;
                    markerSymbol.Size = 5;
                    simpleRenderer.Symbol = (ISymbol) markerSymbol;

                    try
                    {
                        map.AddLayer(featureLayer);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the length of a given Polyline. This function assumes that the Line is WGS84 Latitude, Longitude
        /// </summary>
        /// <param name="polyLine">
        /// Input Polyline
        /// </param>
        /// <returns>
        /// Length in KM
        /// </returns>
        public static double GetLength(IPolyline polyLine)
        {
            double distance = 0;

            //first check the Geometry's SR
            ISpatialReference sr = polyLine.SpatialReference;
            int factoryCode = sr.FactoryCode;
            if (factoryCode == 4326)
            {
                //this is in WGS 84 Lat long
                //iterate over the parts of the PolyLine
                var geomColl = (IGeometryCollection)polyLine;
                for (var i = 0; i < geomColl.GeometryCount; i++)
                {
                    var geom = geomColl.Geometry[i];
                    if (geom.GeometryType == esriGeometryType.esriGeometryPath)
                    {
                        //Now that we have a Path, we need to find the individual Segments
                        var segColl = (ISegmentCollection)geom;
                        for (int j = 0; j < segColl.SegmentCount; j++)
                        {
                            var segment = segColl.Segment[j];
                            if (segment.GeometryType == esriGeometryType.esriGeometryLine)
                            {
                                //Calculate the length of the segment and add it to running total
                                distance += GetHaverSineDistance(segment);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Polyline is not in EPSG:4326");
            }

            return distance;
        }

        /// <summary>
        /// Returns the distance in kilometers of a segment
        /// </summary>
        /// <param name="seg">
        /// The ISegment whose Length is to be calculated
        /// </param>
        /// <returns>
        /// The length in KM
        /// </returns>
        private static double GetHaverSineDistance(ISegment seg)
        {
            // It's based on the code present at: "http://megocode3.wordpress.com/2008/02/05/haversine-formula-in-c/"
            const double R = 6371;
            double dLat = ToRadian(seg.ToPoint.Y - seg.FromPoint.Y);
            double dLong = ToRadian(seg.ToPoint.X - seg.FromPoint.X);

            double a = (Math.Sin(dLat/2)*Math.Sin(dLat/2)) +
                       (Math.Cos(ToRadian(seg.FromPoint.Y))*Math.Cos(ToRadian(seg.ToPoint.Y))*
                       Math.Sin(dLong/2)*Math.Sin(dLong/2));

            double c = 2*Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R*c;

            return d;
        }

        /// <summary>
        /// Convert to Radians.
        /// </summary>
        /// <param name="val">
        /// The angle is degrees
        /// </param>
        /// <returns>
        /// Angle in Radians
        /// </returns>
        private static double ToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}
