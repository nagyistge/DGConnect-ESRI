namespace AnswerFactory
{
    using System.Collections.Generic;

    public class Aois
    {
        public string type { get; set; }
        public List<List<List<List<double>>>> coordinates { get; set; }
    }

    public class Project2
    {
        public string id { get; set; }
        public string owner { get; set; }
        public string name { get; set; }
        public string accountId { get; set; }
        public object idahoImages { get; set; }
        public List<string> aois { get; set; }
        public List<RecipeConfig> recipeConfigs { get; set; }
        public List<object> bestIdahoIds { get; set; }
        public object originalGeometries { get; set; }
        public object namedBuffers { get; set; }
        public long createDate { get; set; }
        public long updateDate { get; set; }
    }
}