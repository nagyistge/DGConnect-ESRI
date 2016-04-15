namespace AnswerFactory
{
    using System.Collections.Generic;

    public class Project
    {
        public string id { get; set; } 
        //public string owner { get; set; }
        public string name { get; set; }
        public string accountId { get; set; }
        //public List<IdahoImage> idahoImages { get; set; }
        public List<object> aois { get; set; }
        public List<RecipeConfig> recipeConfigs { get; set; } 
        //public List<string> bestIdahoIds { get; set; } 
        //public List<string> originalGeometries { get; set; } 
        //public List<NamedBuffer> namedBuffers { get; set; }

        public Project()
        {
            this.id = null;
            //this.owner = string.Empty;
            this.name = string.Empty;
            this.accountId = null;
            //this.idahoImages = new List<IdahoImage>();
            this.aois = new List<object>();
            this.recipeConfigs = new List<RecipeConfig>();
            //this.bestIdahoIds = new List<string>();
            //this.originalGeometries = new List<string>();
            //this.namedBuffers = new List<NamedBuffer>();
        }

        //public override string ToString()
        //{
        //    return
        //        string.Format(
        //            "Project{id='{0}', owner='{1}', name='{2}', accountId='{3}', idahoImages={4}, originalGeometries={5}, namedBuffers={6}, aois={7}, recipeConfigs={8}, bestIdahoIds={9}}",
        //            this.id,
        //            this.owner,
        //            this.name,
        //            this.accountId,
        //            this.idahoImages,
        //            this.originalGeometries,
        //            this.namedBuffers,
        //            this.aois,
        //            this.recipeConfigs,
        //            this.bestIdahoIds);
        //}
    }
}