// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RssHitSource.cs" company="DigitalGlobe">
//   Copyright 2015 DigitalGlobe
//   
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//   
//          http://www.apache.org/licenses/LICENSE-2.0
//   
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// The rss hit source.
    /// </summary>
    public class RssHitSource
    {
        /// <summary>
        /// Gets or sets the posted time.
        /// </summary>
        [JsonProperty(PropertyName = "postedTime")]
        public string PostedTime { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the title negative.
        /// </summary>
        [JsonProperty(PropertyName = "titleNegative")]
        public double TitleNegative { get; set; }

        /// <summary>
        /// Gets or sets the description positive.
        /// </summary>
        [JsonProperty(PropertyName = "descriptionPositive")]
        public double DescriptionPositive { get; set; }

        /// <summary>
        /// Gets or sets the lucene score.
        /// </summary>
        [JsonProperty(PropertyName = "luceneScore")]
        public double LuceneScore { get; set; }

        /// <summary>
        /// Gets or sets the negative sentiment.
        /// </summary>
        [JsonProperty(PropertyName = "negativeSentiment")]
        public double NegativeSentiment { get; set; }

        /// <summary>
        /// Gets or sets the cc score.
        /// </summary>
        [JsonProperty(PropertyName = "ccScore")]
        public double CcScore { get; set; }

        /// <summary>
        /// Gets or sets the positive sentiment.
        /// </summary>
        [JsonProperty(PropertyName = "positiveSentiment")]
        public double PositiveSentiment { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the GAZ index id.
        /// </summary>
        [JsonProperty(PropertyName = "gazIndexId")]
        public string GazIndexId { get; set; }

        /// <summary>
        /// Gets or sets the NER score.
        /// </summary>
        [JsonProperty(PropertyName = "nerScore")]
        public double NerScore { get; set; }

        /// <summary>
        /// Gets or sets the sentence.
        /// </summary>
        [JsonProperty(PropertyName = "sentence")]
        public string Sentence { get; set; }

        /// <summary>
        /// Gets or sets the process version.
        /// </summary>
        [JsonProperty(PropertyName = "process_version")]
        public string ProcessVersion { get; set; }

        /// <summary>
        /// Gets or sets the article id.
        /// </summary>
        [JsonProperty(PropertyName = "articleId")]
        public string ArticleId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the location name.
        /// </summary>
        [JsonProperty(PropertyName = "locName")]
        public string LocName { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        [JsonProperty(PropertyName = "lanaguage")]
        public object Language { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the geo.
        /// </summary>
        [JsonProperty(PropertyName = "geo")]
        public HitPoint Geo { get; set; }

        /// <summary>
        /// Gets or sets the description negative.
        /// </summary>
        [JsonProperty(PropertyName = "descriptionNegative")]
        public double DescriptionNegative { get; set; }

        /// <summary>
        /// Gets or sets the province code.
        /// </summary>
        [JsonProperty(PropertyName = "provCode")]
        public string ProvCode { get; set; }

        /// <summary>
        /// Gets or sets the sentence number.
        /// </summary>
        [JsonProperty(PropertyName = "sentenceNum")]
        public int SentenceNum { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the GHB score.
        /// </summary>
        [JsonProperty(PropertyName = "ghbScore")]
        public double GhbScore { get; set; }

        /// <summary>
        /// Gets or sets the title positive.
        /// </summary>
        [JsonProperty(PropertyName = "titlePositive")]
        public double TitlePositive { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the location type.
        /// </summary>
        [JsonProperty(PropertyName = "locType")]
        public string LocType { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the screen name.
        /// </summary>
        [JsonProperty(PropertyName = "screenName")]
        public string ScreenName { get; set; }
    }
}