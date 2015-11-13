// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InEdges.cs" company="DigitalGlobe">
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
//   Defines the InEdges type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GBD
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The in edges.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed.")]
    public class InEdges
    {
        /// <summary>
        /// Gets or sets the _acquisition.
        /// </summary>
        public List<Acquisition> _acquisition { get; set; }

        /// <summary>
        /// Gets or sets the _includes.
        /// </summary>
        public List<Include> _includes { get; set; } 
    }
}