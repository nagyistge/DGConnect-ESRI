namespace GBD
{
    public class Line
    {
        public OutEdges outEdges { get; set; }
        public InEdges inEdges { get; set; }
        public string salesLineItemNumber { get; set; }
        public Properties properties { get; set; }
        public object identifier { get; set; }
    }
}