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

namespace Dgx
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;
    using System.Windows.Forms;

    using Dgx.Properties;
    using Dgx.Vector_Index;

    using ESRI.ArcGIS.ArcMapUI;
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Display;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.LocationUI;

    using Newtonsoft.Json;

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
        /// The delete graphics refresh active view.
        /// </summary>
        /// <param name="activeView">
        /// The active view.
        /// </param>
        public static void DeleteGraphicsRefreshActiveView(IActiveView activeView)
        {
            var graphicsContainer = activeView.GraphicsContainer;
            graphicsContainer.DeleteAllElements();
            activeView.Refresh();
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
        /// The read rss response to JSON.
        /// </summary>
        /// <param name="responseFromServer">
        /// The response from server.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="xyTable">
        /// The XY table.
        /// </param>
        /// <returns>
        /// The <see cref="Exception"/>.
        /// </returns>
        public static Exception ReadRssResponseToJson(string responseFromServer, string tableName, ref ITable xyTable)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ResponseRss>(responseFromServer);
                xyTable = CreateTable(
                    DgxTools.Jarvis.OpenWorkspace(DGXSettings.Properties.Settings.Default.geoDatabase),
                    tableName,
                    LayerFields.CreateRssFields());

                var sizeResponse = response.Hits.Hits.Length;
                for (int i = 0; i < sizeResponse; i++)
                {
                    var row = xyTable.CreateRow();
                    row.Value[1] = response.Hits.Hits[i].Source.Geo.Lon;
                    row.Value[2] = response.Hits.Hits[i].Source.Geo.Lat;
                    row.Value[3] = response.Hits.Hits[i].Source.TitleNegative;
                    row.Value[4] = response.Hits.Hits[i].Source.DescriptionPositive;
                    row.Value[5] = response.Hits.Hits[i].Source.LuceneScore;
                    row.Value[6] = response.Hits.Hits[i].Source.NegativeSentiment;
                    row.Value[7] = response.Hits.Hits[i].Source.PositiveSentiment;
                    row.Value[8] = response.Hits.Hits[i].Source.DescriptionNegative;
                    row.Value[9] = response.Hits.Hits[i].Source.TitlePositive;
                    row.Value[10] = TrimRows(row.Fields.Field[10], response.Hits.Hits[i].Source.Url);
                    row.Value[11] = TrimRows(row.Fields.Field[11], response.Hits.Hits[i].Source.CountryCode);
                    row.Store();
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        /// <summary>
        /// The read rss response to JSON circle.
        /// </summary>
        /// <param name="responseFromServer">
        /// The response from server.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="radius">
        /// The radius.
        /// </param>
        /// <param name="center">
        /// The center.
        /// </param>
        /// <param name="origSpatialReference">
        /// The original spatial reference.
        /// </param>
        /// <param name="xyTable">
        /// The XY table.
        /// </param>
        /// <returns>
        /// The <see cref="Exception"/>.
        /// </returns>
        public static Exception ReadRssResponseToJsonCircle(
            string responseFromServer,
            string tableName,
            double radius,
            IPoint center,
            ISpatialReference origSpatialReference,
            ref ITable xyTable)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ResponseRss>(responseFromServer);

                xyTable = CreateTable(
                    DgxTools.Jarvis.OpenWorkspace(DGXSettings.Properties.Settings.Default.geoDatabase),
                    tableName,
                    LayerFields.CreateRssFields());

                var sizeResponse = response.Hits.Hits.Length;
                for (int i = 0; i < sizeResponse; i++)
                {
                    // Create a point and project it to the original spatial reference.
                    IPoint point = new PointClass();
                    point.PutCoords(response.Hits.Hits[i].Source.Geo.Lon, response.Hits.Hits[i].Source.Geo.Lat);
                    ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironment();
                    var geographicCoordinateSystem =
                        spatialReferenceFactory.CreateGeographicCoordinateSystem(
                            (int) esriSRGeoCSType.esriSRGeoCS_WGS1984);
                    
                    IGeometry geometry = point;
                    geometry.SpatialReference = geographicCoordinateSystem;
                    geometry.Project(origSpatialReference);
                    point = (IPoint) geometry;

                    // Calculate the distance from the center to the point.  If the distance is greater than the radius
                    // it will not be displayed to the end user.
                    var newDist = DistanceTo(center, point);

                    if (newDist <= radius)
                    {
                        var row = xyTable.CreateRow();
                        row.Value[1] = response.Hits.Hits[i].Source.Geo.Lon;
                        row.Value[2] = response.Hits.Hits[i].Source.Geo.Lat;
                        row.Value[3] = response.Hits.Hits[i].Source.TitleNegative;
                        row.Value[4] = response.Hits.Hits[i].Source.DescriptionPositive;
                        row.Value[5] = response.Hits.Hits[i].Source.LuceneScore;
                        row.Value[6] = response.Hits.Hits[i].Source.NegativeSentiment;
                        row.Value[7] = response.Hits.Hits[i].Source.PositiveSentiment;
                        row.Value[8] = response.Hits.Hits[i].Source.DescriptionNegative;
                        row.Value[9] = response.Hits.Hits[i].Source.TitlePositive;
                        row.Value[10] = TrimRows(row.Fields.Field[10], response.Hits.Hits[i].Source.Url);
                        row.Value[11] = TrimRows(row.Fields.Field[11], response.Hits.Hits[i].Source.CountryCode);
                        row.Store();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
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
        /// The trim rows.
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string TrimRows(IField field, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length > field.Length ? value.Substring(0, field.Length - 1) : value;
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
        /// Creates a table with some default fields.
        /// </summary>
        /// <param name="workspace">
        /// An IWorkspace2 interface
        /// </param>
        /// <param name="tableName">
        /// A System.String of the table name in the workspace. Example: "owners"
        /// </param>
        /// <param name="fields">
        /// An IFields interface or Nothing
        /// </param>
        /// <returns>
        /// An ITable interface or Nothing
        /// </returns>
        /// <remarks>
        /// Notes:
        /// (1) If an IFields interface is supplied for the 'fields' collection it will be used to create the
        ///    table. If a Nothing value is supplied for the 'fields' collection, a table will be created using 
        ///    default values in the method.
        /// (2) If a table with the supplied 'tableName' exists in the workspace an ITable will be returned.
        ///    if table does not exit a new one will be created.
        /// </remarks>
        public static ITable CreateTable(IWorkspace workspace, string tableName, IFields fields)
        {
            // create the behavior class-id for the featureclass
            UID uid = new UIDClass();

            if (workspace == null)
            {
                return null; // valid feature workspace not passed in as an argument to the method
            }

            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace; // Explicit Cast
            ITable table;

            uid.Value = "esriGeoDatabase.Object";

            IObjectClassDescription objectClassDescription = new ObjectClassDescriptionClass();

            // if a fields collection is not passed in then supply our own
            if (fields == null)
            {
                // create the fields using the required fields method
                fields = objectClassDescription.RequiredFields;
                IFieldsEdit fieldsEdit = (IFieldsEdit)fields; // Explicit Cast

                IField xfield = new FieldClass();
                IFieldEdit xfieldEdit = (IFieldEdit)xfield;
                xfieldEdit.Name_2 = "X";
                xfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                xfieldEdit.Length_2 = 20;

                IField yfield = new FieldClass();
                IFieldEdit yfieldEdit = (IFieldEdit)yfield;
                yfieldEdit.Name_2 = "Y";
                yfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                yfieldEdit.Length_2 = 20;

                // add field to field collection
                fieldsEdit.AddField(xfield);
                fieldsEdit.AddField(yfield);
                fields = (IFields)fieldsEdit; // Explicit Cast
            }

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.
            // create and return the table
            table = featureWorkspace.CreateTable(tableName, validatedFields, uid, null, string.Empty);
            return table;
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
