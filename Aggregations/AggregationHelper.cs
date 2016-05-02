namespace Aggregations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using GbdxTools;

    public class AggregationHelper
    {
        /// <summary>
        /// The base 32 codes.
        /// </summary>
        private const string Base32Codes = "0123456789bcdefghjkmnpqrstuvwxyz";

        /// <summary>
        /// The base 32 codes dictionary.
        /// </summary>
        private static Dictionary<char, int> base32CodesDict = Base32Codes.ToDictionary(
            chr => chr,
            chr => Base32Codes.IndexOf(chr));

        /// <summary>
        /// The process aggregations.
        /// </summary>
        /// <param name="aggs">
        /// The aggregations.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="geoHash">
        /// The geo hash.
        /// </param>
        /// <param name="isGeoHashLinked">
        /// The is geo hash linked.
        /// </param>
        public static void ProcessAggregations(List<Aggregation> aggs, int index, ref Dictionary<string, Dictionary<string, double>> output, string geoHash, bool isGeoHashLinked, ref Dictionary<string, string> fieldNames)
        {
            while (true)
            {
                bool isGeoHash = false;
                if (aggs == null)
                {
                    return;
                }

                // Check to see if the aggregation name contains geohash
                if (aggs[index].name.Contains("geohash"))
                {
                    // if it does then set isGeoHas true to signal that all terms under this
                    // aggregation are geohashes to be added as keys to the main dictionary.
                    isGeoHash = true;
                    ProcessTerms(aggs[index].terms, 0, ref output, isGeoHash, string.Empty, ref fieldNames);
                }
                else
                {
                    // if not a geohash check if terms are null if the terms are not null then process them
                    if (aggs[index].terms != null)
                    {
                        ProcessTerms(aggs[index].terms, 0, ref output, isGeoHash, geoHash, ref fieldNames);
                    }
                    else
                    {
                        // terms does equal null lets check to see if we have a single value aggregation on our hands
                        output[geoHash].Add(aggs[index].name, aggs[index].value);
                        var formattedValue = aggs[index].name;
                        formattedValue = CheckName(formattedValue);

                        if (!fieldNames.ContainsKey(aggs[index].name))
                        {
                            fieldNames.Add(aggs[index].name, formattedValue);
                        }
                    }
                }

                // Check to make sure that moving on to the next item will not be out of bounds.
                if (index + 1 <= aggs.Count - 1)
                {
                    // Move on to the next item.
                    index = index + 1;
                    isGeoHashLinked = false;
                    continue;
                }
                break;
            }
        }

        /// <summary>
        /// The process terms.
        /// </summary>
        /// <param name="terms">
        /// The terms.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="isGeoHashLinked">
        /// The is geo hash linked.
        /// </param>
        /// <param name="geoHash">
        /// The geo hash.
        /// </param>
        /// <param name="fieldNames">
        /// The field Names.
        /// </param>
        public static void ProcessTerms(List<Term> terms, int index, ref Dictionary<string, Dictionary<string, double>> output, bool isGeoHashLinked, string geoHash, ref Dictionary<string, string> fieldNames)
        {
            while (true)
            {
                // Check to see if this set of terms will contain geohash data
                if (isGeoHashLinked)
                {
                    if (terms.Count != 0)
                    {
                        try
                        {
                            // if containing geohash data create the dictionary relationship
                            if(!output.ContainsKey(terms[index].term))
                                output.Add(terms[index].term, new Dictionary<string, double>());
                            geoHash = terms[index].term;
                        }
                        catch (Exception error)
                        {
                            Jarvis.Logger.Error(error);
                        }
                    }
                }
                else
                {
                    // Check to see if the dictionary already contains the key if not then create it.
                    if (!output[geoHash].ContainsKey(terms[index].term))
                    {
                        try
                        {
                            output[geoHash].Add(terms[index].term, terms[index].count);


                            if (!fieldNames.ContainsKey(terms[index].term))
                            {
                                var formattedValue = terms[index].term;
                                formattedValue = CheckName(formattedValue);


                                // Make sure the field hasn't already been processed.
                                if (!fieldNames.ContainsKey(terms[index].term))
                                {
                                    fieldNames.Add(terms[index].term, formattedValue);
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            Jarvis.Logger.Error(error);
                        }
                    }
                    else
                    {
                        try
                        {
                            // If the dictionary already contains the key then add to the current count.
                            output[geoHash][terms[index].term] += terms[index].count;
                        }
                        catch (Exception error)
                        {
                            Jarvis.Logger.Error(error);
                        }
                    }
                }
                if (terms.Count == 0)
                {
                    return;
                }

                if (terms[index].aggregations != null)
                {
                    // There are still more nested aggregations to search through so get to it.
                    ProcessAggregations(terms[index].aggregations, 0, ref output, geoHash, false, ref fieldNames);
                }

                if (index + 1 <= terms.Count - 1)
                {
                    // Move on to the next term to process.
                    index = index + 1;
                    continue;
                }
                break;
            }
        }

        private static string CheckName(string name)
        {
            if (GbdxTools.Jarvis.invalidStartingChars.Contains(name[0])||string.Equals(name,"GeoHash", StringComparison.OrdinalIgnoreCase))
            {
                name = "t_" + name;
            }

            name = GbdxTools.Jarvis.InvalidFieldCharacters.Replace(name, "_");
            return "DG_"+ name;
        }
    }
} 