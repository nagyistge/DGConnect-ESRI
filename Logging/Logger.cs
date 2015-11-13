// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="DigitalGlobe">
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

namespace Logging
{
    using System;
    using System.IO;
    using System.Net;

    using NLog;
    using NLog.Config;
    using NLog.Targets;

    /// <summary>
    /// The logger.
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// The writer.
        /// </summary>
        private static NLog.Logger writer;

        /// <summary>
        /// The log config.
        /// </summary>
        private readonly LoggingConfiguration logConfig = new LoggingConfiguration();

        /// <summary>
        /// The filename.
        /// </summary>
        private readonly string filename;

        /// <summary>
        /// The using console.
        /// </summary>
        private bool usingConsole;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="useConsole">
        /// The use console.
        /// </param>
        public Logger(string file, bool useConsole)
        {
            this.filename = file;
            this.usingConsole = useConsole;
            var target = new FileTarget
            {
                AutoFlush = true,
                ConcurrentWriteAttemptDelay = 150,
                ConcurrentWriteAttempts = 5,
                ConcurrentWrites = false,
                EnableFileDelete = true,
                FileName = this.filename,
                CreateDirs = true,
                KeepFileOpen = false,
                Name = "f1",
                MaxArchiveFiles = 5,
                ArchiveAboveSize = 5000000,
                Layout = "${longdate}|${level:uppercase=true}|${message}"
            };

            this.logConfig.AddTarget("file", target);
            var loggerRule = new LoggingRule("*", LogLevel.Info, target);
            this.logConfig.LoggingRules.Add(loggerRule);
            LogManager.Configuration = this.logConfig;
            writer = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// The warning.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Warning(string message)
        {
            writer.Warn(message);
        }

        /// <summary>
        /// The info.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Info(string message)
        {
            writer.Info(message);
        }

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Error(string message)
        {
            writer.Error(message);
        }

        /// <summary>
        /// The debug.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Debug(string message)
        {
            writer.Debug(message);
        }

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public void Error(WebException exception)
        {
            if(exception.Response != null)
            {
                using (var stream = exception.Response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        string output;
                        using (var streamReader = new StreamReader(stream))
                        {
                            output = streamReader.ReadToEnd();
                        }

                        writer.Error(exception.Message + "\n====Web Error Start====\n"+output +"\n====Web Error End====\n");
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    writer.Error("WebException: " + exception.Message);
                }
                else
                {
                    writer.Error("A webexeption has occurred. ");
                }
            }
        }

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="exception">
        /// The exception.
        /// </param>
        public void Error(Exception exception)
        {
            writer.Error(exception.Message + "\n====StackTrace Start====\n" + exception.StackTrace + "\n====StackTrace End====\n");
        }
    }
}
