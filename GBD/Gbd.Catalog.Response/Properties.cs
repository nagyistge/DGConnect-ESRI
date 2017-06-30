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

// ReSharper disable InconsistentNaming

namespace GBD
{
    /// <summary>
    ///     The properties.
    /// </summary>
    public class Properties
    {
        public string acquisitionDate { get; set; }
        public string attFile { get; set; }
        public string bands { get; set; }
        public string bandsList { get; set; }
        public string browseJpgFile { get; set; }
        public string browseURL { get; set; }
        public string bucketName { get; set; }
        public string bucketPrefix { get; set; }
        public string catalogID { get; set; }
        public double cloudCover { get; set; }
        public string colorInterpretation { get; set; }
        public string dataType { get; set; }
        public string ephFile { get; set; }
        public string epsgCode { get; set; }
        public string footprintWkt { get; set; }
        public string geoFile { get; set; }
        public double groundSampleDistanceMeters { get; set; }
        public string idahoImageId { get; set; }
        public string imageBands { get; set; }
        public string imageFile { get; set; }
        public double imageHeight { get; set; }
        public double imageWidth { get; set; }
        public string imdFile { get; set; }
        public double multiResolution { get; set; }
        public object multiResolution_end { get; set; }
        public object multiResolution_max { get; set; }
        public object multiResolution_min { get; set; }
        public object multiResolution_start { get; set; }
        public string nativeTileFileFormat { get; set; }
        public double numBands { get; set; }
        public double numXTiles { get; set; }
        public double numYTiles { get; set; }
        public double offNadirAngle { get; set; }
        public double offNadirAngle_end { get; set; }
        public double offNadirAngle_max { get; set; }
        public double offNadirAngle_min { get; set; }
        public double offNadirAngle_start { get; set; }
        public double panResolution { get; set; }
        public double panResolution_end { get; set; }
        public double panResolution_max { get; set; }
        public double panResolution_min { get; set; }
        public double panResolution_start { get; set; }
        public double part { get; set; }
        public double path { get; set; }
        public string platformName { get; set; }
        public double pniirs { get; set; }
        public string productLevel { get; set; }
        public string profileName { get; set; }
        public string readmeTxtFile { get; set; }
        public double resolution { get; set; }
        public double row { get; set; }
        public string rpbFile { get; set; }
        public double satAzimuth { get; set; }
        public double satElevation { get; set; }
        public string scanDirection { get; set; }
        public string sensorName { get; set; }
        public string sensorPlatformName { get; set; }
        public string soli { get; set; }
        public object stereoPair { get; set; }
        public double sunAzimuth { get; set; }
        public double sunAzimuth_max { get; set; }
        public double sunAzimuth_min { get; set; }
        public double sunElevation { get; set; }
        public double sunElevation_max { get; set; }
        public double sunElevation_min { get; set; }
        public double targetAzimuth { get; set; }
        public double targetAzimuth_end { get; set; }
        public double targetAzimuth_max { get; set; }
        public double targetAzimuth_min { get; set; }
        public double targetAzimuth_start { get; set; }
        public string tileBucketName { get; set; }
        public string tilePartition { get; set; }
        public double tileXOffset { get; set; }
        public double tileXSize { get; set; }
        public double tileYOffset { get; set; }
        public double tileYSize { get; set; }
        public string tilFile { get; set; }
        public string timestamp { get; set; }
        public string vendor { get; set; }
        public string vendorDatasetIdentifier { get; set; }
        public string vendorName { get; set; }
        public string version { get; set; }
        public string xmlFile { get; set; }
    }
}