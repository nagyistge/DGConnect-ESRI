// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerObject.cs" company="DigitalGlobe">
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
//   WorkerObject allows a complex set of parameters and results to be passed to and from different threads.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx.Vector_Index.Forms
{
    using System;
    using System.Collections.Generic;

    using Logging;

    using NetworkConnections;

    /// <summary>
    /// WorkerObject allows a complex set of parameters and results to be passed to and from different threads.
    /// </summary>
    public class WorkerObject
    {
        /// <summary>
        /// Gets or sets the number of attempts.
        /// </summary>
        public int NumberOfAttempts { get; set; }

        /// <summary>
        /// Gets or sets the number of lines.
        /// </summary>
        public int NumberOfLines { get; set; }

        /// <summary>
        /// Gets or sets the temporary file path.
        /// </summary>
        public string TemporaryFilePath { get; set; }

        /// <summary>
        /// Gets or sets the application state.
        /// </summary>
        public int ApplicationState { get; set; }

        /// <summary>
        /// Gets or sets the workspace path.
        /// </summary>
        public string WorkspacePath { get; set; }

        /// <summary>
        /// Gets or sets the base url.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the original paging id url.
        /// </summary>
        public string OriginalPagingIdUrl { get; set; }

        /// <summary>
        /// Gets or sets the bound box.
        /// </summary>
        public BoundingBox BoundBox { get; set; }

        /// <summary>
        /// Gets or sets the source node.
        /// </summary>
        public VectorIndexSourceNode SourceNode { get; set; }

        /// <summary>
        /// Gets or sets the geometry node.
        /// </summary>
        public VectorIndexGeometryNode GeometryNode { get; set; }

        /// <summary>
        /// Gets or sets the type node.
        /// </summary>
        public VectorIndexTypeNode TypeNode { get; set; }

        /// <summary>
        /// Gets or sets the geometry types.
        /// </summary>
        public List<SourceType> GeometryTypes { get; set; }

        /// <summary>
        /// Gets or sets the types.
        /// </summary>
        public List<SourceType> Types { get; set; }

        /// <summary>
        /// Gets or sets the network object.
        /// </summary>
        public NetObject NetworkObject { get; set; }

        /// <summary>
        /// Gets or sets the response object.
        /// </summary>
        public SourceTypeResponseObject ResponseObject { get; set; }

        /// <summary>
        /// Gets or sets the responses.
        /// </summary>
        public List<string> Responses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is unit test.
        /// </summary>
        public bool IsUnitTest { get; set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the combined JSON data.
        /// </summary>
        public string CombinedJsonData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether query source.
        /// </summary>
        public bool QuerySource { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public Logger Logger { get; set; }
    }
}