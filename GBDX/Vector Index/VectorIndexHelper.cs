// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorIndexHelper.cs" company="DigitalGlobe">
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

using GbdxTools;

namespace Gbdx.Vector_Index
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Script.Serialization;

    using ESRI.ArcGIS.ArcMapUI;
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Display;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Logging;

    using Newtonsoft.Json;

    /// <summary>
    /// The vector index helper.
    /// </summary>
    public static class VectorIndexHelper
    {
        /// <summary>
        /// The JSON serializer.
        /// </summary>
        private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer
                                                                          {
                                                                              MaxJsonLength =
                                                                                  int.MaxValue
                                                                          };

        /// <summary>
        /// The spatial reference factory.
        /// </summary>
        private static readonly ISpatialReferenceFactory SpatialReferenceFactory = new SpatialReferenceEnvironment();

        /// <summary>
        /// The projected coordinate system.
        /// </summary>
        private static readonly IGeographicCoordinateSystem ProjectedCoordinateSystem =
            SpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly Logger Logger = new Logger(Jarvis.LogFile, false);

        #region Public Methods

        /// <summary>
        /// Creates a feature layer from the feature class.
        /// </summary>
        /// <param name="featureClass">
        /// the feature class that the layer will use as a data source
        /// </param>
        /// <param name="name">
        /// Name of the feature layer
        /// </param>
        /// <returns>
        /// Feature Layer that can be added to ArcMap
        /// </returns>
        public static ILayer CreateFeatureLayer(IFeatureClass featureClass, string name)
        {
            IFeatureLayer featureLayer = new FeatureLayerClass();
            featureLayer.FeatureClass = featureClass;
            var layer = (ILayer)featureLayer;
            layer.Name = name;
            return layer;
        }

        /// <summary>
        /// Takes a JSON string and converts into a RecordSet.  From there the record set can be transformed into tables or feature classes as needed
        /// </summary>
        /// <param name="json">
        /// JSON string
        /// </param>
        /// <returns>
        /// ESRI IRecordSet2
        /// </returns>
        public static IRecordSet2 GetTable(string json)
        {
            try
            {
                // Establish the Json Reader for ESRI
                var jsonReader = new JSONReaderClass();
                jsonReader.ReadFromString(json);

                var jsonConverterGdb = new JSONConverterGdbClass();
                IPropertySet originalToNewFieldMap;
                IRecordSet recorset;

                // Convert the JSON to RecordSet
                jsonConverterGdb.ReadRecordSet(jsonReader, null, null, out recorset, out originalToNewFieldMap);

                // Cast the Recordset as RecordSet2
                var recordSet2 = recorset as IRecordSet2;

                return recordSet2;
            }
            catch (Exception error)
            {
                Logger.Error(error);
                return null;
            }
        }

        /// <summary>
        /// Combines all the IRecordSet2 items in the table into the first item on the table.  So if the table has 50 elements then the 0 element will have all the data from the other 49.
        /// </summary>
        /// <param name="tables">
        /// List of IRecordSet2 elements
        /// </param>
        public static void CombineTables(ref List<IRecordSet2> tables)
        {
            try
            {
                // Blank query filter to get everything in the featureclass or table
                var queryFilter = new QueryFilterClass();

                // extract the first item on the table to be come the master
                var masterFeature = (IFeatureClass)tables[0].Table;
                var insertCursor = masterFeature.Insert(true);

                var featureBuffer = masterFeature.CreateFeatureBuffer();
                for (var i = 0; i <= tables.Count - 1; i++)
                {
                    try
                    {
                        if (i == 0)
                        {
                            continue;
                        }

                        // get the feature whose attributes will be copied to the master.
                        var searchFeature = (IFeatureClass)tables[i].Table;

                        // Search cursor fo the searchFeature.
                        var searchCursor = searchFeature.Search(queryFilter, true);

                        IFeature row;

                        // Search featureclass until all features have been copied.
                        while ((row = searchCursor.NextFeature()) != null)
                        {
                            // Copy all the fields from the searchFeature to the master.
                            AddFields(ref featureBuffer, row);

                            // Officially insert the feature into the feature buffer
                            insertCursor.InsertFeature(featureBuffer);
                        }

                        // Tell arcobjects that the searchcursor is no longer needed
                        Marshal.FinalReleaseComObject(searchCursor);
                    }
                    catch (Exception error)
                    {
                        Logger.Error(error);
                    }
                }

                // One last flush to push all the changes to the master feature
                insertCursor.Flush();

                // tell arcobjects that the insert cursor is no longer need.
                Marshal.FinalReleaseComObject(insertCursor);

                // The featureclass has been deleted so remove this from the original featureclass list
                tables.RemoveRange(1, tables.Count - 1);
            }
            catch (Exception error)
            {
                Logger.Error(error);
            }
        }

        /// <summary>
        /// The draw rectangle.
        /// </summary>
        /// <param name="elm">
        /// The elm.
        /// </param>
        /// <returns>
        /// The <see cref="IPolygon"/>.
        /// </returns>
        public static IPolygon DrawRectangle(out IElement elm)
        {
            var activeView = ArcMap.Document.ActiveView;
            var screenDisplay = activeView.ScreenDisplay;

            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            var geometry = rubberBand.TrackNew(screenDisplay, null);

            var polygon = (IPolygon)geometry;

            IRgbColor rgbColor = VectorIndexColor();

            //Add the user's drawn graphics as persistent on the map.
            elm = AddGraphicToMap(activeView.FocusMap, polygon, rgbColor, rgbColor);

            //Best practice: Redraw only the portion of the active view that contains graphics. 
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, elm.Geometry.Envelope);
            return polygon;
        }

        /// <summary>
        /// Draw a specified graphic on the map using the supplied colors.
        /// </summary>
        /// <param name="map">
        /// An IMap interface.
        /// </param>
        /// <param name="geometry">
        /// An IGeometry interface. It can be of the geometry type: esriGeometryPoint, esriGeometryPolyline, or esriGeometryPolygon.
        /// </param>
        /// <param name="rgbColor">
        /// An IRgbColor interface. The color to draw the geometry.
        /// </param>
        /// <param name="outlineRgbColor">
        /// An IRgbColor interface. For those geometry's with an outline it will be this color.
        /// </param>
        /// <remarks>
        /// Calling this function will not automatically make the graphics appear in the map area. Refresh the map area after after calling this function with Methods like IActiveView.Refresh or IActiveView.PartialRefresh.
        /// </remarks>
        /// <returns>
        /// The <see cref="IElement"/>.
        /// </returns>
        private static IElement AddGraphicToMap(
            IMap map,
            IGeometry geometry,
            IRgbColor rgbColor,
            IRgbColor outlineRgbColor)
        {
            var graphicsContainer = (IGraphicsContainer)map; // Explicit Cast
            IElement element = null;
            if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                // Marker symbols
                ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();
                simpleMarkerSymbol.Color = rgbColor;
                simpleMarkerSymbol.Outline = true;
                simpleMarkerSymbol.OutlineColor = outlineRgbColor;
                simpleMarkerSymbol.Size = 15;
                simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

                IMarkerElement markerElement = new MarkerElementClass();
                markerElement.Symbol = simpleMarkerSymbol;
                element = (IElement)markerElement; // Explicit Cast
            }
            else if (geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                //  Line elements
                ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
                simpleLineSymbol.Color = rgbColor;
                simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                simpleLineSymbol.Width = 5;

                ILineElement lineElement = new LineElementClass();
                lineElement.Symbol = simpleLineSymbol;
                element = (IElement)lineElement; // Explicit Cast
            }
            else if (geometry.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                // Polygon elements
                ILineSymbol lineSymbol = new SimpleLineSymbolClass();
                lineSymbol.Color = rgbColor;
                lineSymbol.Width = 2.0;

                ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
                simpleFillSymbol.Color = rgbColor;
                simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
                simpleFillSymbol.Outline = lineSymbol;

                IFillShapeElement fillShapeElement = new PolygonElementClass();
                fillShapeElement.Symbol = simpleFillSymbol;
                element = (IElement)fillShapeElement; // Explicit Cast
            }

            if (element != null)
            {
                element.Geometry = geometry;
                graphicsContainer.AddElement(element, 0);
            }

            return element;
        }

        /// <summary>
        /// The cap envelope.
        /// </summary>
        /// <param name="envelope">
        /// The envelope.
        /// </param>
        public static void CapEnvelope(ref IEnvelope envelope)
        {
            if (envelope.XMin < -180)
            {
                envelope.XMin = -180;
            }

            if (envelope.XMax > 180)
            {
                envelope.XMax = 180;
            }

            if (envelope.YMin < -90)
            {
                envelope.YMin = -90;
            }

            if (envelope.YMax > 90)
            {
                envelope.YMax = 90;
            }
        }

        /// <summary>
        /// The display rectangle.
        /// </summary>
        /// <param name="activeView">
        /// The active view.
        /// </param>
        /// <param name="elm">
        /// The elm.
        /// </param>
        /// <returns>
        /// The <see cref="IPolygon"/>.
        /// </returns>
        public static IPolygon DisplayRectangle(IActiveView activeView, out IElement elm)
        {
            ISegmentCollection segmentCollection = new PolygonClass();

            var tempEnvelope = activeView.Extent;
            CapEnvelope(ref tempEnvelope);
            segmentCollection.SetRectangle(tempEnvelope);

            var polygon = (IPolygon)segmentCollection;
            var rgbColor = VectorIndexColor();
            elm = AddGraphicToMap(activeView.FocusMap, polygon, rgbColor, rgbColor);

            //Best practice: Redraw only the portion of the active view that contains graphics. 
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            return polygon;
        }

        /// <summary>
        /// From a JSON object performs string operations to get the page id needed for getting pages of data from insight cloud/elastic search
        /// </summary>
        /// <param name="json">
        /// full JSON string
        /// </param>
        /// <returns>
        /// just the page id
        /// </returns>
        public static string GetPageId(string json)
        {
            try
            {
                var pageID = JsonConvert.DeserializeObject<PageId>(json);

                return pageID.pagingId;
            }
            catch (Exception error)
            {
                Logger.Error(error);
                return string.Empty;
            }
        }

        /// <summary>
        /// Will copy the features from the original feature to the destination feature
        /// </summary>
        /// <param name="homeRow">
        /// Feature that will receive the new row
        /// </param>
        /// <param name="rowToBeCombined">
        /// Original row that will be copied
        /// </param>
        private static void AddFields(ref IFeatureBuffer homeRow, IFeature rowToBeCombined)
        {
            // Copy the attributes of the orig feature the new feature
            var fieldsNew = rowToBeCombined.Fields;

            var fields = homeRow.Fields;
            homeRow.Shape = rowToBeCombined.ShapeCopy;

            // Iterate through the fields and copy them as applicable.
            for (var i = 0; i <= fields.FieldCount - 1; i++)
            {
                var field = fields.Field[i];

                // if the field to be copied is any of the below fields skip it.
                if ((field.Type == esriFieldType.esriFieldTypeOID)
                    || string.Equals(field.Name, "Shape_Length", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var intFieldIndex = fieldsNew.FindField(field.Name);
                var origFieldIndex = fields.FindField(field.Name);
                if (intFieldIndex != -1 && origFieldIndex != -1)
                {
                    homeRow.Value[origFieldIndex] = rowToBeCombined.Value[intFieldIndex];
                }
            }
        }

        /// <summary>
        /// Will add the given feature layer to arcmap.
        /// </summary>
        /// <param name="layer">
        /// The layer to be added
        /// </param>
        public static void AddFeatureLayerToMap(ILayer layer)
        {
            try
            {
                var mxdoc = ArcMap.Application.Document as IMxDocument;
                if (mxdoc == null)
                {
                    return;
                }

                var map = mxdoc.FocusMap;
                map.AddLayer(layer);
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }
        
        /// <summary>
        /// The vector index color.
        /// </summary>
        /// <returns>
        /// The <see cref="IRgbColor"/>.
        /// </returns>
        private static IRgbColor VectorIndexColor()
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = 0;
            rgbColor.Green = 255;
            rgbColor.Blue = 0;
            return rgbColor;
        }

        /// <summary>
        /// The project to WGS 1984.
        /// </summary>
        /// <param name="geom">
        /// The geometry to be converted to WGS 1984.
        /// </param>
        /// <returns>
        /// The <see cref="IEnvelope"/>.
        /// </returns>
        public static IEnvelope ProjectToWgs1984(IEnvelope geom)
        {
            geom.Project(ProjectedCoordinateSystem);
            return geom;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
