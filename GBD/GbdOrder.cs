namespace GBD
{
    using System.Collections.Generic;

    public class GbdOrder
    {
        public OutEdges outEdges { get; set; }
        public InEdges inEdges { get; set; }
        public string salesOrderNumber { get; set; }
        public List<Line> lines { get; set; }
        public object requestReferenceID { get; set; }
        public Properties properties { get; set; }
        public Header header { get; set; }
        public object identifier { get; set; }
    }
}