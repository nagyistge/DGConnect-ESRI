using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gbdx.Aggregations {


    /// <summary>
    /// Holds a row from a pivot table, which abstractly always has a key, and a bunch of 
    /// columns with numeric values that represent things like sums or counts or averages
    /// </summary>
    public class PivotTableEntry {
      public PivotTableEntry() {
        Data = new Dictionary<string, double>();
      }
      /// <summary>
      /// The rowkey to the pivot table row/entry. Example is a geoHash, possibly a geohash-time box id of some kind
      /// </summary>
      public string RowKey { set; get; }
      /// <summary>
      /// Holds all the fields, and their value as a double. "looks and feels" similar to a labeled, but real-valued, vector space.
      /// </summary>
      public Dictionary<string, double> Data { set; get; }
      /// <summary>,
      /// Anything you might need in relation to this row
      /// </summary>
      public Object Context { set; get; }
    }
  

}
