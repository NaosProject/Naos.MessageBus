// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItsLogPassThroughProvider.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if NaosMessageBusHangfireConsole
namespace Naos.MessageBus.Hangfire.Console
#else
namespace Naos.MessageBus.Hangfire.Bootstrapper
#endif
{
    using System;

    using global::Hangfire.Logging;

    /// <summary>
    /// Log provider to register via: LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());
    /// </summary>
    public class ItsLogPassThroughProvider : ILogProvider
    {
#pragma warning disable CS3002 // Return type is not CLS-compliant - needed for Hangfire
        /// <inheritdoc />
        public ILog GetLogger(string name)
#pragma warning restore CS3002 // Return type is not CLS-compliant
        {
            return new ItsLogger();
        }
    }

    /// <summary>
    /// Logger to write Hangfire messages to Its.Log for collecting.
    /// </summary>
    public class ItsLogger : ILog
    {
#pragma warning disable CS3001 // Argument type is not CLS-compliant - needed for Hangfire
        /// <inheritdoc />
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            // add unique id in case both messages get logged and to differentiate
            var guid = Guid.NewGuid().ToString().ToUpperInvariant();

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
