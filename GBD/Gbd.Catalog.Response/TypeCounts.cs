// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeCounts.cs" company="DigitalGlobe">
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
//   Defines the TypeCounts type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.Contracts;
using RestSharp.Deserializers;
// ReSharper disable InconsistentNaming

namespace GBD
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The type counts.
    /// </summary>
    public class TypeCounts
    {
        public int IDAHOImage { get; set; }
        public int WV03_SWIR { get; set; }
        public int WV02 { get; set; }
        public int WV01 { get; set; }
        public int WV03_VNIR { get; set; }
        public int GBDXCatalogRecord { get; set; }
        public int GE01 { get; set; }
        [DeserializeAs(Name="1BProduct")]
        public int OneBProduct { get; set; }
        public int DigitalGlobeProduct { get; set; }
        public int QB02 { get; set; }
    }
}