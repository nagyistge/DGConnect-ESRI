using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Gbdx
{
    using System.Windows.Forms;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.DataSourcesFile;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Gbdx.Utilities_and_Configuration.Forms;

    using GbdxSettings.Properties;

    using GbdxTools;

    using Ionic.Zip;

    using NetworkConnections;

    using Path = System.IO.Path;

    public class VectorUpload : ESRI.ArcGIS.Desktop.AddIns.Button
    {

        private readonly IGbdxComms comms;

        public VectorUpload()
        {
            this.comms = new GbdxComms(Jarvis.LogFile, false);
        }

        protected override void OnClick()
        {
            try
            {
                var openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "Shape Files (*.shp)|*.shp" };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var directoryPath = Path.GetDirectoryName(openFileDialog.FileName);

                    var newZip = directoryPath + "\\"
                                 + (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds + ".zip";

                    MappingForm mapForm = new MappingForm();

                    if (mapForm.ShowDialog() == DialogResult.OK)
                    {
                        string mappingProps = "mapping.properties";
                        const string userContributions = "User Contributions";
                        string itemType = mapForm.ItemName;
                        string spatialReference = GetSpatialReference(openFileDialog.FileName);


                        // If the spatial projection doesn't match tell the user what's up and stop processing from there
                        if (!string.Equals("EPSG:4326", spatialReference))
                        {
                            MessageBox.Show(GbdxSettings.GbdxResources.wrongSpatialReference);
                            return;
                        }

                        // Write the mapping.properties file.
                        if (!File.Exists(mappingProps))
                        {
                            using (var sw = File.CreateText(mappingProps))
                            {
                                sw.WriteLine("vector.crs={0}", spatialReference);
                                sw.WriteLine("vector.ingestSource={0}", userContributions);
                                sw.WriteLine("vector.itemType={0}", itemType);

                                var indexLine = string.Format(
                                    "vector.index=vector-{0}-{1}-{2}",
                                    userContributions,
                                    itemType,
                                    DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
                                indexLine = indexLine.ToLower().Replace(":", "").Replace(" ", "");

                                sw.WriteLine(indexLine);
                                sw.WriteLine("tagger_id=source");
                                sw.WriteLine("id=name");
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        using (var zip = new ZipFile())
                        {
                            AddFile(zip, openFileDialog.FileName);
                            AddFile(zip, openFileDialog.FileName + ".xml");
                            AddFile(zip, Path.ChangeExtension(openFileDialog.FileName, ".dbf"));
                            AddFile(zip, Path.ChangeExtension(openFileDialog.FileName, ".shx"));
                            AddFile(zip, Path.ChangeExtension(openFileDialog.FileName, ".prj"));
                            AddFile(zip, Path.ChangeExtension(openFileDialog.FileName, ".CPG"));
                            zip.AddFile(mappingProps);
                            zip.Save(newZip);
                        }

                        File.Delete(mappingProps);

                        NetObject netobj = new NetObject()
                                               {
                                                   BaseUrl = Settings.Default.baseUrl,
                                                   AddressUrl = "/insight-vector/api/vector/vector-upload",
                                                   AuthUrl =
                                                       string.IsNullOrEmpty(Settings.Default.AuthBase)
                                                           ? Settings.Default.DefaultAuthBase
                                                           : Settings.Default.AuthBase,
                                                   AuthEndpoint = Settings.Default.authenticationServer,
                                                   User = Settings.Default.username,
                                                   ApiKey = Settings.Default.apiKey,
                                               };

                        string decryptedPassword;
                        var success = Aes.Instance.Decrypt128(Settings.Default.password, out decryptedPassword);
                        if (!success)
                        {
                            MessageBox.Show(GbdxSettings.GbdxResources.InvalidUserPass);
                            return;
                        }
                        netobj.Password = decryptedPassword;

                        this.comms.UploadFile(netobj, newZip);
                    }
                }
            }
            catch (Exception error)
            {
            }
        }

        private static bool AddFile(ZipFile zip, string path)
        {
            if (File.Exists(path))
            {
                zip.AddFile(path, "");
                return true;
            }
            return false;
        }

        private static string GetSpatialReference(string shapeFilePath)
        {
            var workspaceFactory = new ShapefileWorkspaceFactoryClass();
            var featureWorkspace = (IFeatureWorkspace)workspaceFactory.OpenFromFile(Path.GetDirectoryName(shapeFilePath), 0);
            var featureClass = featureWorkspace.OpenFeatureClass(Path.GetFileNameWithoutExtension(shapeFilePath));

            ISpatialReference spatialReference = null;
            if (featureClass != null)
            {
                IGeoDataset geoDataset = featureClass as IGeoDataset;
                spatialReference = geoDataset.SpatialReference;
                return "EPSG:"+spatialReference.FactoryCode;
            }
            return string.Empty;
        }

        protected override void OnUpdate()
        {
        }
    }
}
