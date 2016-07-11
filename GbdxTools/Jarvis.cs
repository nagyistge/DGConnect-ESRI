namespace GbdxTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using DataInterop;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using GbdxSettings.Properties;

    using Logging;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Polygon = DataInterop.Polygon;

    public class Jarvis
    {
        #region Fields & Properties

        /// <summary>
        ///     The base 32 codes.
        /// </summary>
        private const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";

        /// <summary>
        ///     Log file where all information should be written too.
        /// </summary>
        public static readonly string LogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                                + Settings.Default.logfile;

        /// <summary>
        ///     The file that contains the GBD order information.  i.e. order numbers etc.
        /// </summary>
        public static readonly string GbdOrderFile = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + Settings.Default.GbdOrders;

        /// <summary>
        ///     The invalid field starting characters.
        /// </summary>
        public static readonly Regex InvalidFieldStartingCharacters =
            new Regex("[\\^`~@#$%^&*()-+=|,\\\\<>?/{}\\.!'[\\]:;_0123456789]");

        /// <summary>
        ///     The invalid field characters.
        /// </summary>
        public static readonly Regex InvalidFieldCharacters = new Regex("[-@^\\%#\\s$!*{}\\.<>?/`'[:\\];=+()|,~&]");

        /// <summary>
        ///     The spatial reference factory.
        /// </summary>
        public static readonly ISpatialReferenceFactory SpatialReferenceFactory = new SpatialReferenceEnvironment();

        /// <summary>
        ///     The projected coordinate system.
        /// </summary>
        public static readonly IGeographicCoordinateSystem ProjectedCoordinateSystem =
            SpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        /// <summary>
        ///     The logger.
        /// </summary>
        public static readonly Logger Logger = new Logger(LogFile, false);

        /// <summary>
        ///     The base 32 codes dictionary.
        /// </summary>
        private static readonly Dictionary<char, int> base32CodesDict = Base32Codes.ToDictionary(
            chr => chr,
            chr => Base32Codes.IndexOf(chr));

        public static HashSet<char> invalidStartingChars = new HashSet<char>
                                                               {
                                                                   '\\',
                                                                   '^',
                                                                   '`',
                                                                   '~',
                                                                   '@',
                                                                   '#',
                                                                   '$',
                                                                   '^',
                                                                   '&',
                                                                   '*',
                                                                   '(',
                                                                   ')',
                                                                   '-',
                                                                   '+',
                                                                   '=',
                                                                   '|',
                                                                   ',',
                                                                   '<',
                                                                   '>',
                                                                   '?',
                                                                   '/',
                                                                   '{',
                                                                   '}',
                                                                   '.',
                                                                   '!',
                                                                   ':',
                                                                   ';',
                                                                   '_',
                                                                   '0',
                                                                   '1',
                                                                   '2',
                                                                   '3',
                                                                   '4',
                                                                   '5',
                                                                   '6',
                                                                   '7',
                                                                   '8',
                                                                   '9'
                                                               };

        #endregion

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

            for (var i = 0; i < interiorRingGeometryCollection.GeometryCount; i++)
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

        private static string ConvertPointCollectionToGeoJson(IGeometry ringGeometry)
        {
            var pointCollect = ringGeometry as IPointCollection;
            var builder = new StringBuilder();
            if (pointCollect != null)
            {
                for (var i = 0; i < pointCollect.PointCount; i++)
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

        public static string ConvertPolygonsToGeoJson(List<IPolygon> polygons)
        {
            var output = new StringBuilder("{\"type\":\"MultiPolygon\", \"coordinates\": [");

            for (var i = 0; i < polygons.Count; i++)
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

        public static string ConvertPolygonsGeoJson(IPolygon polygon)
        {
            var output = new StringBuilder("{\"type\":\"MultiPolygon\", \"coordinates\": [");
            var polygonStr = ConvertToPolygonGeoJson(polygon);

            if (string.IsNullOrEmpty(polygonStr))
            {
                return string.Empty;
            }
            output.Append(polygonStr);
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
                for (var i = 0; i < exteriorRingGeometryCollection.GeometryCount; i++)
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

        public static string ConvertPointGeoJson(IPoint point)
        {
            var output = new StringBuilder("{ \"type\": \"Point\", \"coordinates\": ");
            var pointStr = PointToString(point);

            if (string.IsNullOrEmpty(pointStr))
            {
                return string.Empty;
            }
            output.Append(pointStr);
            output.Append("}");
            return output.ToString();
        }

        public static string ConvertPolyLineGeoJson(IPolyline line)
        {
            StringBuilder output = new StringBuilder("{\"type\": \"LineString\",\"coordinates\": [");
            IPointCollection pointCollection = (IPointCollection)line;

            for (int i = 0; i < pointCollection.PointCount; i++)
            {
                if (i != 0)
                {
                    output.Append(",");
                }
                var point = pointCollection.Point[i];

                var pointStr = PointToString(point);

                if (!string.IsNullOrEmpty(pointStr))
                {
                    output.Append(pointStr);
                }
            }
            output.Append("]}");

            return output.ToString();
        }

        public static MultiPolygon CreateMultiPolygon(List<IPolygon> polygons)
        {
            var polyList = new List<Polygon>();

            foreach (var poly in polygons)
            {
                var poly4 = (IPolygon4)poly;
                var exteriorRingGeometryBag = poly4.ExteriorRingBag;
                var exteriorRingGeometryCollection = exteriorRingGeometryBag as IGeometryCollection;

                if (exteriorRingGeometryCollection != null)
                {
                    for (var i = 0; i < exteriorRingGeometryCollection.GeometryCount; i++)
                    {
                        var exteriorRingGeometry = exteriorRingGeometryCollection.Geometry[i];
                        var pointCollect = exteriorRingGeometry as IPointCollection;

                        var coordinates = new List<List<IPosition>>();
                        if (pointCollect != null)
                        {
                            var exteriorPointString = new List<IPosition>();
                            for (var j = 0; j < pointCollect.PointCount; j++)
                            {
                                var point = pointCollect.Point[j];
                                exteriorPointString.Add(new GeographicPosition(point.Y, point.X));
                            }
                            coordinates.Add(exteriorPointString);
                        }

                        var interiorRingBag = poly4.InteriorRingBag[exteriorRingGeometry as IRing];
                        var interiorRingGeometryCollection = interiorRingBag as IGeometryCollection;
                        if (interiorRingGeometryCollection != null)
                        {
                            for (var t = 0; t < interiorRingGeometryCollection.GeometryCount; t++)
                            {
                                var interiorPointCollect =
                                    interiorRingGeometryCollection.Geometry[t] as IPointCollection;

                                if (interiorPointCollect != null)
                                {
                                    var interiorPointString = new List<IPosition>();
                                    for (var z = 0; z < interiorPointCollect.PointCount; z++)
                                    {
                                        var point = interiorPointCollect.Point[z];
                                        interiorPointString.Add(new GeographicPosition(point.Y, point.X));
                                    }
                                    coordinates.Add(interiorPointString);
                                }
                            }
                        }
                        var linestring = new LineString(coordinates[0]);
                    }
                }
            }

            var output = new MultiPolygon();
            return output;
        }

        public static IFeatureClass CreateStandaloneFeatureClass(
            IWorkspace workspace,
            string featureClassName,
            Dictionary<string, string> uniqueNames,
            bool allowNull,
            double defaultValue)
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
            var fieldsEdit = (IFieldsEdit)fields; // Explicit Cast

            var tmpField = new FieldClass();
            IFieldEdit tmpFieldEdit = tmpField;
            tmpFieldEdit.Name_2 = "GeoHash";
            tmpFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            tmpFieldEdit.Length_2 = 20;
            fieldsEdit.AddField(tmpField);

            foreach (var name in uniqueNames.Keys)
            {
                var tempField = new FieldClass();
                IFieldEdit tempFieldEdit = tempField;
                tempFieldEdit.Name_2 = "DG_" + uniqueNames[name];
                if (name.EndsWith("_str"))
                {
                    tempFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    tempFieldEdit.Length_2 = 250;
                    tempFieldEdit.AliasName_2 = name;
                    tempFieldEdit.IsNullable_2 = allowNull;
                    tempFieldEdit.DefaultValue_2 = "";
                }
                else
                {
                    tempFieldEdit.Name_2 = "DG_" + uniqueNames[name];
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
                spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = geographicCoordinateSystem;
            var shapeField = new FieldClass();
            IFieldEdit shapeFieldEdit = shapeField;
            shapeFieldEdit.Name_2 = "SHAPE";
            shapeFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shapeFieldEdit.GeometryDef_2 = geometryDef;
            shapeFieldEdit.IsNullable_2 = true;
            shapeFieldEdit.Required_2 = true;
            fieldsEdit.AddField(shapeField);
            fields = fieldsEdit;

            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.
            try
            {
                var featureClass = featureWorkspace.CreateFeatureClass(
                    featureClassName,
                    validatedFields,
                    ocDesc.InstanceCLSID,
                    ocDesc.ClassExtensionCLSID,
                    esriFeatureType.esriFTSimple,
                    "SHAPE",
                    string.Empty);
                return featureClass;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new Exception("Issue with creating feature class");
            }
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

        public static List<string> GetFieldList(IFeatureClass fc)
        {
            var outlist = new List<string>();
            for (var i = 0; i < fc.Fields.FieldCount; i++)
            {
                outlist.Add(fc.Fields.get_Field(i).Name);
            }
            return outlist;
        }

        /// <summary>
        ///     Get all the selected Polygons from the map document
        /// </summary>
        /// <param name="focusMap">Map document with selected polygons</param>
        /// <returns>List of IPolygons</returns>
        public static List<IPolygon> GetPolygons(IMap focusMap)
        {
            var polygons = new List<IPolygon>();

            var pEnumFeat = (IEnumFeature)focusMap.FeatureSelection;
            pEnumFeat.Reset();

            try
            {
                IFeature pfeat;
                while ((pfeat = pEnumFeat.Next()) != null)
                {
                    var geo = (IPolygon)pfeat.ShapeCopy;
                    var geo2 = (IPolygon)ProjectToWGS84(geo);
                    polygons.Add(geo2);
                }
            }
            catch (Exception error)
            {
                Logger.Error(error);
            }

            return polygons;
        }


        public static List<IGeometry> GetSelectedGeometries(IMap focusMap)
        {
            var geometries = new List<IGeometry>();
            var pEnumFeat = (IEnumFeature)focusMap.FeatureSelection;
            pEnumFeat.Reset();

            try
            {
                IFeature pfeat;
                while ((pfeat = pEnumFeat.Next()) != null)
                {
                    var geo = pfeat.ShapeCopy;
                    
                    var geo2 = ProjectToWGS84(geo);
                    geometries.Add(geo2);
                }
            }
            catch (Exception error)
            {
                Logger.Error(error);
            }
            return geometries;
        }


        public static string CreateGeometryCollectionGeoJson(List<IGeometry> geometries)
        {
            var output = new StringBuilder("{\"type\": \"GeometryCollection\",\"geometries\": [");
            for (int i = 0; i < geometries.Count; i++)
            {
                if (i != 0)
                {
                    output.Append(",");
                }

                var geom = geometries[i];

                var geometryStr = string.Empty;
                switch (geom.GeometryType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        var point = (IPoint)geom;
                        geometryStr = ConvertPointGeoJson(point);
                        break;

                    case esriGeometryType.esriGeometryPolyline:
                        var line = (IPolyline)geom;
                        geometryStr = ConvertPolyLineGeoJson(line);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        var polygon = (IPolygon)geom;
                        geometryStr = ConvertPolygonsGeoJson(polygon);
                        break;
                }

                if (!string.IsNullOrEmpty(geometryStr))
                {
                    output.Append(geometryStr);
                }
            }

            output.Append("]}");
            return output.ToString();
        }

        /// <summary>
        ///     Generic method to load objects of type T from file that are serialized via JSON.
        /// </summary>
        /// <param name="path">
        ///     The path.
        /// </param>
        /// <typeparam name="T">
        ///     Object that is serialized via JSON in the file
        /// </typeparam>
        /// <returns>
        ///     The <see cref="T[]" />.
        /// </returns>
        public static T[] LoadObjectsFromFile<T>(string path)
        {
            var lines = File.ReadAllLines(path);
            var objects = new List<T>();
            for (var i = 0; i <= lines.Length - 1; i++)
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

        /// <summary>
        ///     Open the GBDX cloud workspace.  This defaults to the GBDX cloud file GDB.
        /// </summary>
        /// <param name="path">
        ///     File system path to the file GDB.
        /// </param>
        /// <returns>
        ///     IWorkspace that was opened on the file GDB.
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

        private static string PointToString(IPoint point)
        {
            return "[" + point.X + ", " + point.Y + "]";
        }

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

        public static void ConvertPagesToFeatureClass(string filepath, string layerName)
        {
            try
            {
                var json = MergeJsonStrings(filepath);

                json = MergeProperties(json);

                var jsonOutput = json.ToString(Formatting.None);

                var workspace = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);

                IFieldChecker fieldChecker = new FieldCheckerClass();
                fieldChecker.ValidateWorkspace = workspace;

                var proposedTableName = string.Format("AnswerFactory{0}", Guid.NewGuid());
                string tableName;

                fieldChecker.ValidateTableName(proposedTableName, out tableName);

                WriteToTable(workspace, jsonOutput, tableName, new object());

                //this.Invoke((MethodInvoker)(() => { AddLayerToMap(tableName, layerName); }));

                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
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

        private static bool WriteToTable(IWorkspace workspace, string featureClassJson, string tableName, object locker)
        {
            var success = true;
            if (string.IsNullOrEmpty(featureClassJson))
            {
                return false;
            }

            try
            {
                var outputTable = GetTable(featureClassJson);
                lock (locker)
                {
                    outputTable.SaveAsTable(workspace, tableName);
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
                success = false;
            }

            return success;
        }

        private static JObject MergeJsonStrings(string filePath)
        {
            var mergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union };

            JObject jsonObject = null;
            try
            {
                string line;
                using (var file = new StreamReader(filePath))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (jsonObject != null)
                        {
                            jsonObject.Merge(JObject.Parse(line), mergeSettings);
                        }
                        else
                        {
                            jsonObject = JObject.Parse(line);
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
                return null;
            }

            return jsonObject;
        }

        private static JObject MergeProperties(JObject jsonObject)
        {
            try
            {
                var fields = new Dictionary<string, FieldDefinition>();
                if (jsonObject != null)
                {
                    var jsonFields = (JArray)jsonObject["fields"];

                    // This for loops looks through all of the fields and updtes the max length for the fields
                    foreach (var t in jsonFields)
                    {
                        var tempFieldDefintion = JsonConvert.DeserializeObject<FieldDefinition>(t.ToString());
                        FieldDefinition value;

                        // Attempt to get the value if one is received will evaluate to true and return the object.
                        if (fields.TryGetValue(tempFieldDefintion.Alias, out value))
                        {
                            if (value.Length < tempFieldDefintion.Length)
                            {
                                value.Length = tempFieldDefintion.Length;
                            }
                        }
                        else
                        {
                            fields.Add(tempFieldDefintion.Alias, tempFieldDefintion);
                        }
                    }

                    jsonFields.RemoveAll();
                    foreach (var defintion in fields.Values)
                    {
                        jsonFields.Add(JObject.FromObject(defintion));
                    }
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
                return null;
            }

            return jsonObject;
        }

    }
}