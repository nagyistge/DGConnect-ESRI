namespace AnswerFactory
{
    public class RecipeConfigParameter
    {
        public string name { get; set; } 
        public object value { get; set; }
        public string type { get; set; }

        public RecipeConfigParameter()
        {}

        public RecipeConfigParameter(string name, object value, string type)
        {
            this.name = name;
            this.value = value;
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format(
                "RecipeConfigParameter{name='{0}', value={1},type={2}}",
                this.name,
                this.value,
                this.type);
        }
    }
}