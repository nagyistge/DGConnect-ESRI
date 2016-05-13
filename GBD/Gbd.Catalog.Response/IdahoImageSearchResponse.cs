using System.Collections.Generic;

namespace GBD
{

    public class Properties
    {
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
        public string epsgCode { get; set; }
        public string vendorDatasetIdentifier2 { get; set; }
        public string vendorDatasetIdentifier3 { get; set; }
        public string vendorDatasetIdentifier4 { get; set; }
        public string tileXSize { get; set; }
        public string timestamp { get; set; }
        public string imageWidth { get; set; }
        public string sunElevation { get; set; }
        public string imageId { get; set; }
        public string sensorPlatformName { get; set; }
        public string sensorName { get; set; }
        public string sunAzimuth { get; set; }
        public string groundSampleDistanceMeters { get; set; }
        public string dataType { get; set; }
        public string footprintWkt { get; set; }
        public string cloudCover { get; set; }
        public string tileYSize { get; set; }
        public string tileXOffset { get; set; }
        public string tileBucketName { get; set; }
        public string vendorName { get; set; }
        public string version { get; set; }
        public string imageHeight { get; set; }
        public string offNadirAngle { get; set; }
        public string numBands { get; set; }
        public string satAzimuth { get; set; }
        public string vendorDatasetIdentifier { get; set; }
        public string numYTiles { get; set; }
        public string satElevation { get; set; }
        public string imageBucketName { get; set; }
    }

    public class Result
    {
        public string identifier { get; set; }
        public string owner { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class TypeCounts
    {
        public int IDAHOImage { get; set; }
    }

    public class Stats
    {
        public int totalRecords { get; set; }
        public int recordsReturned { get; set; }
        public TypeCounts typeCounts { get; set; }
    }

    public class RootObject
    {
        public object searchTag { get; set; }
        public List<Result> results { get; set; }
        public Stats stats { get; set; }
    }

}