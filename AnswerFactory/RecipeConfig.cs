namespace AnswerFactory
{
    using System;
    using System.Collections.Generic;

    public class RecipeConfig
    {
        public string recipeId { get; set; }
        public string recipeName { get; set; }
        public long configurationDate { get; set; }
        public List<RecipeConfigParameter> parameters { get; set; }

        public RecipeConfig()
        {
            this.parameters = new List<RecipeConfigParameter>();
            //var localTime = DateTime.UtcNow;
            //this.configurationDate = new DateTime(localTime.Year,localTime.Month,localTime.Day,localTime.Hour,localTime.Minute,localTime.Second,localTime.Millisecond);
        }

        public override string ToString()
        {
            return
                string.Format(
                    "RecipeConfig{recipeName='{0}', recipeId='{1}', configurationDate='{2}',parameters={3}}",
                    this.recipeName,
                    this.recipeId,
                    this.configurationDate.ToString(),
                    this.parameters);
        }
    }
}