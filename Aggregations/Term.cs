namespace Aggregations
{
    using System.Collections.Generic;

    public class Term
    {
        public string term { get; set; }
        public int count { get; set; }
        public List<Aggregation> aggregations { get; set; }
    }
}