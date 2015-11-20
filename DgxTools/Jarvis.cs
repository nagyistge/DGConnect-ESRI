namespace DgxTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

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
                                                 + DGXSettings.Properties.Settings.Default.logfile;

        /// <summary>
        /// The base 32 codes.
        /// </summary>
        private const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";

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

            foreach (var name in uniqueNames.Keys)
            {

                var tempField = new FieldClass();
                IFieldEdit tempFieldEdit = (IFieldEdit)tempField;
                tempFieldEdit.Name_2 = uniqueNames[name];
                tempFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                tempFieldEdit.Length_2 = 20;
                tempFieldEdit.AliasName_2 = name;
                tempFieldEdit.IsNullable_2 = allowNull;
                tempFieldEdit.DefaultValue_2 = defaultValue;
                fieldsEdit.AddField(tempField);
            }

            var tmpField = new FieldClass();
            IFieldEdit tmpFieldEdit = (IFieldEdit)tmpField;
            tmpFieldEdit.Name_2 = "Name";
            tmpFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            tmpFieldEdit.Length_2 = 20;
            fieldsEdit.AddField(tmpField);

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
        public static readonly Logger Logger =
            new Logger(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DGX\\DGX.log",
                false);

        /// <summary>
        /// Open the DGX cloud workspace.  This defaults to the DGX cloud file GDB.
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
            if (workspaceFactory.IsWorkspace(path + "\\DGX.gdb"))
            {
                workspace = workspaceFactory.OpenFromFile(path + "\\DGX.gdb", 0);
            }
            else
            {
                var workspaceName = workspaceFactory.Create(path + "\\", "DGX.gdb", null, 0);

                // Cast the workspace name object to the IName interface and open the workspace.
                var name = (IName)workspaceName;
                workspace = (IWorkspace)name.Open();
            }

            return workspace;
        }
    }
}