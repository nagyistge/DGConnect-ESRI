// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Result.cs" company="DigitalGlobe">
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
//   Defines the Result type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The result.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
    public class Result
    {
        /// <summary>
        /// Gets or sets the in edges.
        /// </summary>
        public InEdges inEdges { get; set; }

        /// <summary>
        /// Gets or sets the out edges.
        /// </summary>
        public OutEdges outEdges { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public Properties properties { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        public object owner { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string identifier { get; set; } 
    }
}