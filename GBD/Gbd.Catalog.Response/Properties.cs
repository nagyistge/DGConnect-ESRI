// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Properties.cs" company="DigitalGlobe">
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
//   Defines the Properties type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The properties.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
    public partial class Properties
    {
        /// <summary>
        /// Gets or sets the pan resolution.
        /// </summary>
        public string panResolution { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public string timestamp { get; set; }

        /// <summary>
        /// Gets or sets the EPSG code.
        /// </summary>
        public string epsgCode { get; set; }

        /// <summary>
        /// Gets or sets the timestamp WKT (Well Known Text).
        /// </summary>
        public string timestampWkt { get; set; }

        /// <summary>
        /// Gets or sets the cloud cover.
        /// </summary>
        public string cloudCover { get; set; }

        /// <summary>
        /// Gets or sets the browse url.
        /// </summary>
        public string browseURL { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Gets or sets the multi resolution.
        /// </summary>
        public string multiResolution { get; set; }

        /// <summary>
        /// Gets or sets the row.
        /// </summary>
        public string row { get; set; }

        /// <summary>
        /// Gets or sets the footprint WKT (Well Known Text).
        /// </summary>
        public string footprintWkt { get; set; }

        /// <summary>
        /// Gets or sets the vendor name.
        /// </summary>
        public string vendorName { get; set; }

        /// <summary>
        /// Gets or sets the catalog id.
        /// </summary>
        public string catalogID { get; set; }

        /// <summary>
        /// Gets or sets the image bands.
        /// </summary>
        public string imageBands { get; set; }

        /// <summary>
        /// Gets or sets the sensor platform name.
        /// </summary>
        public string sensorPlatformName { get; set; }

        /// <summary>
        /// Gets or sets the sun azimuth.
        /// </summary>
        public string sunAzimuth { get; set; }

        /// <summary>
        /// Gets or sets the off nadir angle.
        /// </summary>
        public string offNadirAngle { get; set; }

        /// <summary>
        /// Gets or sets the sun elevation.
        /// </summary>
        public string sunElevation { get; set; }

        /// <summary>
        /// Gets or sets the available.
        /// </summary>
        public string available { get; set; }

        /// <summary>
        /// Gets or sets the ordered.
        /// </summary>
        public string ordered { get; set; }

        /// <summary>
        /// Gets or sets the points.
        /// </summary>
        public List<GbdPoint> Points { get; set; }

        public string numXTiles { get; set; }
        public string tilePartition { get; set; }
        public string profileName { get; set; }
        public string acquisitionDate { get; set; }
        public string pniirs { get; set; }
        public string tileYOffset { get; set; }
        public string nativeTileFileFormat { get; set; }
        public string imageBoundsWGS84 { get; set; }
        public string colorInterpretation { get; set; }
        public string vendorDatasetIdentifier1 { get; set; }
        public string vendorDatasetIdentifier2 { get; set; }
        public string vendorDatasetIdentifier3 { get; set; }
        public string vendorDatasetIdentifier4 { get; set; }
        public string tileXSize { get; set; }
        public string imageWidth { get; set; }
        public string imageId { get; set; }
        public string sensorName { get; set; }
        public string groundSampleDistanceMeters { get; set; }
        public string dataType { get; set; }
        public string tileYSize { get; set; }
        public string tileXOffset { get; set; }
        public string tileBucketName { get; set; }
        public string version { get; set; }
        public string imageHeight { get; set; }
        public string numBands { get; set; }
        public string satAzimuth { get; set; }
        public string vendorDatasetIdentifier { get; set; }
        public string numYTiles { get; set; }
        public string satElevation { get; set; }
        public string imageBucketName { get; set; }
    }
}