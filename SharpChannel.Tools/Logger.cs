using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace SharpChannel.Tools
{
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        SUCCESS
    }

    public interface ILogger
    {
        void Log(LogLevel level, string format, params object[] args);

        void Debug(string format, params object[] args);

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Error(string format, params object[] args);

        void Success(string format, params object[] args);
    }

    public interface ILogAppender
    {
        void Append(LogLevel level, string line);
    }

    public interface ILogFormatter
    {
        string Format(LogLevel level, DateTime dt, Thread thread, string line);
    }

    public class Logger : ILogger, IDisposable
    {
        private readonly List<Action<LogLevel, string>> appenders;
        private readonly List<Action<LogLevel, string>> removes;
        private readonly ILogFormatter formatter;
        private readonly ThreadRunner runner;

        public Logger(ILogFormatter formatter = null)
        {
            this.appenders = new List<Action<LogLevel, string>>();
            this.removes = new List<Action<LogLevel, string>>();
            if (formatter == null)
                formatter = PatternLogFormatter.TIMEONLY_LINE;
            this.formatter = formatter;
            //expected only one per application
            this.runner = new ThreadRunner("Logger");
        }

        public void Dispose()
        {
            runner.Dispose(appenders.Clear);
        }

        public void AddAppender(ILogAppender appender)
        {
            runner.Run(() => appenders.Add(appender.Append));
        }

        public void AddAppender(Action<LogLevel, string> appender)
        {
            runner.Run(() => appenders.Add(appender));
        }

        public void Debug(string format, params object[] args)
        {
            Log(LogLevel.DEBUG, format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log(LogLevel.INFO, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log(LogLevel.WARN, format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log(LogLevel.ERROR, format, args);
        }

        public void Success(string format, params object[] args)
        {
            Log(LogLevel.SUCCESS, format, args);
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            //ignore any format {N} placeholders if no params are provided
            var data = args.Length == 0 ? format : string.Format(format, args);
            var line = formatter.Format(level, DateTime.Now, Thread.CurrentThread, data);
            runner.Run(() => {
                foreach (var appender in appenders)
                {
                   Execute(() => appender(level, line), (Exception ex) => removes.Add(appender));
                }
                //unable to auto dispose actions 
                //autoremove excepting appenders
                foreach (var appender in removes)
                {
                    appenders.Remove(appender);
                }
                removes.Clear();
            });
        }

        public void Flush()
        {
            runner.Flush();
        }

        private void Execute(Action action, Action<Exception> handler)
        {
            try { action(); } catch (Exception ex) { handler(ex);  }
        }
    }

    public class PatternLogFormatter : ILogFormatter
    {
        public readonly static ILogFormatter LINE = new PatternLogFormatter("{LINE}");
        public readonly static ILogFormatter TIMEONLY_LINE = new PatternLogFormatter("{TIMESTAMP:HH:mm:ss.fff} {LINE}");
        public readonly static ILogFormatter TIMESTAMP_LINE = new PatternLogFormatter("{TIMESTAMP:yyyy-MM-dd HH:mm:ss.fff} {LINE}");
        public readonly static ILogFormatter TIMESTAMP_LEVEL_LINE = new PatternLogFormatter("{TIMESTAMP:yyyy-MM-dd HH:mm:ss.fff} {LEVEL} {LINE}");
        public readonly static ILogFormatter TIMESTAMP_LEVEL_THREAD_LINE = new PatternLogFormatter("{TIMESTAMP:yyyy-MM-dd HH:mm:ss.fff} {LEVEL} {THREAD} {LINE}");

        private readonly string format;

        public PatternLogFormatter(string pattern)
        {
            pattern = pattern.Replace("TIMESTAMP", "0");
            pattern = pattern.Replace("LEVEL", "1");
            pattern = pattern.Replace("THREAD", "2");
            pattern = pattern.Replace("LINE", "3");
            this.format = pattern;
        }

        public string Format(LogLevel level, DateTime dt, Thread thread, string line)
        {
            var args = new object[] { dt, level, thread.Name, line };
            return string.Format(format, args);
        }
    }

    public class ConsoleLogAppender : ILogAppender
    {
        public void Append(LogLevel level, string line)
        {
            //console appenders honor debug by default
            Console.WriteLine(line);
        }
    }

    public class WriterLogAppender : ILogAppender, IDisposable
    {
        private readonly TextWriter writer;

        public WriterLogAppender(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            this.writer = new StreamWriter(filePath, true);
        }

        public WriterLogAppender(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Append(LogLevel level, string line)
        {
            //file appenders ignore debug by default
            if (level != LogLevel.DEBUG)
            {
                writer.WriteLine(line);
                writer.Flush();
            }
        }

        public void Dispose()
        {
            Disposer.Dispose(writer);
        }
    }
}