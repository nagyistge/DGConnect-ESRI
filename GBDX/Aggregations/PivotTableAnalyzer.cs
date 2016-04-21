using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace Gbdx.Aggregations {
 public class PivotTableAnalyzer {

  private SendAnInt pbarValueUpdate;
  private UpdateAggWindowPbar pbarUpdate;

   public PivotTableAnalyzer(SendAnInt pbarValueUpdate,UpdateAggWindowPbar pbarUpdate) {
     this.pbarValueUpdate = pbarValueUpdate;
     this.pbarUpdate = pbarUpdate;
   }
    /// <summary>
    /// Calculates the difference between the values in entry a and b. The resulting distribution is normalized to a value between 0 and 1
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public Dictionary<String, Double> CalculateDiffs(PivotTableEntry a, PivotTableEntry b) {
      Dictionary<String, Double> diffs = new Dictionary<String, Double>();
      HashSet<string> keys = new HashSet<string>();
      foreach (string k in a.Data.Keys) {
        if (k != "OBJECTID") {
          keys.Add(k);
        }
      }
      foreach (string k in b.Data.Keys) {
        if (k != "OBJECTID") {
          keys.Add(k);
        }
      }
      foreach (string key in keys) {
        if (key == "OBJECTID") {
          continue;
        }
        double x = Double.MinValue;
        double y = Double.MinValue;
        if (a.Data.ContainsKey(key)) { x = a.Data[key]; }
        if (b.Data.ContainsKey(key)) { y = b.Data[key]; }
        //calc the simple percent diff
        double theDiff = Math.Abs((x - y) / (x + y));
        double percent = 0;
        double av = (x + y) / 2;
        double dif = Math.Abs(x - y);
        if (x != 0 && y != 0) {
          percent = Math.Abs((x - y) / (x + y));
        }
        if (percent > 0) {
          diffs.Add(key, Math.Round(percent, 2));
        }
        else {
          diffs.Add(key,0d);
        }
      }
      return diffs;
    }
    /// <summary>
    /// Returns a PivotTableAnalysisResult with an empty Data dictionary but with a prob 
    /// score set to the cosine similarity value of the two input vector spaces.
    /// Look here for more info:  https://upload.wikimedia.org/math/4/e/4/4e45dc7ae582130813e804f793f24ead.png
    ///</summary>
    ///
    /// <param name="a"></param>The base vectors
    /// <param name="b"></param>
    /// <param name="logarithm"></param>
    /// <param name="onlyBase"></param>
    /// <returns></returns>
    public PivotTableAnalysisResult GetSparseSimilarity(PivotTableEntry a, PivotTableEntry b, bool logarithm, bool onlyBase) {
      if (a == null || b == null) {
        throw new Exception("neither a nor b are allowed to be null");
      }
      PivotTableAnalysisResult prob = new PivotTableAnalysisResult();
      Double aSoS = 0d, bSoS = 0d, dotProd = 0d;
      HashSet<string> keys = new HashSet<string>();
      foreach (string k in a.Data.Keys) {
        if (k != "OBJECTID") {
          keys.Add(k);
        }
      }
      if (!onlyBase) {
        foreach (string k in b.Data.Keys) {
          if (k != "OBJECTID") {
            keys.Add(k);
          }
        }
      }
      foreach (string key in keys) {
        if (key == "OBJECTID") {
          continue;
        }
        double x = 0d;
        double y = 0d;
        if (a.Data.ContainsKey(key)) { x = a.Data[key]; }
        if (b.Data.ContainsKey(key)) { y = b.Data[key]; }
        if (logarithm) {
          x = Math.Log10(x + 1);
          y = Math.Log10(y + 1);
        }
        aSoS += x * x;
        bSoS += y * y;
        dotProd += x * y;
      }
      if (dotProd == 0) {
        return new PivotTableAnalysisResult() { prob = -1d, RowKey = b.RowKey, Context = b.Context };
      }
      double div = (Math.Sqrt(aSoS) * Math.Sqrt(bSoS));
      if (div == 0d) {
        return new PivotTableAnalysisResult() { prob = -1d, RowKey = b.RowKey, Context = b.Context };
      }
      Double similarity = dotProd / div;
      PivotTableAnalysisResult idprob = new PivotTableAnalysisResult() { prob = similarity, RowKey = b.RowKey, Context = b.Context };
      return idprob;
    }

    public PivotTable GenerateAverageVector(PivotTable tableWithMoreThanOneRow) {
      if (tableWithMoreThanOneRow.Count < 2) {
        throw new Exception("Must Have more than one row");
      }
      
      Dictionary<String, List<Double>> data = new Dictionary<String, List<Double>>();
      //consolidate all the values for each column of each entry in the pivot table
      foreach (PivotTableEntry entry in tableWithMoreThanOneRow) {
        foreach (String key in entry.Data.Keys) {
          if (data.ContainsKey(key)) {
            data[key].Add(entry.Data[key]);
          }
          else {
            List<Double> newList = new List<Double>();
            newList.Add(entry.Data[key]);
            data.Add(key,newList);
          }
        }
      }
      //now average each and produce a new data dictionary
      Dictionary<String, Double> averages = new Dictionary<String, Double>();
      foreach (String key in data.Keys) {
        double avg = 0d;
        foreach (Double val in data[key]) {
          avg += val;
        }
        avg = avg / data[key].Count;
        averages.Add(key, avg);
      }
      PivotTableEntry newEntry = new PivotTableEntry() {
        Data = averages, 
        Context = tableWithMoreThanOneRow[0].Context, 
        RowKey = tableWithMoreThanOneRow[0].RowKey 
      };

      PivotTable pt = new PivotTable();
      pt.Add(newEntry);
      return pt;
    }


    public PivotTable GetSparseSimilarites(PivotTableEntry baseVector, PivotTable vectors, bool logarithm, bool onlyBase) {
     
      this.pbarUpdate(vectors.Count, 0, 0);
      
      PivotTable outMap = new PivotTable();
      int i = 0;
      foreach (PivotTableEntry b in vectors) {
        PivotTableAnalysisResult similarity = GetSparseSimilarity(baseVector, b, logarithm, onlyBase);
        similarity.Data.Add("cos_sim", similarity.prob);
        Dictionary<String, double> diffData = CalculateDiffs(baseVector, b);
        foreach (String key in diffData.Keys) {
          if (!similarity.Data.ContainsKey(key)) {
            similarity.Data.Add(key, diffData[key]);
          }
        }
        outMap.Add(similarity);
        this.pbarValueUpdate(i);
        i++;
      }
      return outMap;
    }

    /// <summary>
    /// Compares two pivot tables. Do not pass in columns that don't make sense to compare. This method encapsulates a cosine similarity
    /// calculation on geohash cell pairs, and subsequently, each pair also calculates a diff between each col pair as a quasi percentage diff.
    /// 
    /// </summary>
    /// <param name="timeA"></param>
    /// <param name="timeB"></param>A PivotTable that is full
    /// <returns></returns>
    public PivotTable DetectChange(PivotTable ptA, PivotTable ptB, string label, bool diffs) {
     
   
      PivotTable outList = new PivotTable();
      //each dictionary below is a geohash agg layer, key=aGeoHashPrefix,value=anAggVectorOfThatBox
      Dictionary<string, PivotTableEntry> a = new Dictionary<string, PivotTableEntry>();
      Dictionary<string, PivotTableEntry> b = new Dictionary<string, PivotTableEntry>();
      HashSet<string> hashset = new HashSet<string>();
      //union the key sets into hashset variable
      foreach (PivotTableEntry av in ptA) {
        a.Add(av.RowKey, av);
        hashset.Add(av.RowKey);
      }
      foreach (PivotTableEntry av in ptB) {
        b.Add(av.RowKey, av);
        hashset.Add(av.RowKey);
      }

      this.pbarUpdate.Invoke(hashset.Count, 0, 0);
      //now hashset variable is a unique list of strings
      Dictionary<string,double> empty = new Dictionary<string, double>();
      foreach(String s in hashset){
        empty.Add(s, 0d);
      }
     int x =0;
      foreach (string geohash in hashset) {
        this.pbarValueUpdate.Invoke(x);
        x++;
        PivotTableEntry ava = null;
        PivotTableEntry avb = null;
        if (a.ContainsKey(geohash)) { ava = a[geohash]; }
        if (b.ContainsKey(geohash)) { avb = b[geohash]; }
        if (ava == null || avb == null) {
          outList.Add(new PivotTableAnalysisResult() { RowKey = geohash, prob = 0d, Data = empty, Label = label });
        }
        else {
          PivotTableAnalysisResult p = GetSparseSimilarity(ava, avb, true, false);
          p.RowKey = geohash;
          p.Label = label;
          if (diffs) {
          p.Data = CalculateDiffs(ava, avb);
          }
          else {
            p.Data = new Dictionary<string, double>();
          }
          p.Data.Add("cos_sim", p.prob);
          p.Data.Add("percent_change", Math.Abs(p.prob - 1) * 100);
          outList.Add(p);
        }
      }
      return outList;
    }

    public double stdev(List<Double> list) {
      double sum = 0.0;
      double mean = 0.0;
      double num = 0.0;
      double numi = 0.0;
      double deno = 0.0;
      for (int i = 0; i < list.Count; i++) {
        sum += list[i];
        mean = sum / list.Count - 1;
      }

      for (int i = 0; i < list.Count; i++) {
        numi = Math.Pow((list.Count - mean), 2);
        num += numi;
        deno = list.Count - 1;
      }

      double stdevResult = Math.Sqrt(num / deno);
      return stdevResult;
    }


    public double avg(List<Double> nums) {
      double av = 0d;
      for (int i = 0; i < nums.Count; i++) {
        av += nums[i];
      }
      return av / nums.Count;
    }

    public double max(List<Double> nums) {
      if (nums.Count == 0) {
        return 0d;
      }
      double max = Double.MinValue;
      foreach (Double d in nums) {
        if (d > max) {
          max = d;
        }
      }
      return max;
    }

    public double min(List<Double> nums) {
      if (nums.Count == 0) {
        return 0d;
      }
      double min = Double.MaxValue;
      foreach (Double d in nums) {
        if (d < min) {
          min = d;
        }
      }
      return min;
    }



    public PivotTable flattenAndSimplify(PivotTable withMultipleCompares, String pivCol) {
      PivotTable output = new PivotTable();
      Dictionary<String, Dictionary<String, Double>> outMap = new Dictionary<String, Dictionary<String, Double>>();

      foreach (PivotTableEntry pte in withMultipleCompares) {
        if (outMap.ContainsKey(pte.RowKey)) {
          if (pte.Data.ContainsKey(pivCol)) {
            outMap[pte.RowKey].Add(pte.Label, pte.Data[pivCol]);
          }
         else {
            outMap[pte.RowKey].Add(pte.Label, 0d);
         }
        }
        else {
          Dictionary<String, double> newMap = new Dictionary<string, double>();
          if (pte.Data.ContainsKey(pivCol)) {
            newMap.Add(pte.Label, pte.Data[pivCol]);
            outMap.Add(pte.RowKey, newMap);
          }
         // else {
         //   newMap.Add(pte.Label, 0d);
           // outMap.Add(pte.RowKey, newMap);
          //}
        }
      }
      foreach (String key in outMap.Keys) {
        output.Add(new PivotTableEntry() { RowKey = key, Data = outMap[key] });

      }

      return output;
    }
    //@wurfless :^)
    public Dictionary<String, Dictionary<String, Double>> GetSimilarityGraph(PivotTable signature) {
   
      Dictionary<String, Dictionary<String, Double>> outputGraph = new Dictionary<string, Dictionary<string, double>>();
      if (signature.Count < 2) { return outputGraph; }

      for (int i = 0; i < signature.Count; i++) {
        ///tricky... creates an acyclic undirected, non repeating graph
        for (int x = i +1; x < signature.Count; x++) {
          PivotTableEntry a = signature[i];
          PivotTableEntry b = signature[x];
          double sim = GetSparseSimilarity(a, b, true, false).prob;
          if (outputGraph.ContainsKey(a.RowKey)) {
            outputGraph[a.RowKey].Add(b.RowKey, sim);
          }
          else {
            Dictionary<String, double> newInnerMap = new Dictionary<string, double>();
            newInnerMap.Add(b.RowKey, sim);
            outputGraph.Add(a.RowKey, newInnerMap);
          }
        }
      }


      return outputGraph;
    }


   /// <summary>
   /// Converts a csv file to a pivot table.
   /// </summary>
   /// <param name="file"></param>
   /// <param name="rowkeyColumnName"></param>
   /// <returns></returns>
    public PivotTable FileToPivotTable(String file, string rowkeyColumnName) {
      string[] fields = null;
      PivotTable vectors = new PivotTable();
      using (var reader = System.IO.File.OpenText(file)) {
        String line = null;
        int i = 0;
        while ((line = reader.ReadLine()) != null) {
          if (i == 0) {
            fields = Regex.Split(line, ",");
            i++;
            continue;
          }
          Dictionary<string, double> vector = new Dictionary<string, double>();
          string[] vals = Regex.Split(line, ",");
          String geohash = "";
          for (int x = 0; x < fields.Length; x++) {
            //  Console.WriteLine(fields[x]);
            string key = fields[x];
            String valStr = vals[x];
            if (Regex.IsMatch(key, "(SHAPE_Length|SHAPE_Area)", RegexOptions.IgnoreCase)) { continue; }
            if (Regex.IsMatch(key, rowkeyColumnName, RegexOptions.IgnoreCase)) {
              geohash = vals[x];
              continue;
            }
            double val = Double.Parse(vals[x]);
            vector.Add(key, val);
          }
          vectors.Add(new PivotTableEntry() { Data = new Dictionary<string, double>(vector), RowKey = geohash });
          //Console.WriteLine(line);
          i++;
        }
      }
      return vectors;
    }
  }
}


