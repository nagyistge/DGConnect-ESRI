// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogger.cs" company="DigitalGlobe">
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
//   The Logger interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Logging
{
    using System;
    using System.Net;

    /// <summary>
    /// The Logger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes warning messages to the log file.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Warning(string message);

        /// <summary>
        /// Writes information messages to the log file.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Info(string message);

        /// <summary>
        /// Writes debug messages to the log file
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Debug(string message);

        /// <summary>
        /// Writes web exception information to the log file.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Error(WebException exception);

        /// <summary>
        /// Writes exception information to the log file.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Error(Exception exception);

        /// <summary>
        /// Writes a general error message to the log file.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Error(string message);
    }
}