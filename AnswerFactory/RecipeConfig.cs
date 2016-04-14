namespace AnswerFactory
{
    using System;
    using System.Collections.Generic;

    public class RecipeConfig
    {
        public string recipeId { get; set; }
        public string recipeName { get; set; }
        public DateTime configurationDate { get; set; }
        public List<RecipeConfigParameter> parameters { get; set; }

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