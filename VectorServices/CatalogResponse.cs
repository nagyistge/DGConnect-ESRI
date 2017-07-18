using System.Collections.Generic;
// ReSharper disable InconsistentNaming

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
        public double? targetAzimuth_start_dbl { get; set; }
        public object stereoPair { get; set; }
        public double? offNadirAngle_end_dbl { get; set; }
        public string scanDirection { get; set; }
        public double? panResolution_end_dbl { get; set; }
        public string imageBands { get; set; }
        public double? targetAzimuth_end_dbl { get; set; }
        public double? multiResolution_max_dbl { get; set; }
        public double? offNadirAngle_min_dbl { get; set; }
        public double? offNadirAngle_start_dbl { get; set; }
        public string browseURL { get; set; }
        public double? multiResolution_min_dbl { get; set; }
        public double? offNadirAngle_max_dbl { get; set; }
        public double? multiResolution_start_dbl { get; set; }
        public double? targetAzimuth_min_dbl { get; set; }
        public double? sunElevation_max_dbl { get; set; }
        public double? panResolution_min_dbl { get; set; }
        public double? sunAzimuth_max_dbl { get; set; }
        public double? panResolution_dbl { get; set; }
        public double? panResolution_start_dbl { get; set; }
        public double? sunAzimuth_min_dbl { get; set; }
        public double? targetAzimuth_max_dbl { get; set; }
        public double? multiResolution_end_dbl { get; set; }
        public double? sunElevation_min_dbl { get; set; }
        public double? multiResolution_dbl { get; set; }
        public double? panResolution_max_dbl { get; set; }
        public double? targetAzimuth_dbl { get; set; }
        public string bands { get; set; }
        public string xmlFile { get; set; }
        public string browseJpgFile { get; set; }
        public string imdFile { get; set; }
        public string attFile { get; set; }
        public double? resolution_dbl { get; set; }
        public string bandsList { get; set; }
        public string geoFile { get; set; }
        public string imageFile { get; set; }
        public string tilFile { get; set; }
        public string readmeTxtFile { get; set; }
        public string rpbFile { get; set; }
        public string soli { get; set; }
        public string productLevel { get; set; }
        public int? part_int { get; set; }
        public string ephFile { get; set; }
        public string bucketPrefix { get; set; }
    }

    public class IngestAttributes
    {
        public string _rest_user { get; set; }
        public string _rest_url { get; set; }
    }

    public class Properties
    {
        public string ingest_source { get; set; }
        public Access access { get; set; }
        public string item_date { get; set; }
        public string original_crs { get; set; }
        public List<string> item_type { get; set; }
        public object format { get; set; }
        public string ingest_date { get; set; }
        public object source { get; set; }
        public object name { get; set; }
        public Attributes attributes { get; set; }
        public IngestAttributes ingest_attributes { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class CatalogResponse
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class PagingCatalogResponse
    {
        public List<CatalogResponse> data { get; set; }
        public string next_paging_id { get; set; }
        public string item_count { get; set; }
        public string total_count { get; set; }  
    }
}