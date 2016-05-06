// Author: Russ Wittmer

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace Gbdx.Answer_Factory
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using AnswerFactory;

    using GbdxSettings;
    using GbdxSettings.Properties;

    using System.Windows.Threading;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Gbdx.Vector_Index;

    using GbdxTools;

    using NetworkConnections;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using RestSharp;

    using FileStream = System.IO.FileStream;
    using ListViewItem = System.Windows.Forms.ListViewItem;
    using Path = System.IO.Path;
    using UserControl = System.Windows.Forms.UserControl;

    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class AnswerFactoryDockableWindow : UserControl
    {
        private static readonly object locker = new object();

        private string token;

        List<Recipe> recipeList = new List<Recipe>();

        List<Project2> existingProjects = new List<Project2>();

        private IRestClient client;

        private ICommandItem PreviouslySelectedItem { get; set; }

        private IPolygon drawnPolygon = null;

        private IElement drawnElement = null;

        private DataTable ProjIdRepo;

        private DataTable RecipeRepo;

        private List<string> selectedAois;

        public AnswerFactoryDockableWindow(object hook)
        {
            this.InitializeComponent();
            this.client = new RestClient(Settings.Default.baseUrl);
            this.GetToken();
            this.Hook = hook;
            this.selectionTypecomboBox.SelectedIndex = 0;

            this.ProjIdRepo = CreateProjIdDataTable();
            this.projectNameDataGridView.DataSource = this.ProjIdRepo;

            var dataGridViewColumn = this.projectNameDataGridView.Columns["Id"];
            if (dataGridViewColumn != null)
            {
                dataGridViewColumn.Visible = false;
            }
            this.RecipeRepo = CreateRecipeInfoDataDatable();

            this.recipeStatusDataGridView.DataSource = this.RecipeRepo;

            var recipeGridDataCol0 = this.recipeStatusDataGridView.Columns["Recipe Name"];
            if (recipeGridDataCol0 != null)
            {
                recipeGridDataCol0.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }


            var recipeGridDataCol1 = this.recipeStatusDataGridView.Columns["Status"];
            if (recipeGridDataCol1 != null)
            {
                recipeGridDataCol1.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }


            this.selectedAois = new List<string>();
        }



        /// <summary>
        /// Get Token to use with GBDX services
        /// </summary>
        /// <returns></returns>
        private void GetToken()
        {
            IRestClient restClient = new RestClient(Settings.Default.AuthBase);

            string password;
            var result = Encryption.Aes.Instance.Decrypt128(Settings.Default.password, out password);

            if (!result)
            {
                Jarvis.Logger.Warning("PASSWORD FAILED DECRYPTION");
                return;
            }

            var request = new RestRequest(Settings.Default.authenticationServer, Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", string.Format("Basic {0}", Settings.Default.apiKey));
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", Settings.Default.username);
            request.AddParameter("password", password);

            //var response = restClient.Execute<AccessToken>(request);
            restClient.ExecuteAsync<AccessToken>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());

                        if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                        {
                            this.token = resp.Data.access_token;
                            this.GetRecipes();
                            this.GetExistingProjects();
                        }
                        else
                        {
                            this.token = string.Empty;
                        }
                    });
        }

        private void GetRecipes()
        {
            this.CheckBaseUrl();
            var request = new RestRequest("/answer-factory-recipe-service/api/recipes", Method.GET);
            request.AddHeader("Authorization", "Bearer" + this.token);
            request.AddHeader("Content-Type", "application/json");

            this.client.ExecuteAsync<List<Recipe>>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());
                        if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                        {
                            this.recipeList = resp.Data;
                            this.Invoke(
                                (MethodInvoker)(() =>
                                    {
                                        foreach (var item in this.recipeList)
                                        {
                                            this.availableRecipesCombobox.Items.Add(item.name);
                                        }
                                    }));
                        }
                    });
        }

        private void CheckBaseUrl()
        {
            if (this.client == null || !this.client.BaseUrl.Equals(new Uri(Settings.Default.baseUrl)))
            {
                this.client = new RestClient(Settings.Default.baseUrl);
            }
        }

        private string CreateProjectJson(List<IPolygon> polygons)
        {
            // get the geojson of the aois
            var aoi = Jarvis.ConvertPolygonsToGeoJson(polygons);
            var newProject = new Project();
            newProject.aois.Add(aoi);
            newProject.name = this.projectNameTextbox.Text;

            if (this.availableRecipesCombobox.SelectedIndex != -1)
            {
                var recName =
                    this.availableRecipesCombobox.Items[this.availableRecipesCombobox.SelectedIndex].ToString();
                var recipe = this.GetRecipe(recName);

                if (recipe != null)
                {
                    var recipeConfig = new RecipeConfig { recipeId = recipe.id, recipeName = recipe.name };
                    newProject.recipeConfigs.Add(recipeConfig);
                }
            }

            var projectJson = JsonConvert.SerializeObject(newProject).Replace("\\", "");

            projectJson = projectJson.Replace("\"aois\":[\"{\"", "\"aois\":[{\"");
            projectJson = projectJson.Replace("\"],\"recipeConfigs\"", "],\"recipeConfigs\"");
            return projectJson;
        }

        private Recipe GetRecipe(string name)
        {
            IEnumerable<Recipe> query = (from q in this.recipeList where q.name.Equals(name) select q);

            if (query.Count() == 1)
            {
                return query.Single();
            }
            return null;
        }

        private void GetExistingProjects()
        {
            var request = new RestRequest("/answer-factory-project-service/api/project", Method.GET);
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");

            this.CheckBaseUrl();
            this.client.ExecuteAsync<List<ProjId>>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());

                        if (resp.Data != null && resp.StatusCode == HttpStatusCode.OK)
                        {

                            foreach (var item in resp.Data)
                            {
                                var row = this.ProjIdRepo.NewRow();
                                row["Project Name"] = item.name;
                                row["Id"] = item.id;
                                this.ProjIdRepo.Rows.Add(row);
                            }

                            // Update the list of projects with an unknown status
                            this.Invoke((MethodInvoker)(() =>
                                {
                                    this.projectNameDataGridView.Refresh();
                                    this.projectNameDataGridView.PerformLayout();
                                }));
                        }
                    });
        }

        private void GetResult(string id, string recipeName, string projectName)
        {
            this.CheckBaseUrl();

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("No Project id");
                return;
            }
            var request = new RestRequest(
                string.Format("/answer-factory-recipe-service/api/result/project/{0}", id),
                Method.GET);

            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");
            this.client.ExecuteAsync<List<ResultItem>>(
                request,
                resp =>
                    {
                        ProcessResult(projectName, recipeName, resp, this.selectedAois, this.token, this.client);
                    });
        }

        private void ProcessResult(
            string projectName,
            string recipeName,
            IRestResponse<List<ResultItem>> resp,
            List<string> AoiList,
            string token,
            IRestClient client)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());
            string layername = string.Empty;
            string aoi = ConvertAoisToGeometryCollection(AoiList);
            if (resp.Data != null)
            {
                foreach (var item in resp.Data)
                {
                    // if the recipe names don't match move on to the next
                    if (!recipeName.Equals(item.recipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    layername = string.Format("{0}|{1}", projectName, item.recipeName);

                    if(!string.IsNullOrEmpty(aoi) && !string.IsNullOrEmpty(layername))
                    {
                        GetGeometries(item.properties.query_string, token, aoi, client, layername);
                    }
                }
            }
        }


        private void GetProjectRecipes(string projectid)
        {
            this.CheckBaseUrl();
            
            // Clear out the selected AOIS from a previous project/recipe query
            this.selectedAois.Clear();

            var request =
                new RestRequest(string.Format("/answer-factory-project-service/api/project/{0}", projectid));
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");

            this.client.ExecuteAsync<List<Project2>>(
                request,
                resp =>
                {
                    Jarvis.Logger.Info(resp.ResponseUri.ToString());

                    if (resp.Data != null)
                    {
                        
                        foreach (var item in resp.Data)
                        {
                            this.selectedAois.AddRange(item.aois);
                            foreach (var recipe in item.recipeConfigs)
                            {
                                var newRow = this.RecipeRepo.NewRow();
                                newRow["Recipe Name"] = recipe.recipeName;
                                newRow["Status"] = "Working";
                                this.RecipeRepo.Rows.Add(newRow);
                            }
                        }
                    }
                    this.Invoke(
                            (MethodInvoker)(() =>
                            {
                                this.recipeStatusDataGridView.Refresh();
                                this.recipeStatusDataGridView.PerformLayout();
                            }));
                    this.GetRecipeStatus(projectid);
                });
        }

        private void GetRecipeStatus(string projectId)
        {
            this.CheckBaseUrl();

            var request =
                new RestRequest(string.Format("/answer-factory-recipe-service/api/result/project/{0}", projectId));
            request.AddHeader("Authorization", "Bearer " + this.token);
            request.AddHeader("Content-Type", "application/json");

            this.client.ExecuteAsync<List<ResultItem>>(
                request,
                resp =>
                {
                    Jarvis.Logger.Info(resp.ResponseUri.ToString());
                    if (resp.Data != null)
                    {
                        foreach (var item in resp.Data)
                        {
                            var query = from row in this.RecipeRepo.AsEnumerable()
                                        where (string)row["Recipe Name"] == item.recipeName
                                        select row;
                            foreach (DataRow queryItem in query)
                            {
                                queryItem["Status"] = item.status;
                            }
                        }
                    }

                    this.Invoke(
                            (MethodInvoker)(() =>
                            {
                                this.recipeStatusDataGridView.Refresh();
                                this.recipeStatusDataGridView.PerformLayout();
                            }));

                });
        }

        private static string ConvertAoisToGeometryCollection(List<string> aois)
        {
            StringBuilder builder = new StringBuilder("{\"type\":\"GeometryCollection\",\"geometries\":[");
            foreach (var item in aois)
            {
                builder.Append(item + ",");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("]}");
            return builder.ToString();
        }
        
        #region Download Vector Items

        private void GetGeometries(string query, string token, string aoi, IRestClient client, string layerName, int attempt = 0)
        {
            var request = new RestRequest("/insight-vector/api/shape/query/geometries?q=" + query, Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", aoi, ParameterType.RequestBody);

            attempt++;

            client.ExecuteAsync<ResponseData>(
                request,
                resp => GetGeometriesResponseProcess(resp, query, token, aoi, client,layerName, attempt));
        }

        private void GetGeometriesResponseProcess(
            IRestResponse<ResponseData> resp,
            string query,
            string token,
            string aoi,
            IRestClient client,
            string layerName,
            int geomAttempts)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            // Check to see if we have a good status if not try it again
            if (resp.StatusCode != HttpStatusCode.OK && geomAttempts <= 3)
            {
                GetGeometries(query, token, aoi, client, layerName, geomAttempts);
            }
            else if (geomAttempts > 3)
            {
                MessageBox.Show("An Error occurred.  Please try again");
                return;
            }

            if (resp.Data != null)
            {
                var sources = resp.Data;

                foreach (var source in sources.data)
                {
                    GetTypes(source.name, query, token, aoi, client, layerName);
                }
            }

        }

        private void GetTypes(
            string geometry,
            string query,
            string token,
            string aoi,
            IRestClient client,
            string layerName,
            int attempts = 0)
        {
            var request =
                new RestRequest(
                    string.Format("/insight-vector/api/shape/query/{0}/types?q={1}", geometry, query),
                    Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", aoi, ParameterType.RequestBody);

            attempts++;

            client.ExecuteAsync<ResponseData>(
                request,
                resp => GetTypesResponseProcess(resp, geometry, query, token, aoi, client,layerName, attempts));

        }

        private void GetTypesResponseProcess(
            IRestResponse<ResponseData> resp,
            string geometry,
            string query,
            string token,
            string aoi,
            IRestClient client,
            string layerName,
            int typeAttempts)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            // Check to see if we have a good status if not try it again
            if (resp.StatusCode != HttpStatusCode.OK && typeAttempts <= 3)
            {
                GetTypes(geometry, query, token, aoi, client,layerName, typeAttempts);
            }
            else if (typeAttempts > 3)
            {
                MessageBox.Show("An Error occurred.  Please try again");
                return;
            }

            if (resp.Data != null)
            {
                var types = resp.Data;
                foreach (var type in types.data)
                {
                    GetPagingId(geometry, type.name, query, token, aoi, client, layerName);
                }
            }


        }

        private void GetPagingId(
            string geometry,
            string type,
            string query,
            string token,
            string aoi,
            IRestClient client,
            string layerName,
            int attempts = 0)
        {
            var request =
                new RestRequest(
                    string.Format("/insight-vector/api/shape/query/{0}/{1}/paging?q={2}", geometry, type, query),
                    Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", aoi, ParameterType.RequestBody);

            attempts ++;

            client.ExecuteAsync<PageId>(
                request,
                resp => GetPageIdResponseProcess(resp, geometry, type, query, token, aoi, client, layerName, attempts));

        }

        private void GetPageIdResponseProcess(
            IRestResponse<PageId> resp,
            string geometry,
            string type,
            string query,
            string token,
            string aoi,
            IRestClient client,
            string layerName,
            int pageIdAttempts = 0)
        {
            Jarvis.Logger.Info(resp.ResponseUri.ToString());

            // Check to see if we have a good status if not try it again
            if (resp.StatusCode != HttpStatusCode.OK && pageIdAttempts <= 3)
            {
                GetPagingId(geometry, type, query, token, aoi, client,layerName, pageIdAttempts);
            }
            else if (pageIdAttempts > 3)
            {
                MessageBox.Show("An Error occurred.  Please try again");
                return;
            }
            if (resp.Data != null)
            {
                var pageId = resp.Data.pagingId;
                var tempFile = Path.GetTempFileName();

                var fileStream = File.Open(tempFile, FileMode.Append);
                var fileStreamWriter = new StreamWriter(fileStream);

                GetPages(pageId,token, client, fileStreamWriter, layerName);
            }
        }

        private void GetPages(string pageId, string token, IRestClient client, StreamWriter fileStreamWriter, string layerName, int attempts=0)
        {
            var request = new RestRequest("/insight-vector/api/esri/paging", Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("ttl", "5m");
            request.AddParameter("fields", "attributes");
            request.AddParameter("pagingId", pageId);

            attempts++;
            client.ExecuteAsync<PagedData2>(request, resp => ProcessPageResponse(resp, token, pageId, client, layerName, attempts, fileStreamWriter));
        }

        private void ProcessPageResponse(
            IRestResponse<PagedData2> resp,
            string token,
            string pageId,
            IRestClient client,
            string layerName,
            int pageAttempts,
            StreamWriter fileStreamWriter)
        {
            try
            {
                Jarvis.Logger.Info(resp.ResponseUri.ToString());

                if (resp.StatusCode != HttpStatusCode.OK && pageAttempts <= 3)
                {
                    GetPages(pageId, token, client, fileStreamWriter, layerName, pageAttempts);
                }
                else if (pageAttempts > 3)
                {
                    MessageBox.Show("An error occurred.  Please try again");
                    return;
                }

                if (resp.Data != null)
                {
                    if (resp.Data.item_count != "0")
                    {
                        fileStreamWriter.WriteLine(resp.Data.data.ToString().Replace("\r", "").Replace("\n", ""));
                        GetPages(resp.Data.next_paging_id, token, client, fileStreamWriter, layerName);
                    }
                    else
                    {
                        FileStream fs = (FileStream)fileStreamWriter.BaseStream;
                        string filepath = fs.Name;
                        fileStreamWriter.Close();
                        ConvertPagesToFeatureClass(filepath, layerName);
                    }
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        private void ConvertPagesToFeatureClass(string filepath, string layerName)
        {
            try
            {


                var json = MergeJsonStrings(filepath);

                json = MergeProperties(json);

                string jsonOutput = json.ToString(Formatting.None);

                IWorkspace workspace = Jarvis.OpenWorkspace(Settings.Default.geoDatabase);

                IFieldChecker fieldChecker = new FieldCheckerClass();
                fieldChecker.ValidateWorkspace = workspace;

                var proposedTableName = string.Format("AnswerFactory{0}", Guid.NewGuid());
                string tableName;

                fieldChecker.ValidateTableName(proposedTableName, out tableName);

                WriteToTable(workspace, jsonOutput, tableName);

                this.Invoke((MethodInvoker)(() => { AddLayerToMap(tableName, layerName); }));
                
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
                    foreach (JToken t in jsonFields)
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

        private static bool WriteToTable(IWorkspace workspace, string featureClassJson, string tableName)
        {
            var success = true;
            if (string.IsNullOrEmpty(featureClassJson))
            {
                return false;
            }

            try
            {
                var outputTable = VectorIndexHelper.GetTable(featureClassJson);
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

        private static bool AddLayerToMap(string tableName, string layerName)
        {
            var success = false;
            try
            {
                lock (locker)
                {
                    var featureWorkspace = (IFeatureWorkspace)Jarvis.OpenWorkspace(Settings.Default.geoDatabase);
                    var featureClass = featureWorkspace.OpenFeatureClass(tableName);
                    ILayer featureLayer;
                    featureLayer = VectorIndexHelper.CreateFeatureLayer(featureClass, layerName);
                    VectorIndexHelper.AddFeatureLayerToMap(featureLayer);
                    success = true;
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
                success = false;
            }

            return success;
        }

        #endregion

        #region User Interface Setup

        private static DataTable CreateProjIdDataTable()
        {
            var dt = new DataTable();

            dt.Columns.Add(new DataColumn("Project Name", typeof(string)) { ReadOnly = true });
            dt.Columns.Add(new DataColumn("Id", typeof(string)) { ReadOnly = true });
            
            var primary = new DataColumn[1];
            primary[0] = dt.Columns["Id"];

            dt.PrimaryKey = primary;

            return dt;
        }

        private static DataTable CreateRecipeInfoDataDatable()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("Recipe Name", typeof(string)) { ReadOnly = false });
            dt.Columns.Add(new DataColumn("Status", typeof(string)) { ReadOnly = false });

            return dt;
        }
        #endregion

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook { get; set; }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private AnswerFactoryDockableWindow m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new AnswerFactoryDockableWindow(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                {
                    m_windowUI.Dispose(disposing);
                }

                base.Dispose(disposing);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // check for a project name
                if (string.IsNullOrEmpty(this.projectNameTextbox.Text))
                {
                    MessageBox.Show("Project Name Required.");
                    return;
                }

                List<IPolygon> polygons;
                if(this.selectionTypecomboBox.SelectedIndex == 1)
                {
                    polygons = Jarvis.GetPolygons(ArcMap.Document.FocusMap);

                    // check to make sure an aoi(s) have been selected.
                    if (polygons.Count == 0)
                    {
                        MessageBox.Show("Please select polygon(s)");
                        return;
                    }
                }
                else
                {
                    if (this.drawnPolygon == null)
                    {
                        MessageBox.Show(
                            "Please draw a bounding box by clicking Draw button and clicking and dragging on the map");
                        return;
                    }

                    polygons = new List<IPolygon> { this.drawnPolygon };
                }

                var projectJson = this.CreateProjectJson(polygons);
                var request = new RestRequest("/answer-factory-project-service/api/project", Method.POST);
                request.AddHeader("Authorization", "Bearer " + this.token);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", projectJson, ParameterType.RequestBody);
                this.CheckBaseUrl();
                this.client.ExecuteAsync(request, resp => { Jarvis.Logger.Info(resp.ResponseUri.ToString()); });

                this.projectNameTextbox.Clear();
                this.availableRecipesCombobox.SelectedIndex = -1;

                this.GetExistingProjects();
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        private void showResultsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.projectNameDataGridView.SelectedRows.Count <= 0 || this.recipeStatusDataGridView.SelectedRows.Count <=0)
                {
                    MessageBox.Show("Please select a project and recipe");
                    return;
                }
                var projectId = this.projectNameDataGridView.SelectedRows[0].Cells["Id"].Value;
                var recipeName = this.recipeStatusDataGridView.SelectedRows[0].Cells["Recipe Name"].Value;
                var projectName = this.projectNameDataGridView.SelectedRows[0].Cells["Project Name"].Value;

                if (projectId == null || recipeName == null || projectName == null)
                {
                    MessageBox.Show("Selection Error");
                    return;
                }

                this.GetResult(projectId.ToString(), recipeName.ToString(), projectName.ToString());
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        private void resultRefrshButton_Click(object sender, EventArgs e)
        {
            this.GetExistingProjects();
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            this.projectNameTextbox.Clear();
            this.availableRecipesCombobox.SelectedIndex = -1;

            // if there is already a element on the screen remove it.
            if (this.drawnElement != null)
            {
                ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.drawnElement);
            }
            this.drawnPolygon = null;
            this.drawnElement = null;

        }

        private void selectionTypecomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.selectionTypecomboBox.SelectedIndex == 0)
            {
                this.drawButton.Enabled = true;
            }
            else
            {
                this.drawButton.Enabled = false;
            }
        }

        private void drawButton_Click(object sender, EventArgs e)
        {
            // if there is already a element on the screen remove it.
            if (this.drawnElement != null)
            {
                 ArcUtility.DeleteElementFromGraphicContainer(ArcMap.Document.ActiveView, this.drawnElement);
            }

            this.drawnPolygon = null;
            this.drawnElement = null;

            // Unsubscribe to pre-existing events if any then subscibe so only one 
            // subscription is active.
            AnswerFactoryRelay.Instance.AoiHasBeenDrawn -= this.Instance_AoiHasBeenDrawn;
            AnswerFactoryRelay.Instance.AoiHasBeenDrawn += this.Instance_AoiHasBeenDrawn;

            var commandBars = ArcMap.Application.Document.CommandBars;
            var commandId = new UIDClass(){Value = ThisAddIn.IDs.Gbdx_Answer_Factory_AnswerFactorySelector};
            var commandItem = commandBars.Find(commandId, false, false);

            if (commandItem != null)
            {
                this.PreviouslySelectedItem = ArcMap.Application.CurrentTool;
                ArcMap.Application.CurrentTool = commandItem;
            }
        }

        void Instance_AoiHasBeenDrawn(IPolygon poly, IElement elm)
        {
            //unsubscribe from the event
            AnswerFactoryRelay.Instance.AoiHasBeenDrawn -= this.Instance_AoiHasBeenDrawn;

            this.drawnPolygon = poly;
            this.drawnElement = elm;

            // reset arcmap to whatever tool was selected prior to clicking the draw button 
            ArcMap.Application.CurrentTool = this.PreviouslySelectedItem;
        }

        private void projectNameDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = (DataGridView)sender;

            // Don't do anything if there are no rows.
            if (dgv.Rows.Count <= 0)
            {
                return;
            }

            this.RecipeRepo.Clear();
            this.recipeStatusDataGridView.Refresh();
            this.recipeStatusDataGridView.PerformLayout();

            var id = dgv.SelectedRows[0].Cells["Id"].Value.ToString();
            this.GetProjectRecipes(id);
        }
    }
}