// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetworkResponse.cs" company="DigitalGlobe">
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
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetworkConnections
{
    using System;

    /// <summary>
    /// The network response.
    /// </summary>
    public class NetworkResponse
    {
        /// <summary>
        /// result of the query
        /// </summary>
        private string localResult;

        /// <summary>
        /// If an error occurred.
        /// </summary>
        private bool localError;

        /// <summary>
        /// The exception if one happened.
        /// </summary>
        private Exception localExceptionIncurred;

        /// <summary>
        /// The number of allowed timeouts
        /// </summary>
        private int localTimeouts;

        /// <summary>
        /// value if the AOI is a circle
        /// </summary>
        private bool localIsCircle = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkResponse"/> class.
        /// </summary>
        public NetworkResponse()
        {
            this.localResult = string.Empty;
            this.localError = false;
            this.localExceptionIncurred = null;
            this.localTimeouts = 0;
        }

        /// <summary>
        /// Gets or sets the results of the query.
        /// </summary>
        public string Result
        {
            get
            {
                return this.localResult;
            }

            set
            {
                this.localResult = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether error.
        /// </summary>
        public bool Error
        {
            get
            {
                return this.localError;
            }

            set
            {
                this.localError = value;
            }
        }

        /// <summary>
        /// Gets or sets an exception if one occurred. 
        /// </summary>
        public Exception ExceptionIncurred
        {
            get
            {
                return this.localExceptionIncurred;
            }

            set
            {
                this.localExceptionIncurred = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of allowed timeouts.
        /// </summary>
        public int Timeouts
        {
            get
            {
                return this.localTimeouts;
            }

            set
            {
                this.localTimeouts = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is circle.
        /// </summary>
        public bool IsCircle
        {
            get
            {
                return this.localIsCircle;
            }

            set
            {
                this.localIsCircle = value;
            }
        }
    }
}
