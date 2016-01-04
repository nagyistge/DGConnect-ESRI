using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gbdx.Aggregations {
  public static class PivotTableCache {
     static PivotTableCache() {
      PivotTableCache.Cache = new Dictionary<string, PivotTable>();
    }
    public static Dictionary<String, PivotTable> Cache { set; get; }
  }
}
