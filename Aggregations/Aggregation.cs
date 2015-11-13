namespace Aggregations
{
    using System.Collections.Generic;

    public class Aggregation
    {
        public string name { get; set; }
        public List<Term> terms { get; set; }
        public double value { get; set; }
    }
}