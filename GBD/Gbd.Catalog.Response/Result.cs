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

using System.Collections.Generic;

namespace GBD
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The result.
    /// </summary>
    public class Result
    {
        public string identifier { get; set; }
        public List<string> type { get; set; }
        public Properties properties { get; set; }
    }
}