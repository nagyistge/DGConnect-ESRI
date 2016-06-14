using System.Windows.Forms;
using DataInterop;

namespace GbdxTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
  
    using Logging;

    using Newtonsoft.Json;

    public class Jarvis
    {

        /// <summary>
        /// Log file where all information should be written too.
        /// </summary>
        public static readonly string LogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                 + GbdxSettings.Properties.Settings.Default.logfile;

        /// <summary>
        /// The file that contains the GBD order information.  i.e. order numbers etc.
        /// </summary>
        public static readonly string GbdOrderFile = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + GbdxSettings.Properties.Settings.Default.GbdOrders;

        /// <summary>
        /// The base 32 codes.
        /// </summary>
        private const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";


        /// <summary>
        /// The invalid field starting characters.
        /// </summary>
        public static readonly Regex InvalidFieldStartingCharacters =
            new Regex("[\\^`~@#$%^&*()-+=|,\\\\<>?/{}\\.!'[\\]:;_0123456789]");

        /// <summary>
        /// The invalid field characters.
        /// </summary>
        public static readonly Regex InvalidFieldCharacters = new Regex("[-@^\\%#\\s$!*{}\\.<>?/`'[:\\];=+()|,~&]");

        /// <summary>
        /// The spatial reference factory.
        /// </summary>
        public static readonly ISpatialReferenceFactory SpatialReferenceFactory = new SpatialReferenceEnvironment();

        /// <summary>
        /// The projected coordinate system.
        /// </summary>
        public static readonly IGeographicCoordinateSystem ProjectedCoordinateSystem =
            SpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        /// <summary>
        /// The logger.
        /// </summary>
        public static readonly Logger Logger = new Logger(LogFile, false);
        

        /// <summary>
        /// The base 32 codes dictionary.
        /// </summary>
        private static Dictionary<char, int> base32CodesDict = Base32Codes.ToDictionary(
            chr => chr,
            chr => Base32Codes.IndexOf(chr));

        public static HashSet<char> invalidStartingChars = new HashSet<char>
                                                                {
                                                                    '\\','^','`','~','@','#','$','^', '&', '*','(',')','-','+','=','|',',','<','>','?','/','{','}','.','!',':',';','_','0','1','2','3','4','5','6','7','8','9'
                                                                };

        public static IGeometry ProjectToWGS84(IGeometry geom)
        {
            geom.Project(ProjectedCoordinateSystem);
            return geom;
        }

        public static IFields ValidateFields(IWorkspace workspace, IFields fields)
{
      // Create and initialize a field checker.
      IFieldChecker fieldChecker = new FieldCheckerClass();
      fieldChecker.ValidateWorkspace = workspace;
                 
      // Generate a validated fields collection.
      IFields validatedFields = null;
      IEnumFieldError enumFieldError = null;
      fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

      // Return the validated fields.
      return validatedFields;
}

        public static IFeatureClass CreateStandaloneFeatureClass(
            IWorkspace workspace,
            string featureClassName,
            Dictionary<string,string> uniqueNames, bool allowNull, double defaultValue)
        {
            var featureWorkspace = (IFeatureWorkspace)workspace;
            IFeatureClassDescription fcDesc = new FeatureClassDescriptionClass();
            var ocDesc = (IObjectClassDescription)fcDesc;

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;

            IFields fields = new FieldsClass();

            // if a fields collection is not passed in then supply our own
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields; // Explicit Cast

            var tmpField = new FieldClass();
                        IFieldEdit tmpFieldEdit = (IFieldEdit)tmpField;
                        tmpFieldEdit.Name_2 = "GeoHash";
                        tmpFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                        tmpFieldEdit.Length_2 = 20;
                        fieldsEdit.AddField(tmpField);

            foreach (var name in uniqueNames.Keys)
            {

                var tempField = new FieldClass();
                IFieldEdit tempFieldEdit = (IFieldEdit)tempField;
                tempFieldEdit.Name_2 = "DG_" + uniqueNames[name];
                if (name.EndsWith("_str")) {
                  tempFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                  tempFieldEdit.Length_2 = 250;
                  tempFieldEdit.AliasName_2 = name;
                  tempFieldEdit.IsNullable_2 = allowNull;
                  tempFieldEdit.DefaultValue_2 = "";
                }
                else {
                tempFieldEdit.Name_2 = "DG_"+uniqueNames[name];
                tempFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                tempFieldEdit.Length_2 = 20;
                tempFieldEdit.AliasName_2 = name;
                tempFieldEdit.IsNullable_2 = allowNull;
                tempFieldEdit.DefaultValue_2 = defaultValue;
                }
                
              
                fieldsEdit.AddField(tempField);
            }

            ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironment();
            var geographicCoordinateSystem =
                spatialReferenceFactory.CreateGeographicCoordinateSystem(
                    (int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = geographicCoordinateSystem;
            var shapeField = new FieldClass();
            IFieldEdit shapeFieldEdit = (IFieldEdit)shapeField;
            shapeFieldEdit.Name_2 = "SHAPE";
            shapeFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shapeFieldEdit.GeometryDef_2 = geometryDef;
            shapeFieldEdit.IsNullable_2 = true;
            shapeFieldEdit.Required_2 = true;
            fieldsEdit.AddField(shapeField);
            fields = (IFields)fieldsEdit;

            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.
            try {
              IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(
                  featureClassName,
                  validatedFields,
                  ocDesc.InstanceCLSID,
                  ocDesc.ClassExtensionCLSID,
                  esriFeatureType.esriFTSimple,
                  "SHAPE",
                  string.Empty);
              return featureClass;
            }
            catch (Exception ex) {
              Logger.Error(ex);
                throw new Exception("Issue with creating feature class");
            }
          
        }

        /// <summary>
        /// Generic method to load objects of type T from file that are serialized via JSON.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <typeparam name="T">
        /// Object that is serialized via JSON in the file
        /// </typeparam>
        /// <returns>
        /// The <see cref="T[]"/>.
        /// </returns>
        public static T[] LoadObjectsFromFile<T>(string path)
        {
            var lines = File.ReadAllLines(path);
            List<T> objects = new List<T>();
            for (int i = 0; i <= lines.Length - 1; i++)
            {
                // Don't bother with empty or null strings
                if (string.IsNullOrEmpty(lines[i]))
                {
                    continue;
                }

                // convert and if not null lets add it to the objects
                var obj = JsonConvert.DeserializeObject<T>(lines[i]);
                if (obj != null)
                {
                    objects.Add(obj);
                }
            }

            return objects.ToArray();
        }

        public static double[] DecodeBbox(string hashString)
        {

            var isLon = true;
            var maxLat = 90D;
            var minLat = -90D;
            var maxLon = 180D;
            var minLon = -180D;

            foreach (var code in hashString.ToLower())
            {
                var hashValue = base32CodesDict[code];

                for (var bits = 4; bits >= 0; bits--)
                {
                    var bit = (hashValue >> bits) & 1;
                    double mid;
                    if (isLon)
                    {
                        mid = (maxLon + minLon) / 2;
                        if (bit == 1)
                        {
                            minLon = mid;
                        }
                        else
                        {
                            maxLon = mid;
                        }
                    }
                    else
                    {
                        mid = (maxLat + minLat) / 2;
                        if (bit == 1)
                        {
                            minLat = mid;
                        }
                        else
                        {
                            maxLat = mid;
                        }
                    }
                    isLon = !isLon;
                }
            }
            return new[] { minLat, minLon, maxLat, maxLon };
        }


        /// <summary>
        /// Open the GBDX cloud workspace.  This defaults to the GBDX cloud file GDB.
        /// </summary>
        /// <param name="path">
        /// File system path to the file GDB.
        /// </param>
        /// <returns>
        /// IWorkspace that was opened on the file GDB.
        /// </returns>
        public static IWorkspace OpenWorkspace(string path)
        {
            IWorkspace workspace;
            var factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            var workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            // If it already exists open otherwise create it.
            if (workspaceFactory.IsWorkspace(path + "\\GBDX.gdb"))
            {
                workspace = workspaceFactory.OpenFromFile(path + "\\GBDX.gdb", 0);
            }
            else
            {
                var workspaceName = workspaceFactory.Create(path + "\\", "GBDX.gdb", null, 0);

                // Cast the workspace name object to the IName interface and open the workspace.
                var name = (IName)workspaceName;
                workspace = (IWorkspace)name.Open();
            }

            return workspace;
        }

        public static List<String> GetFieldList(IFeatureClass fc) {
          List<String> outlist = new List<String>();
          for (int i = 0; i < fc.Fields.FieldCount;i++ ) {
            outlist.Add(fc.Fields.get_Field(i).Name);
          }
          return outlist;
        }

        /// <summary>
        /// Get all the selected Polygons from the map document
        /// </summary>
        /// <param name="focusMap">Map document with selected polygons</param>
        /// <returns>List of IPolygons</returns>
        public static List<IPolygon> GetPolygons(IMap focusMap)
        {
            List<IPolygon> polygons = new List<IPolygon>();

            IEnumFeature pEnumFeat = (IEnumFeature)focusMap.FeatureSelection;
            pEnumFeat.Reset();

            try
            {
                IFeature pfeat;
                while ((pfeat = pEnumFeat.Next()) != null)
                {
                    IPolygon geo = (IPolygon)pfeat.ShapeCopy;
                    var geo2 = (IPolygon) ProjectToWGS84(geo);
                    polygons.Add(geo2);

                }
            }
            catch (Exception error)
            {
                Logger.Error(error);
            }

            return polygons;
        }

        public static DataInterop.MultiPolygon CreateMultiPolygon(List<IPolygon> polygons)
        {
            List<DataInterop.Polygon> polyList = new List<DataInterop.Polygon>();

            foreach (var poly in polygons)
            {
                var poly4 = (IPolygon4) poly;
                var exteriorRingGeometryBag = poly4.ExteriorRingBag;
                var exteriorRingGeometryCollection = exteriorRingGeometryBag as IGeometryCollection;

                if (exteriorRingGeometryCollection != null)
                {
                    for (int i = 0; i < exteriorRingGeometryCollection.GeometryCount; i++)
                    {
                        var exteriorRingGeometry = exteriorRingGeometryCollection.Geometry[i];
                        var pointCollect = exteriorRingGeometry as IPointCollection;

                        var coordinates = new List<List<IPosition>>();
                        if (pointCollect != null)
                        {
                            var exteriorPointString = new List<IPosition>();
                            for (int j = 0; j < pointCollect.PointCount; j++)
                            {
                                var point = pointCollect.Point[j];
                                exteriorPointString.Add(new GeographicPosition(point.Y,point.X));
                            }
                            coordinates.Add(exteriorPointString);
                        }

                        var interiorRingBag = poly4.InteriorRingBag[exteriorRingGeometry as IRing];
                        var interiorRingGeometryCollection = interiorRingBag as IGeometryCollection;
                        if (interiorRingGeometryCollection != null)
                        {
                            for (int t = 0; t < interiorRingGeometryCollection.GeometryCount; t++) 
                            {
                                var interiorPointCollect =
                                    interiorRingGeometryCollection.Geometry[t] as IPointCollection;

                                if (interiorPointCollect != null)
                                {
                                    var interiorPointString = new List<IPosition>();
                                    for (int z = 0; z < interiorPointCollect.PointCount; z++)
                                    {
                                        var point = interiorPointCollect.Point[z];
                                        interiorPointString.Add(new GeographicPosition(point.Y,point.X));
                                    }
                                    coordinates.Add(interiorPointString);
                                }
                            }
                        }
                        var linestring = new LineString(coordinates[0]);
                //polyList.Add(new DataInterop.Polygon());
                    }
                }
                
            }

            DataInterop.MultiPolygon output = new DataInterop.MultiPolygon();
            return output;
        }

        public static string ConvertPolygonsToGeoJson(List<IPolygon> polygons)
        {
            StringBuilder output = new StringBuilder("{\"type\":\"MultiPolygon\", \"coordinates\": [");

            for (int i = 0; i < polygons.Count; i++)
            {
                if (i != 0)
                {
                    output.Append(",");
                }

                output.Append(ConvertToPolygonGeoJson(polygons[i]));
            }

            output.Append("]}");
            return output.ToString();
        }

        private static string ConvertToPolygonGeoJson(IPolygon poly)
        {
            var output = new StringBuilder();
            var poly4 = (IPolygon4)poly;
            var exteriorRingGeometryBag = poly4.ExteriorRingBag;
            var exteriorRingGeometryCollection = exteriorRingGeometryBag as IGeometryCollection;

            if (exteriorRingGeometryCollection != null)
            {
                for (int i = 0; i < exteriorRingGeometryCollection.GeometryCount; i++)
                {
                    if (i != 0)
                    {
                        output.Append(",");
                    }
                    output.Append("[[");
                    var exteriorRingGeometry = exteriorRingGeometryCollection.Geometry[i];

                    output.Append(ConvertPointCollectionToGeoJson(exteriorRingGeometry));
                    output.Append("]]");

                    output.Append(ConvertInteriorRingsToGeoJson(poly4.InteriorRingBag[exteriorRingGeometry as IRing]));
                }
            }

            return output.ToString();
        }

        private static string ConvertPointCollectionToGeoJson(IGeometry ringGeometry)
        {
            var pointCollect = ringGeometry as IPointCollection;
            var builder = new StringBuilder();
            if(pointCollect != null)
            {
                for (int i = 0; i < pointCollect.PointCount; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(", ");
                    }
                    var point = pointCollect.Point[i];
                    builder.Append(PointToString(point));
                }
            }
            return builder.ToString();
        }

        private static string ConvertInteriorRingsToGeoJson(IGeometryBag interiorRingGeometryBag)
        {
            var output = new StringBuilder();
            var interiorRingGeometryCollection = interiorRingGeometryBag as IGeometryCollection;

            if (interiorRingGeometryCollection != null && interiorRingGeometryCollection.GeometryCount > 0)
            {
                output.Append(",");
            }
            else
            {
                return string.Empty;
            }

            for (int i = 0; i < interiorRingGeometryCollection.GeometryCount; i++)
            {
                if (i != 0)
                {
                    output.Append(", ");
                }

                output.Append("[[");
                var interiorRingGeometry = interiorRingGeometryCollection.Geometry[i];
                output.Append(ConvertPointCollectionToGeoJson(interiorRingGeometry));
                output.Append("]]");
            }

            return output.ToString();
        }

        private static string PointToString(IPoint point)
        {
            return "[" + point.X + ", " + point.Y + "]";
        }  
    }
}