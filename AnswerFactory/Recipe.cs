namespace AnswerFactory
{
    public class Recipe
    {
        public string id { get; set; }
        public string name { get; set; }
        public string owner { get; set; }
        public string accountId { get; set; }
        public string access { get; set; }
        public string description { get; set; }
        public string recipeType { get; set; }
        public string inputType { get; set; }
        public string outputType { get; set; }
        public object parameters { get; set; }
        public string validators { get; set; }
        public string definition { get; set; }
    }
}