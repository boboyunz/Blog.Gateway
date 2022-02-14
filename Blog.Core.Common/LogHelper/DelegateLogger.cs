using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Blog.Core.Common.Helper;
using Microsoft.Extensions.Hosting;

namespace Blog.Core.Common.LogHelper
{
    [ProviderAlias("Delegate")]
    public sealed class DelegateLoggerProvider : ILoggerProvider
    {
        private readonly Action<string, LogLevel, Exception, string> _logAction;
        private readonly ConcurrentDictionary<string, DelegateLogger> _loggers = new();

        public DelegateLoggerProvider(Action<string, LogLevel, Exception, string> logAction)
        {
            _logAction = logAction;
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, category => new DelegateLogger(category, _logAction));
        }

        private class DelegateLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly Action<string, LogLevel, Exception, string> _logAction;

            public DelegateLogger(string categoryName, Action<string, LogLevel, Exception, string> logAction)
            {
                _categoryName = categoryName;
                _logAction = logAction;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (null != _logAction)
                {
                    var msg = formatter(state, exception);
                    _logAction.Invoke(_categoryName, logLevel, exception, msg);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return NullScope.Instance;
            }
        }
    }
    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; }

        public void Dispose()
        { 
            
        }
    }
    public static class LoggerExtensions
    {
        #region Info

        public static void Info(this ILogger logger, string msg, params object[] parameters) => logger.LogInformation(msg, parameters);

        public static void Info(this ILogger logger, Exception ex, string msg) => logger.LogInformation(ex, msg);

        #endregion Info

        #region Trace

        public static void Trace(this ILogger logger, string msg, params object[] parameters) => logger.LogTrace(msg, parameters);

        public static void Trace(this ILogger logger, Exception ex, string msg) => logger.LogTrace(ex, msg);

        public static void Trace(this ILogger logger, Exception ex) => logger.LogTrace(ex, ex?.Message);

        #endregion Trace

        #region Debug

        public static void Debug(this ILogger logger, string msg, params object[] parameters) => logger.LogDebug(msg, parameters);

        public static void Debug(this ILogger logger, Exception ex, string msg) => logger.LogDebug(ex, msg);

        public static void Debug(this ILogger logger, Exception ex) => logger.LogDebug(ex, ex?.Message);

        #endregion Debug

        #region Warn

        public static void Warn(this ILogger logger, string msg, params object[] parameters) => logger.LogWarning(msg, parameters);

        public static void Warn(this ILogger logger, Exception ex, string msg) => logger.LogWarning(ex, msg);

        public static void Warn(this ILogger logger, Exception ex) => logger.LogWarning(ex, ex?.Message);

        #endregion Warn

        #region Error

        public static void Error(this ILogger logger, string msg, params object[] parameters) => logger.LogError(msg, parameters);

        public static void Error(this ILogger logger, Exception ex, string msg) => logger.LogError(ex, msg);

        public static void Error(this ILogger logger, Exception ex) => logger.LogError(ex, ex?.Message);

        #endregion Error

        #region Fatal

        public static void Fatal(this ILogger logger, string msg, params object[] parameters) => logger.LogCritical(msg, parameters);

        public static void Fatal(this ILogger logger, Exception ex, string msg) => logger.LogCritical(ex, msg);

        public static void Fatal(this ILogger logger, Exception ex) => logger.LogCritical(ex, ex?.Message);

        #endregion Fatal

        #region LoggerFactory

        /// <summary>
        /// 自定义的系统日志组件
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public static ILoggerFactory AddDelegateLoggerCom(this ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new DelegateLoggerProvider((category, logLevel, exception, msg) => {
                if (false && Appsettings.Env.IsDevelopment())
                    Console.WriteLine($"{category}:[{logLevel}] {msg}\n{exception}");
                else
                {
                    if (category == "Microsoft.Hosting.Lifetime")
                    {
                        Console.WriteLine($"info--:{msg}\n{exception}");
                    }
                    InfluxdbHelper.GetInstance().AddLog(new SysComLog
                    {
                        Ltype = 9,
                        LsubType = 1,
                        SysID = Appsettings.Env.ApplicationName.Cof_FilterWord(),
                        LogTime = DateTime.Now,
                        KeyShow = $"{logLevel}-{category}",
                        LogDataShow = $"{msg}\n{exception}"
                    });
                }
            }));
            return loggerFactory;
        }

        /// <summary>
        /// AddDelegateLoggerProvider
        /// </summary>
        /// <param name="loggerFactory">loggerFactory</param>
        /// <param name="logAction">logAction</param>
        /// <returns>loggerFactory</returns>
        public static ILoggerFactory AddDelegateLogger(this ILoggerFactory loggerFactory, Action<string, LogLevel, Exception, string> logAction)
        {
            loggerFactory.AddProvider(new DelegateLoggerProvider(logAction));
            return loggerFactory;
        }

        #endregion LoggerFactory

        #region ILoggingBuilder

        public static ILoggingBuilder AddDelegateLogger(this ILoggingBuilder loggingBuilder,
            Action<string, LogLevel, Exception, string> logAction)
        {
            return loggingBuilder.AddProvider(new DelegateLoggerProvider(logAction));
        }

        //public static ILoggingBuilder UseCustomGenericLogger(this ILoggingBuilder loggingBuilder, Action<GenericLoggerOptions> genericLoggerConfig)
        //{
        //    Guard.NotNull(loggingBuilder, nameof(loggingBuilder));
        //    Guard.NotNull(genericLoggerConfig, nameof(genericLoggerConfig));
        //    loggingBuilder.Services.Configure(genericLoggerConfig);
        //    loggingBuilder.Services.AddSingleton(typeof(ILogger<>), typeof(GenericLogger<>));
        //    return loggingBuilder;
        //}

        #endregion ILoggingBuilder
    }
}
