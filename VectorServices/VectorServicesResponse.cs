using System.Collections.Generic;

namespace VectorServices
{
    public class Geometry
    {
        public string type { get; set; }
        public List<List<List<List<double>>>> coordinates { get; set; }
    }

    public class Access
    {
        public List<string> groups { get; set; }
        public List<string> users { get; set; }
    }

    public class Attributes
    {
        public string profileName { get; set; }
        public string tilePartition { get; set; }
        public string acquisitionDate { get; set; }
        public string bucketName { get; set; }
        public int numBands_int { get; set; }
        public double sunAzimuth_dbl { get; set; }
        public string nativeTileFileFormat { get; set; }
        public string colorInterpretation { get; set; }
        public int cloudCover_int { get; set; }
        public string idahoImageId { get; set; }
        public double satAzimuth_dbl { get; set; }
        public double satElevation_dbl { get; set; }
        public double pniirs_dbl { get; set; }
        public int numYTiles_int { get; set; }
        public int tileXOffset_int { get; set; }
        public string epsgCode { get; set; }
        public string vendor { get; set; }
        public double sunElevation_dbl { get; set; }
        public string platformName { get; set; }
        public int imageHeight_int { get; set; }
        public int numXTiles_int { get; set; }
        public string sensorPlatformName { get; set; }
        public string sensorName { get; set; }
        public string dataType { get; set; }
        public int imageWidth_int { get; set; }
        public int tileXSize_int { get; set; }
        public int tileYSize_int { get; set; }
        public double offNadirAngle_dbl { get; set; }
        public string tileBucketName { get; set; }
        public string vendorName { get; set; }
        public string version { get; set; }
        public int tileYOffset_int { get; set; }
        public string catalogID { get; set; }
        public string vendorDatasetIdentifier { get; set; }
        public double groundSampleDistanceMeters_dbl { get; set; }
    }

    public class IngestAttributes
    {
        public string _rest_user { get; set; }
        public string _rest_url { get; set; }
    }

    public class Properties
    {
        public string ingest_source { get; set; }
        public Access Access { get; set; }
        public string item_date { get; set; }
        public string original_crs { get; set; }
        public List<string> item_type { get; set; }
        public object format { get; set; }
        public string ingest_date { get; set; }
        public object source { get; set; }
        public object name { get; set; }
        public Attributes Attributes { get; set; }
        public IngestAttributes ingest_attributes { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class VectorServicesResponse
    {
        public string VectorType { get; set; }
        public Geometry Geometry { get; set; }
        public Properties Properties { get; set; }
    }
}