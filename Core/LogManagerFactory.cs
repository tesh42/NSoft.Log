using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NSoft.Log.Core.Exceptions;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides a set of methods for creating log managers.
    /// </summary>
    public class LogManagerFactory
    {
        /// <summary>
        /// Creates log manager based on the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ILogManager Create(LogManagerConfiguration configuration)
        {
            var logManager = new LogManager();
            var logWriters = CreateLogWriters(configuration.LogWriters.LogWriters).ToDictionary(obj => obj.Id);
            CreateCategories(logManager, configuration, logWriters);
            return logManager;
        }

        static ILogWriter CreateLogWriter(int id, string writerTypeName, string configurationTypeName, XmlElement configuration)
        {
            try
            {
                ILogWriter writer;
                var writerType = Type.GetType(writerTypeName);
                if (configurationTypeName != null)
                {
                    var configurationType = Type.GetType(configurationTypeName);
                    var config = ConfigurationHelper.Load(configurationType, "configuration", configuration.OuterXml);
                    writer = (ILogWriter)Activator.CreateInstance(writerType, id, config);
                }
                else
                {
                    writer = (ILogWriter)Activator.CreateInstance(writerType, id);
                }
                return writer;
            }
            catch (Exception ex)
            {
                throw new InitializationException(string.Format("Can't create writer {0}.", id), ex);
            }
        }

        static IEnumerable<ILogWriter> CreateLogWriters(IEnumerable<LogWriterConfiguration> configuration)
        {
            return configuration.Select(writerInfo => CreateLogWriter(writerInfo.Id, writerInfo.Type, writerInfo.ConfigurationType, writerInfo.Configuration)).ToList();
        }

        static void CreateCategories(ILogManagerConfigurator configurator, LogManagerConfiguration configuration, Dictionary<int, ILogWriter> logWriters)
        {
            foreach (var categoryInfo in configuration.Categories)
            {
                configurator.CreateCategory(categoryInfo.Id, configuration.LogWriters.SwitchTime);
                foreach (var channel in categoryInfo.Channels)
                    configurator.BindChannel(categoryInfo.Id, channel.Name);
                foreach (var logWriterInfo in categoryInfo.LogWriters)
                {
                    ILogWriter logWriter;
                    if (!logWriters.TryGetValue(logWriterInfo.Id, out logWriter))
                        throw new InitializationException("Log writer with Id = {0} is not found.", logWriterInfo.Id);
                    configurator.BindWriter(categoryInfo.Id, logWriter, logWriterInfo.Priority);
                }
            }
        }
    }
}
