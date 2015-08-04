// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItsLogPassThroughProvider.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;

    using global::Hangfire.Logging;

    /// <summary>
    /// Log provider to register via: LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());
    /// </summary>
    public class ItsLogPassThroughProvider : ILogProvider
    {
        /// <inheritdoc />
        public ILog GetLogger(string name)
        {
            return new ItsLogger();
        }
    }

    /// <summary>
    /// Logger to write Hangfire messages to Its.Log for collecting.
    /// </summary>
    public class ItsLogger : ILog
    {
        /// <inheritdoc />
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            // add unique id in case both messages get logged and to differentiate
            var guid = Guid.NewGuid().ToString().ToUpper();

            if (messageFunc != null)
            {
                Its.Log.Instrumentation.Log.Write(messageFunc, "Hangfire logged message: " + guid);
            }

            if (exception != null)
            {
                Its.Log.Instrumentation.Log.Write(exception, "Hangfire logged message: " + guid);
            }

            return true;
        }
    }
}
