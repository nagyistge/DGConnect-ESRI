// Author: Russ Wittmer

namespace Gbdx.Answer_Factory
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;

    using AnswerFactory;

    using Encryption;

    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    using Gbdx.Vector_Index;

    using GbdxSettings.Properties;

    using GbdxTools;

    using NetworkConnections;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using RestSharp;

    using DockableWindow = ESRI.ArcGIS.Desktop.AddIns.DockableWindow;
    using FileStream = System.IO.FileStream;
    using Path = System.IO.Path;

    /// <summary>
    ///     Designer class of the dockable window add-in. It contains user interfaces that
    ///     make up the dockable window.
    /// </summary>
    public partial class AnswerFactoryDockableWindow : UserControl
    {
        private delegate void ProjectListDelegate(List<ProjId> projects);

        private delegate void RecipeListDelegate(List<Project2> results);

        private delegate void ResultItemListDelegate(List<ResultItem> results);

        #region Fields & Properties

        private static readonly object locker = new object();

        private IRestClient client;

        private IElement drawnElement;

        private IPolygon drawnPolygon;

        private List<Project2> existingProjects = new List<Project2>();

        private readonly DataView projectDataView;

        private readonly DataTable ProjIdRepo;

        private List<Recipe> recipeList = new List<Recipe>();

        private readonly DataTable RecipeRepo;

        private readonly List<string> selectedAois;

        private string token;

        private ICommandItem PreviouslySelectedItem { get; set; }

        /// <summary>
        ///     Host object of the dockable window
        /// </summary>
        private object Hook { get; set; }

        #endregion

        public AnswerFactoryDockableWindow(object hook)
        {
            this.InitializeComponent();
            this.client = new RestClient(Settings.Default.baseUrl);
            this.GetToken();
            this.Hook = hook;
            this.selectionTypecomboBox.SelectedIndex = 0;

            this.ProjIdRepo = CreateProjIdDataTable();
            this.projectDataView = new DataView(this.ProjIdRepo);
            this.projectNameDataGridView.DataSource = this.projectDataView;

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

        private void AnswerFactoryDockableWindow_Load(object sender, EventArgs e)
        {
            this.ActiveControl = this.label1;
            this.projectSearchTextBox.GotFocus += this.SearchBoxGotFocus;
            this.projectSearchTextBox.LostFocus += this.SearchBoxLostFocus;
        }

        private void AnswerFactoryDockableWindow_VisibleChanged(object sender, EventArgs e)
        {
            this.ActiveControl = this.label1;
        }

        private void CheckBaseUrl()
        {
            if (this.client == null || !this.client.BaseUrl.Equals(new Uri(Settings.Default.baseUrl)))
            {
                this.client = new RestClient(Settings.Default.baseUrl);
            }
        }

        private static string ConvertAoisToGeometryCollection(List<string> aois)
        {
            var builder = new StringBuilder("{\"type\":\"GeometryCollection\",\"geometries\":[");
            foreach (var item in aois)
            {
                builder.Append(item + ",");
            }
            builder.Remove(builder.Length - 1, 1);
            builder.Append("]}");
            return builder.ToString();
        }

        private void ConvertPagesToFeatureClass(string filepath, string layerName)
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

        private string CreateProjectJson(List<IPolygon> polygons)
        {
            // get the geojson of the aois
            var aoi = Jarvis.ConvertPolygonsToGeoJson(polygons);
            var newProject = new Project();
            newProject.aois.Add(aoi);

            newProject.originalGeometries.Add(aoi);
            newProject.namedBuffers.Add(new NamedBuffer { name = "original AOI", buffer = aoi });

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
            projectJson = projectJson.Replace("\"originalGeometries\":[\"", "\"originalGeometries\":[");
            projectJson = projectJson.Replace("\"],\"namedBuffers\"", "],\"namedBuffers\"");
            projectJson = projectJson.Replace("\"buffer\":\"{", "\"buffer\":{");
            projectJson = projectJson.Replace("]]}\"}]}", "]]}}]}");

            return projectJson;
        }

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
            var commandId = new UIDClass { Value = ThisAddIn.IDs.Gbdx_Answer_Factory_AnswerFactorySelector };
            var commandItem = commandBars.Find(commandId, false, false);

            if (commandItem != null)
            {
                this.PreviouslySelectedItem = ArcMap.Application.CurrentTool;
                ArcMap.Application.CurrentTool = commandItem;
            }
        }

        private void GetGeometries(
            string query,
            string token,
            string aoi,
            IRestClient client,
            string layerName,
            int attempt = 0)
        {
            var request = new RestRequest("/insight-vector/api/shape/query/geometries?q=" + query, Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", aoi, ParameterType.RequestBody);

            attempt++;

            client.ExecuteAsync<ResponseData>(
                request,
                resp => this.GetGeometriesResponseProcess(resp, query, token, aoi, client, layerName, attempt));
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
                this.GetGeometries(query, token, aoi, client, layerName, geomAttempts);
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
                    this.GetTypes(source.name, query, token, aoi, client, layerName);
                }
            }
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
                this.GetPagingId(geometry, type, query, token, aoi, client, layerName, pageIdAttempts);
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

                this.GetPages(pageId, token, client, fileStreamWriter, layerName);
            }
        }

        private void GetPages(
            string pageId,
            string token,
            IRestClient client,
            StreamWriter fileStreamWriter,
            string layerName,
            int attempts = 0)
        {
            var request = new RestRequest("/insight-vector/api/esri/paging", Method.POST);
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("ttl", "5m");
            request.AddParameter("fields", "attributes");
            request.AddParameter("pagingId", pageId);

            attempts++;
            client.ExecuteAsync<PagedData2>(
                request,
                resp => this.ProcessPageResponse(resp, token, pageId, client, layerName, attempts, fileStreamWriter));
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
                resp =>
                this.GetPageIdResponseProcess(resp, geometry, type, query, token, aoi, client, layerName, attempts));
        }

        private void GetProjects(string authToken)
        {
            var restClient = new RestClient(Settings.Default.baseUrl);
            var request = new RestRequest("/answer-factory-project-service/api/project", Method.GET);
            request.AddHeader("Authorization", "Bearer " + authToken);
            request.AddHeader("Content-Type", "application/json");

            restClient.ExecuteAsync<List<ProjId>>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());

                        if (resp.Data != null && resp.StatusCode == HttpStatusCode.OK)
                        {
                            this.Invoke(new ProjectListDelegate(this.UpdateProjectsUi), resp.Data);
                        }
                    });
        }

        private Recipe GetRecipe(string name)
        {
            var query = from q in this.recipeList where q.name.Equals(name) select q;

            if (query.Count() == 1)
            {
                return query.Single();
            }
            return null;
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

        private void GetRecipes(string authToken, string projectId)
        {
            var restClient = new RestClient(Settings.Default.baseUrl);
            var request = new RestRequest(string.Format("/answer-factory-project-service/api/project/{0}", projectId));
            request.AddHeader("Authorization", "Bearer " + authToken);
            request.AddHeader("Content-Type", "application/json");

            restClient.ExecuteAsync<List<Project2>>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());
                        if (resp.Data != null)
                        {
                            this.Invoke(new RecipeListDelegate(this.UpdateUiWithRecipes), resp.Data);
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
                    this.ProcessResult(projectName, recipeName, resp, this.selectedAois, this.token, this.client);
                });
        }

        private void GetRecipeStatus(string authToken, string projectId)
        {
            var restClient = new RestClient(Settings.Default.baseUrl);
            var request =
                new RestRequest(string.Format("/answer-factory-recipe-service/api/result/project/{0}", projectId));
            request.AddHeader("Authorization", "Bearer " + authToken);
            request.AddHeader("Content-Type", "application/json");

            restClient.ExecuteAsync<List<ResultItem>>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());
                        if (resp.Data != null)
                        {
                            this.Invoke(new ResultItemListDelegate(this.UpdateRecipeUiStatus), resp.Data);
                        }
                    });
        }
        /// <summary>
        ///     Get Token to use with GBDX services
        /// </summary>
        /// <returns></returns>
        private void GetToken()
        {
            IRestClient restClient = new RestClient(Settings.Default.AuthBase);

            string password;
            var result = Aes.Instance.Decrypt128(Settings.Default.password, out password);

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

            restClient.ExecuteAsync<AccessToken>(
                request,
                resp =>
                    {
                        Jarvis.Logger.Info(resp.ResponseUri.ToString());

                        if (resp.StatusCode == HttpStatusCode.OK && resp.Data != null)
                        {
                            this.token = resp.Data.access_token;
                            this.GetRecipes();
                            this.GetProjects(this.token);
                        }
                        else
                        {
                            this.token = string.Empty;
                        }
                    });
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
                resp => this.GetTypesResponseProcess(resp, geometry, query, token, aoi, client, layerName, attempts));
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
                this.GetTypes(geometry, query, token, aoi, client, layerName, typeAttempts);
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
                    this.GetPagingId(geometry, type.name, query, token, aoi, client, layerName+"|"+type.name);
                }
            }
        }

        private void Instance_AoiHasBeenDrawn(IPolygon poly, IElement elm)
        {
            //unsubscribe from the event
            AnswerFactoryRelay.Instance.AoiHasBeenDrawn -= this.Instance_AoiHasBeenDrawn;

            this.drawnPolygon = poly;
            this.drawnElement = elm;

            // reset arcmap to whatever tool was selected prior to clicking the draw button 
            ArcMap.Application.CurrentTool = this.PreviouslySelectedItem;
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
                    this.GetPages(pageId, token, client, fileStreamWriter, layerName, pageAttempts);
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
                        fileStreamWriter.WriteLine(resp.Data.data.Replace("\r", "").Replace("\n", ""));
                        this.GetPages(resp.Data.next_paging_id, token, client, fileStreamWriter, layerName);
                    }
                    else
                    {
                        var fs = (FileStream)fileStreamWriter.BaseStream;
                        var filepath = fs.Name;
                        fileStreamWriter.Close();
                        this.ConvertPagesToFeatureClass(filepath, layerName);
                    }
                }
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
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
            var layername = string.Empty;
            var aoi = ConvertAoisToGeometryCollection(AoiList);
            if (resp.Data != null)
            {
                // since there can be multiple query ids its good to check to make sure we don't end up pulling duplicate results.
                HashSet<string> usedQueries = new HashSet<string>();
                foreach (var item in resp.Data)
                {
                    // if the recipe names don't match move on to the next
                    if (!recipeName.Equals(item.recipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    layername = string.Format("{0}|{1}", projectName, item.recipeName);

                    if (!string.IsNullOrEmpty(aoi) && !string.IsNullOrEmpty(layername))
                    {
                        if (usedQueries.Contains(item.properties.query_string))
                        {
                            continue;
                        }

                        usedQueries.Add(item.properties.query_string);
                        this.GetGeometries(item.properties.query_string, token, aoi, client, layername);
                    }
                }
            }
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
            this.GetRecipes(this.token, id);
        }

        private void projectSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.projectSearchTextBox.Text)
                || this.projectSearchTextBox.Text == "Type some text here to filter projects...")
            {
                this.projectDataView.RowFilter = "";
            }
            else
            {
                var filterStr = string.Format("[Project Name] LIKE '{0}*'", this.projectSearchTextBox.Text);
                this.projectDataView.RowFilter = filterStr;
            }
            this.projectNameDataGridView.Refresh();
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

        private void resultRefrshButton_Click(object sender, EventArgs e)
        {
            this.ProjIdRepo.Clear();
            this.RecipeRepo.Clear();
            this.GetProjects(this.token);
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
                if (this.selectionTypecomboBox.SelectedIndex == 1)
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

                this.ProjIdRepo.Clear();
                this.RecipeRepo.Clear();

                this.GetProjects(this.token);
            }
            catch (Exception error)
            {
                Jarvis.Logger.Error(error);
            }
        }

        public void SearchBoxGotFocus(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;

            if (tb.Text == "Type some text here to filter projects...")
            {
                tb.Text = string.Empty;
                tb.ForeColor = Color.Black;
            }
        }

        public void SearchBoxLostFocus(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Text == string.Empty)
            {
                tb.Text = "Type some text here to filter projects...";
                tb.ForeColor = Color.LightGray;
            }
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

        private void showResultsButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.projectNameDataGridView.SelectedRows.Count <= 0
                    || this.recipeStatusDataGridView.SelectedRows.Count <= 0)
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

        private void UpdateProjectsUi(List<ProjId> projects)
        {
            foreach (var item in projects)
            {
                var row = this.ProjIdRepo.NewRow();
                row["Project Name"] = item.name;
                row["Id"] = item.id;
                this.ProjIdRepo.Rows.Add(row);
            }
        }

        private void UpdateRecipeUiStatus(List<ResultItem> results)
        {
            foreach (var item in results)
            {
                var query = from row in this.RecipeRepo.AsEnumerable()
                            where (string)row["Recipe Name"] == item.recipeName
                            select row;
                foreach (var queryItem in query)
                {
                    queryItem["Status"] = item.status;
                }
            }

            this.recipeStatusDataGridView.Refresh();
            this.recipeStatusDataGridView.PerformLayout();
        }

        private void UpdateUiWithRecipes(List<Project2> results)
        {
            this.selectedAois.Clear();
            foreach (var item in results)
            {
                this.selectedAois.AddRange(item.aois);
                foreach (var recipe in item.recipeConfigs)
                {
                    var newRow = this.RecipeRepo.NewRow();
                    newRow["Recipe Name"] = recipe.recipeName;
                    newRow["Status"] = "Working";
                    this.RecipeRepo.Rows.Add(newRow);
                }
                this.GetRecipeStatus(this.token, item.id);
            }

            this.recipeStatusDataGridView.Refresh();
            this.recipeStatusDataGridView.PerformLayout();
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

        /// <summary>
        ///     Implementation class of the dockable window add-in. It is responsible for
        ///     creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : DockableWindow
        {
            #region Fields & Properties

            private AnswerFactoryDockableWindow m_windowUI;

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (this.m_windowUI != null)
                {
                    this.m_windowUI.Dispose(disposing);
                }

                base.Dispose(disposing);
            }

            protected override IntPtr OnCreateChild()
            {
                this.m_windowUI = new AnswerFactoryDockableWindow(this.Hook);
                return this.m_windowUI.Handle;
            }
        }
    }
}