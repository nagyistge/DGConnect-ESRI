using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Gbdx
{
    using System.Windows.Forms;

    using Gbdx.Utilities_and_Configuration.Forms;

    using Ionic.Zip;

    public class VectorUpload : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public VectorUpload()
        {
        }

        protected override void OnClick()
        {
            try
            {
                var openFileDialog = new OpenFileDialog() { Multiselect = false, };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var directoryPath = Path.GetDirectoryName(openFileDialog.FileName);

                    var newZip = directoryPath + "\\"
                                       + (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds+".zip";

                    MappingForm mapForm = new MappingForm();

                    if (mapForm.ShowDialog() == DialogResult.OK)
                    {
                        string mappingProps = "mapping.properties";
                        const string UserContributions = "User Contributions";
                        string itemType = mapForm.ItemName;
                        string spatialReference = "";

                        // Write the mapping.properties file.
                        if (!File.Exists(mappingProps))
                        {
                            using (var sw = File.CreateText(mappingProps))
                            {
                                sw.WriteLine("vector.crs={0}", spatialReference);
                                sw.WriteLine("vector.ingestSource={0}", UserContributions);
                                sw.WriteLine("vector.itemType={0}", itemType);
                                sw.WriteLine(
                                    "vector.index=vector-{0}-{1}-{2}",
                                    UserContributions,
                                    itemType,
                                    DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
                                sw.WriteLine("tagger_id=source");
                                sw.WriteLine("id=name");
                                sw.Flush();
                                sw.Close();
                            }
                        }
                        using (var zip = new ZipFile())
                        {
                            zip.AddFile(openFileDialog.FileName, "");
                            zip.AddFile(mappingProps);
                            zip.Save(newZip);
                        }

                        File.Delete(mappingProps);
                    }
                }
            }
            catch (Exception error)
            {
                
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
