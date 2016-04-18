namespace AnswerFactory
{
    public class Properties
    {
        public string query_string { get; set; }
    }

    public class ResultItem
    {
        public string id { get; set; }
        public string projectId { get; set; }
        public string owner { get; set; }
        public string accountId { get; set; }
        public string recipeId { get; set; }
        public string recipeName { get; set; }
        public long runDate { get; set; }
        public string processingId { get; set; }
        public string resultType { get; set; }
        public string status { get; set; }
        public Properties properties { get; set; }
        public string displayAs { get; set; }
    }
}