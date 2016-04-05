namespace Aggregations
{
    using System.Collections.Generic;

    public class Geom
    {
        public List<List<List<List<double>>>> coordinates { get; set; }
        public string type { get; set; }
    }
}