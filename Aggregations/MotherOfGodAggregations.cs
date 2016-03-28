namespace Aggregations
{
    using System.Collections.Generic;

    public class MotherOfGodAggregations
    {
        public string responseDate { get; set; }
        public Geom geom { get; set; }
        public object query { get; set; }
        public object startDate { get; set; }
        public object endDate { get; set; }
        public int totalItems { get; set; }
        public List<Aggregation> aggregations { get; set; }
    }
}