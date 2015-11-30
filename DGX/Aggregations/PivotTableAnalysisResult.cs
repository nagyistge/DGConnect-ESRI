using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dgx.Aggregations {
  public class PivotTableAnalysisResult : PivotTableEntry {
    public PivotTableAnalysisResult() {
      prob = -1d;
    }
    /// <summary>
    /// A field to store a probability value
    /// </summary>
    public double prob { get; set; }
  }
}
