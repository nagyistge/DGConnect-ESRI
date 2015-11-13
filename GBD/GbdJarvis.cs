// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GbdJarvis.cs" company="DigitalGlobe">
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
//   Defines the GbdJarvis type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System;
    using System.Collections.Generic;

    using DgxTools;

    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// The GBD helper class.  Named after the fun ai in ironman..
    /// </summary>
    public static class GbdJarvis
    {
        /// <summary>
        /// Takes the users large AOI and breaks it down to smaller 1x1 degree AOI's to query all information in the larger one.  
        /// </summary>
        /// <param name="env">
        /// The envelope of the large AOI
        /// </param>
        /// <returns>
        /// Returns a list of polygons that are used to query GBD.
        /// </returns>
        public static List<GbdPolygon> CreateAois(IEnvelope env)
        {
            try
            {
                env.Project(Jarvis.ProjectedCoordinateSystem);
                List<GbdPolygon> polygons = new List<GbdPolygon>();
                IPoint startingPoint = env.UpperLeft;

                while (startingPoint.Y != env.LowerLeft.Y)
                {
                    var temp = ProcessRow(new List<GbdPolygon>(), startingPoint, env);
                    polygons.AddRange(temp);

                    var newY = GetY(startingPoint, env);
                    startingPoint.Y = newY;
                }

                return polygons;
            }
            catch (Exception error)
            {
                return null;
            }
        }

        /// <summary>
        /// Takes the points from the WKT string and stores them in a list.
        /// </summary>
        /// <param name="wkt">
        /// Well Known Text (WKT) of the polygon.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<GbdPoint> GetPointsFromWkt(string wkt)
        {
            List<GbdPoint> points = new List<GbdPoint>();
            var wktString = wkt.Replace("POLYGON ((", string.Empty);
            wktString = wktString.Replace("))", string.Empty);
            var coordPairs = wktString.Split(',');
            foreach (var pair in coordPairs)
            {
                var splitpair = pair.Trim().Split(' ');
                if (string.IsNullOrEmpty(splitpair[0]) || string.IsNullOrEmpty(splitpair[1]))
                {
                    continue;
                }

                points.Add(new GbdPoint(splitpair[0], splitpair[1]));
            }

            return points;
        }

        /// <summary>
        /// Process the row across the large AOI
        /// </summary>
        /// <param name="list">
        /// Current list of GBDPolygons.
        /// </param>
        /// <param name="startingPoint">
        /// The point to start AOI traversal from.
        /// </param>
        /// <param name="env">
        /// Envelope of the large AOI
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<GbdPolygon> ProcessRow(List<GbdPolygon> list, IPoint startingPoint, IEnvelope env)
        {
            IPoint nextPoint;
            var output = GetSmallerAoi(startingPoint, env, out nextPoint);
            list.Add(output);

            if (nextPoint.X == env.UpperRight.X)
            {
                return list;
            }

            return ProcessRow(list, nextPoint, env);
        }

        /// <summary>
        /// Get smaller AOI
        /// </summary>
        /// <param name="startingPoint">
        /// The starting point.
        /// </param>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <param name="nextPoint">
        /// The next point.
        /// </param>
        /// <returns>
        /// The <see cref="GbdPolygon"/>.
        /// </returns>
        private static GbdPolygon GetSmallerAoi(IPoint startingPoint, IEnvelope env, out IPoint nextPoint)
        {
            var polygon = new GbdPolygon();
            IPoint pnt1 = new PointClass();
            IPoint pnt2 = new PointClass();
            IPoint pnt3 = new PointClass();

            var xValue = GetX(startingPoint, env);
            var yValue = GetY(startingPoint, env);

            pnt1.PutCoords(xValue, startingPoint.Y);
            pnt2.PutCoords(xValue, yValue);
            pnt3.PutCoords(startingPoint.X, yValue);

            polygon.AddPoint(pnt1);
            polygon.AddPoint(pnt2);
            polygon.AddPoint(pnt3);

            nextPoint = pnt1;
            return polygon;
        }

        /// <summary>
        /// Get the next X value from the starting point
        /// </summary>
        /// <param name="startingPoint">
        /// The starting point.
        /// </param>
        /// <param name="env">
        /// The envelope of the large AOI
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private static double GetX(IPoint startingPoint, IEnvelope env)
        {
            var potentialValue = startingPoint.X + 1;
            if (potentialValue >= env.UpperLeft.X && potentialValue <= env.UpperRight.X)
            {
                return potentialValue;
            }

            return env.UpperRight.X;
        }

        /// <summary>
        /// Get the next Y value from the starting point
        /// </summary>
        /// <param name="startingPoint">
        /// The starting point.
        /// </param>
        /// <param name="env">
        /// The envelope of the large AOI
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private static double GetY(IPoint startingPoint, IEnvelope env)
        {
            var potentialValue = startingPoint.Y - 1;
            if (potentialValue <= env.UpperLeft.Y && potentialValue >= env.LowerLeft.Y)
            {
                return potentialValue;
            }

            return env.LowerRight.Y;
        }
    }
}