// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HitSource.cs" company="DigitalGlobe">
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
// <summary>
//   The hit source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx
{
    using Newtonsoft.Json;

    /// <summary>
    /// The hit source.
    /// </summary>
    public class HitSource
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the screen name.
        /// </summary>
        [JsonProperty(PropertyName = "screenName")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets the location hash.
        /// </summary>
        [JsonProperty(PropertyName = "loc_hash")]
        public string LocHash { get; set; }

        /// <summary>
        /// Gets or sets the re tweets.
        /// </summary>
        [JsonProperty(PropertyName = "retweets")]
        public object Retweets { get; set; }

        /// <summary>
        /// Gets or sets the GNIP language.
        /// </summary>
        [JsonProperty(PropertyName = "gnipLanguage")]
        public string GnipLanguage { get; set; }

        /// <summary>
        /// Gets or sets the friends count.
        /// </summary>
        [JsonProperty(PropertyName = "friendsCount")]
        public int FriendsCount { get; set; }

        /// <summary>
        /// Gets or sets the actor description.
        /// </summary>
        [JsonProperty(PropertyName = "actorDescription")]
        public object ActorDescription { get; set; }

        /// <summary>
        /// Gets or sets the followers count.
        /// </summary>
        [JsonProperty(PropertyName = "followersCount")]
        public int FollowersCount { get; set; }

        /// <summary>
        /// Gets or sets the negative sentiment.
        /// </summary>
        [JsonProperty(PropertyName = "negativeSentiment")]
        public double NegativeSentiment { get; set; }

        /// <summary>
        /// Gets or sets the favorites count.
        /// </summary>
        [JsonProperty(PropertyName = "favoritesCount")]
        public int FavoritesCount { get; set; }

        /// <summary>
        /// Gets or sets the positive sentiment.
        /// </summary>
        [JsonProperty(PropertyName = "positiveSentiment")]
        public double PositiveSentiment { get; set; }

        /// <summary>
        /// Gets or sets the user mentions.
        /// </summary>
        [JsonProperty(PropertyName = "userMentions")]
        public object[] UserMentions { get; set; }

        /// <summary>
        /// Gets or sets the twitter country code.
        /// </summary>
        [JsonProperty(PropertyName = "twitterCountryCode")]
        public string TwitterCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the twitter language.
        /// </summary>
        [JsonProperty(PropertyName = "twitterLanguage")]
        public object TwitterLanguage { get; set; }

        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        [JsonProperty(PropertyName = "device")]
        public string Device { get; set; }

        /// <summary>
        /// Gets or sets the created.
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public string Created { get; set; }

        /// <summary>
        /// Gets or sets the geo.
        /// </summary>
        [JsonProperty(PropertyName = "geo")]
        public HitPoint Geo { get; set; }

        /// <summary>
        /// Gets or sets the actor display name.
        /// </summary>
        [JsonProperty(PropertyName = "actorDisplayName")]
        public string ActorDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the listed count.
        /// </summary>
        [JsonProperty(PropertyName = "listedCount")]
        public int ListedCount { get; set; }

        /// <summary>
        /// Gets or sets the exact geo.
        /// </summary>
        [JsonProperty(PropertyName = "exactGeo")]
        public string ExactGeo { get; set; }

        /// <summary>
        /// Gets or sets the verb.
        /// </summary>
        [JsonProperty(PropertyName = "verb")]
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the statuses count.
        /// </summary>
        [JsonProperty(PropertyName = "statusesCount")]
        public int StatusesCount { get; set; }

        /// <summary>
        /// Gets or sets the languages.
        /// </summary>
        [JsonProperty(PropertyName = "languages")]
        public string[] Languages { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the actor id.
        /// </summary>
        [JsonProperty(PropertyName = "actorId")]
        public string ActorId { get; set; }

        /// <summary>
        /// Gets or sets the place geo type.
        /// </summary>
        [JsonProperty(PropertyName = "placeGeoType")]
        public string PlaceGeoType { get; set; }
    }
}
