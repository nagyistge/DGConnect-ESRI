// --------------------------------------------------------------------------------------------------------------------
// <copyright file="rssPolygon.cs" company="DigitalGlobe">
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
//   Defines the rssPolygon type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dgx
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    using Dgx.Properties;
    using Dgx.Vector_Index;

    using DGXSettings;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Desktop.AddIns;
    using ESRI.ArcGIS.Display;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using NetworkConnections;

    /// <summary>
    /// The rss polygon.
    /// </summary>
    public class RssPolygon : Tool
    {
        /// <summary>
        /// Signals when work has been completed.
        /// </summary>
        private bool workDone;

        /// <summary>
        /// The network response contain rss results.
        /// </summary>
        private NetworkResponse smaResponse;

        /// <summary>
        /// The query searched for
        /// </summary>
        private string query = string.Empty;

        /// <summary>
        /// Background worker to handle all work off of the main UI thread
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// True when work is happening.
        /// </summary>
        private bool doingWork;

        /// <summary>
        /// The element of the bounding box drawn on screen.
        /// </summary>
        private IElement element;

        /// <summary>
        /// Name of the layer where the results will be displayed
        /// </summary>
        private string layerName = string.Empty;

        /// <summary>
        /// The actual table name that supplies the data for the layer.  
        /// </summary>
        private string tablename = string.Empty;

        /// <summary>
        /// Esri format of the table required to be used within arcmap
        /// </summary>
        private ITable table;

        /// <summary>
        /// Boolean variable for no results being found
        /// </summary>
        private bool noResultsFound;

        /// <summary>
        /// Center point on the map where the mouse was clicked while holding shift.
        /// </summary>
        private IPoint center;

        /// <summary>
        /// Radius of the drawn circle.
        /// </summary>
        private double radius;

        /// <summary>
        /// Original spatial reference only used by the circle queries for the time being.
        /// </summary>
        private ISpatialReference originalSpatialReference;

        #region Public Methods
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
        public static IElement AddGraphicToMap(IMap map, IGeometry geometry, IRgbColor rgbColor, IRgbColor outlineRgbColor)
        {
            var graphicsContainer = (IGraphicsContainer)map; // Explicit Cast
            IElement shapeElement = null;
            switch (geometry.GeometryType)
            {
                case esriGeometryType.esriGeometryPoint:

                    // Marker symbols
                    ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();
                    simpleMarkerSymbol.Color = rgbColor;
                    simpleMarkerSymbol.Outline = true;
                    simpleMarkerSymbol.OutlineColor = outlineRgbColor;
                    simpleMarkerSymbol.Size = 15;
                    simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

                    IMarkerElement markerElement = new MarkerElementClass();
                    markerElement.Symbol = simpleMarkerSymbol;
                    shapeElement = (IElement)markerElement; // Explicit Cast
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    //  Line elements
                    ISimpleLineSymbol simpleLineSymbol = new SimpleLineSymbolClass();
                    simpleLineSymbol.Color = rgbColor;
                    simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                    simpleLineSymbol.Width = 5;

                    ILineElement lineElement = new LineElementClass();
                    lineElement.Symbol = simpleLineSymbol;
                    shapeElement = (IElement)lineElement; // Explicit Cast
                    break;
                case esriGeometryType.esriGeometryPolygon:

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
                    shapeElement = (IElement)fillShapeElement; // Explicit Cast
                    break;
            }

            if (shapeElement != null)
            {
                shapeElement.Geometry = geometry;
                graphicsContainer.AddElement(shapeElement, 0);
            }

            return shapeElement;
        }

        ///<summary>
        ///Create a polyline geometry object using the RubberBand.TrackNew method when a user click the mouse on the map control. 
        ///</summary>
        ///<param name="activeView">An ESRI.ArcGIS.Carto.IActiveView interface that will user will interface with to draw a polyline.</param>
        ///<returns>An ESRI.ArcGIS.Geometry.IPolyline interface that is the polyline the user drew</returns>
        ///<remarks>Double click the left mouse button to end tracking the polyline.</remarks>
        public IPolyline GetPolylineFromMouseClicks(IActiveView activeView)
        {
            var screenDisplay = activeView.ScreenDisplay;

            IRubberBand rubberBand = new RubberLineClass();
            var geometry = rubberBand.TrackNew(screenDisplay, null);

            var polyline = (IPolyline)geometry;

            return polyline;
        }

        ///<summary>
        ///Create a rectangle geometry object using the RubberBand.TrackNew method when a user click the mouse on the map control. 
        ///</summary>
        ///<param name="activeView">An ESRI.ArcGIS.Carto.IActiveView interface that will user will interface with to draw a polyline.</param>
        ///<returns>An ESRI.ArcGIS.Geometry.IPolyline interface that is the polyline the user drew</returns>
        ///<remarks>Double click the left mouse button to end tracking the polyline.</remarks>
        public IPolygon GetRectangleFromMouseClicks(IActiveView activeView)
        {
            var screenDisplay = activeView.ScreenDisplay;

            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            var geometry = rubberBand.TrackNew(screenDisplay, null);

            var polygon = (IPolygon)geometry;

            return polygon;
        }

        /// <summary>
        /// Draws a polygon on the screen in the ActiveView where the mouse is clicked.
        /// </summary>
        /// <param name="activeView">
        /// An IActiveView interface
        /// </param>
        /// <remarks>
        /// Ideally, this function would be called from within the OnMouseDown event that was created with the ArcGIS base tool template.
        /// </remarks>
        public void DrawPolygon(IActiveView activeView)
        {
            if (activeView == null)
            {
                return;
            }

            var screenDisplay = activeView.ScreenDisplay;

            // Constant
            screenDisplay.StartDrawing(screenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache); // Explicit Cast
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Blue = 255;

            IColor color = rgbColor; // Implicit Cast
            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            simpleFillSymbol.Color = color;
            simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;

            var symbol = simpleFillSymbol as ISymbol; // Dynamic Cast
            IRubberBand rubberBand = new RubberRectangularPolygonClass();
            var geometry = rubberBand.TrackNew(screenDisplay, symbol);
            screenDisplay.SetSymbol(symbol);
            screenDisplay.DrawPolygon(geometry);
            screenDisplay.FinishDrawing();
        }

        ///<summary>
        ///Delete all the graphics in the GraphicsContainer and refresh the ActiveView.
        ///</summary>
        ///<param name="activeView">An ESRI.ArcGIS.Carto.IActiveView interface that will have the graphics deleted and refreshed.</param>
        public void DeleteGraphicsRefreshActiveView(IActiveView activeView)
        {
            var graphicsContainer = activeView.GraphicsContainer;
            graphicsContainer.DeleteAllElements();
            activeView.Refresh();
        }

        #endregion
        #region Protected Methods
        /// <summary>
        /// The on double click.
        /// </summary>
        protected override void OnDoubleClick()
        {
            if (this.element != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.element);
                this.element = null;
            }
        }

        /// <summary>
        /// The on mouse down event for the tool's button
        /// </summary>
        /// <param name="arg">
        /// The mouse event arguments
        /// </param>
        protected override void OnMouseDown(MouseEventArgs arg)
        {
            // If not a left mouse button click we don't care.  Althought Left handed users my get miffed at this.
            if (arg.Button != MouseButtons.Left)
            {
                return;
            }

            this.noResultsFound = false;
            this.table = null;

            // If the shift key is being held down then do Circle Analytics otherwise Rectangle Analytics.
            if (Control.ModifierKeys == Keys.Shift)
            {
                IPoint screenPoint = new PointClass();
                screenPoint.PutCoords(arg.X, arg.Y);

                // Create esri point from where the mouse fist clicked.
                this.center = ArcUtility.GetMapCoordinatesFromScreenCoordinates(screenPoint, ArcMap.Document.ActiveView);
                this.CircleAnalytics();
            }
            else
            {
                this.RectangleAnalytics();
            }
        }

        /// <summary>
        /// The on update method occurs constantly when the ESRI tool is selected.
        /// </summary>
        protected override void OnUpdate()
        {
            this.Enabled = ArcMap.Application != null;
            if (this.doingWork && ArcMap.Application != null)
            {
                ArcMap.Application.StatusBar.Message[0] = "Running RSS Query: " + this.query;
            }

            if (!this.workDone)
            {
                return;
            }

            this.ProcessResponse(this.smaResponse);
        }

        #endregion
        #region Private Methods
        /// <summary>
        /// Create fill symbol.
        /// </summary>
        /// <returns>
        /// The <see cref="ISimpleFillSymbol"/>.
        /// </returns>
        private static ISimpleFillSymbol CreateFillSymbol()
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Blue = 255;

            ILineSymbol lineSymbol = new SimpleLineSymbolClass();
            lineSymbol.Color = rgbColor;
            lineSymbol.Width = 2.0;

            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            simpleFillSymbol.Color = rgbColor;
            simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
            simpleFillSymbol.Outline = lineSymbol;

            return simpleFillSymbol;
        }

        /// <summary>
        /// Create circle.
        /// </summary>
        /// <param name="geometry">
        /// The esri IGeometry of the circle
        /// </param>
        /// <returns>
        /// The <see cref="IGeometry"/>.
        /// </returns>
        private static IGeometry CreateCircle(IGeometry geometry)
        {
            if (null == geometry)
            {
                return null;
            }

            var segment = geometry as ISegment;
            ISegmentCollection polygon = new PolygonClass();

            var missing = Type.Missing;
            polygon.AddSegment(segment, ref missing, ref missing);

            return (IGeometry)polygon;
        }

        /// <summary>
        /// Run circle analytics.
        /// </summary>
        private void CircleAnalytics()
        {
            if (this.element != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.element);
                this.element = null;
            }

            //Get the active view from the ArcMap static class.
            var activeView = ArcMap.Document.ActiveView;
            IRubberBand rubberCircle = new RubberCircleClass();
            var circle = rubberCircle.TrackNew(activeView.ScreenDisplay, null);
            var circleArc = circle as ICircularArc;

            // Get the raidus distance
            this.radius = ArcUtility.DistanceTo(this.center, circleArc.FromPoint);

            IFillShapeElement fillShape = new CircleElementClass();
            fillShape.Symbol = CreateFillSymbol(); //a method that create a a simple fill symbol
            this.element = (IElement)fillShape;
            var geometry = CreateCircle(circle);
            geometry.SpatialReference = activeView.FocusMap.SpatialReference;

            // Save the spatial reference for use later.
            this.originalSpatialReference = activeView.FocusMap.SpatialReference;
            this.element.Geometry = geometry;
            var graphicsContainer = (IGraphicsContainer)activeView;
            graphicsContainer.AddElement(this.element, 0);

            var envelope = VectorIndexHelper.ProjectToWgs1984(geometry.Envelope);
            if (!ArcUtility.ValidateWgs84Polygon(envelope))
            {
                MessageBox.Show("Bounding circle invalid. Please re-draw.");
                if (this.element != null)
                {
                    ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.element);
                    this.element = null;
                }

                return;
            }

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            this.RunAnalytics(circleArc);
        }

        /// <summary>
        /// Run analytics for a circle
        /// </summary>
        /// <param name="arc2Circle">
        /// The arc of the circle.
        /// </param>
        private void RunAnalytics(ICircularArc arc2Circle)
        {
            if (arc2Circle.IsEmpty)
            {
                return;
            }

            // Specify the projection
            var spatialReferenceFactory = new SpatialReferenceEnvironment() as ISpatialReferenceFactory;
            var geographicCoordinateSystem =
                spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

            // Prevents an error from occuring here when the spatial references are the same.
            if (!string.Equals(arc2Circle.SpatialReference.Name, geographicCoordinateSystem.Name, StringComparison.OrdinalIgnoreCase))
            {
                arc2Circle.Project(geographicCoordinateSystem);
            }

            if (!this.doingWork)
            {
                var qb = new FormQueryBuilder(arc2Circle.Envelope);

                if (!qb.ValidAuthentication)
                {
                    return;
                }

                qb.Text = "Text Search Builder";

                // If ok not clicked return
                if (qb.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var webClient = new SmaClient(
                    qb.GetKeywords(),
                    qb.GetRows(),
                    arc2Circle,
                    qb.GetStartTime(),
                    qb.GetStopTime());
                webClient.SetUrl(false);

                // Setup the worker, listen to events, set/reset local variables.
                this.SetupWorker(qb.GetKeywords(), qb.GetStartTime().ToString("MM/dd/yy H:mm"), qb.GetStopTime().ToString("MM/dd/yy H:mm"), true);

                // Start the background worker
                this.worker.RunWorkerAsync(webClient);
            }
            else
            {
                // A query is already being processed tell the user to wait.
                MessageBox.Show("Please wait until the previous sma query has finished.");
            }
        }

        #region Rectangle

        /// <summary>
        /// Run rectangle analytics
        /// </summary>
        private void RectangleAnalytics()
        {
            //Get the active view from the ArcMap static class.
            var activeView = ArcMap.Document.ActiveView;

            //If it's a polyline object, get from the user's mouse clicks.
            var poly = this.GetRectangleFromMouseClicks(activeView);

            //Make a color to draw the polyline. 
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Blue = 255;

            //Add the user's drawn graphics as persistent on the map.
            if (this.element != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.element);
                this.element = null;
            }

            this.element = AddGraphicToMap(activeView.FocusMap, poly, rgbColor, rgbColor);
            
            if (poly.IsEmpty == false)
            {
                var env = poly.Envelope;

                // Specify the projection
                var spatialReferenceFactory = new SpatialReferenceEnvironment() as ISpatialReferenceFactory;
                var geographicCoordinateSystem =
                    spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                env.Project(geographicCoordinateSystem);

                if (!ArcUtility.ValidateWgs84Polygon(env))
                {
                    MessageBox.Show("Bounding box invalid. Please re-draw.");
                    if (this.element != null)
                    {
                        ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.element);
                        this.element = null;
                    }

                    return;
                }

                //Best practice: Redraw only the portion of the active view that contains graphics. 
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                // If not doing work LET's do some.
                if (!this.doingWork)
                {
                    var qb = new FormQueryBuilder(env);

                    if (!qb.ValidAuthentication)
                    {
                        return;
                    }

                    qb.Text = "Text Search Builder";
                    if (qb.ShowDialog() == DialogResult.OK)
                    {
                        var webClient = new SmaClient(
                            qb.GetKeywords(),
                            qb.GetRows(),
                            env,
                            qb.GetStartTime(),
                            qb.GetStopTime());
                        webClient.SetUrl(false);

                        this.SetupWorker(qb.GetKeywords(), qb.GetStartTime().ToString("MM/dd/yy H:mm"), qb.GetStopTime().ToString("MM/dd/yy H:mm"), false);

                        this.worker.RunWorkerAsync(webClient);
                    }
                }
                else
                {
                    // Sorry currently busy please try again later.
                    MessageBox.Show("Please wait until the sma query has finished.");
                }
            }
        }

        /// <summary>
        /// The process response.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        private void ProcessResponse(NetworkResponse response)
        {
            // Reset Work done variable to keep from continually adding layers.
            this.workDone = false;

            // Set doing work to false to allow more SMA queries to be conducted.
            this.doingWork = false;
            if (response.Error)
            {
                if (response.Timeouts == 4)
                {
                    MessageBox.Show("Response Timed out.  If this happens frequently please contact your administrator.");
                }
                else
                {
                    MessageBox.Show(DgxResources.Source_ErrorMessage);
                }

                // clear out the query 
                this.query = string.Empty;

                return;
            }

            if (this.noResultsFound)
            {
                MessageBox.Show(DGXSettings.DgxResources.VectorIndexDockable_UpdateTreeViewWithSources_No_data_found_);
                return;
            }

            ArcUtility.AddXyEventLayer(this.table, this.layerName);

            // clear out the query 
            this.query = string.Empty;

            // clear center point
            this.center = null;

            // clear radius
            this.radius = double.NaN;
            this.layerName = string.Empty;
            this.tablename = string.Empty;
        }

        /// <summary>
        /// The setup worker.
        /// </summary>
        /// <param name="keywords">
        /// The keywords.
        /// </param>
        /// <param name="from">
        /// The from.
        /// </param>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <param name="isCircle">
        /// The is circle.
        /// </param>
        private void SetupWorker(string keywords, string from, string to, bool isCircle)
        {
            this.doingWork = true;
            this.smaResponse = new NetworkResponse();
            this.query = string.Format("\"{0}\" {1} - {2}", keywords, from, to);
            this.worker = new BackgroundWorker();
            this.worker.DoWork += (sender, e) =>
            {
                var client = (SmaClient)e.Argument;
                var responseFromServer = client.Run();
                e.Result = responseFromServer;
            };
            this.worker.RunWorkerCompleted += (sender, e) =>
            {
                if (this.workDone)
                {
                    return;
                }

                this.ProcessRequest(e, isCircle);
            };
        }

        #endregion

        /// <summary>
        /// Process Request.  this method is used by the background worker.
        /// </summary>
        /// <param name="e">
        /// The RunWorkerCompletedEventArgs
        /// </param>
        /// <param name="isCircle">
        /// The is circle boolean variable
        /// </param>
        private void ProcessRequest(RunWorkerCompletedEventArgs e, bool isCircle)
        {
            var responseFromServer = (NetObject)e.Result;
            this.smaResponse = new NetworkResponse();
            this.smaResponse.Result = responseFromServer.Result;
            this.smaResponse.IsCircle = isCircle;

            // We had an error don't continue any further.
            if (responseFromServer.ErrorOccurred)
            {
                this.smaResponse.Error = true;
                this.workDone = true;
                return;
            }

            // Check to see if there are any hits.  If there aren't then lets just skip to the end where we alert the user.
            var hits = ArcUtility.HitRegex.Match(responseFromServer.Result).Groups["hits"].ToString();
            if (string.Equals(hits, "0", StringComparison.OrdinalIgnoreCase))
            {
                this.noResultsFound = true;
                this.workDone = true;
                return;
            }

            // According to the regular expression results were found so lets process them.
            this.layerName = "RSS " + this.query;
            this.tablename = ArcUtility.CreateTableName(this.layerName);
            Exception output = null;

            // Execute the proper function for the user drawn shape.
            if (this.smaResponse.IsCircle)
            {
                output = ArcUtility.ReadRssResponseToJsonCircle(this.smaResponse.Result, this.tablename, this.radius, this.center, this.originalSpatialReference, ref this.table);
            }
            else
            {
                // Display results of the query
                output = ArcUtility.ReadRssResponseToJson(this.smaResponse.Result, this.tablename, ref this.table);
            }

            // If an error occurred lets make sure that user gets informed about it.
            if (output != null)
            {
                this.smaResponse.Error = true;
                this.smaResponse.ExceptionIncurred = output;
            }

            this.workDone = true;
        }

        #endregion
    }
}
