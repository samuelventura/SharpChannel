using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

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

    public abstract class AbstractLogger : ILogger
    {
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

        public abstract void Log(LogLevel level, string format, params object[] args);
    }

    public class Logger : AbstractLogger, IDisposable
    {
        private readonly List<ILogAppender> appenders;
        private readonly List<ILogAppender> removes;
        private readonly ILogFormatter formatter;
        private readonly ThreadRunner runner;

        public Logger(ILogFormatter formatter = null)
        {
            this.appenders = new List<ILogAppender>();
            this.removes = new List<ILogAppender>();
            this.formatter = formatter ?? PatternLogFormatter.TIMEONLY_LINE;
            this.runner = new ThreadRunner(typeof(Logger).Name);
        }

        public void Dispose()
        {
            runner.Dispose(() => {
                foreach (var appender in appenders)
                {
                    Disposer.Dispose(appender);
                }
                appenders.Clear();
            });
        }

        public void AddAppender(ILogAppender appender)
        {
            runner.Run(() => appenders.Add(appender));
        }

        public override void Log(LogLevel level, string format, params object[] args)
        {
            var data = args.Length > 0 ? string.Format(format, args) : format;
            var line = formatter.Format(level, DateTime.Now, Thread.CurrentThread, data);
            runner.Run(() => {
                foreach (var appender in appenders)
                {
                    Execute(() => appender.Append(level, line), (Exception ex) => removes.Add(appender));
                }
                foreach (var appender in removes)
                {
                    Disposer.Dispose(appender);
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
            try { action(); } catch (Exception ex) { handler(ex); }
        }
    }

    public class PatternLogFormatter : ILogFormatter
    {
        public readonly static ILogFormatter LINE = new PatternLogFormatter("{LINE}");
        public readonly static ILogFormatter TIMEONLY_LINE = new PatternLogFormatter("{TIMESTAMP:HH:mm:ss.fff} {LINE}");
        public readonly static ILogFormatter TIMEONLY_LEVEL_LINE = new PatternLogFormatter("{TIMESTAMP:HH:mm:ss.fff} {LEVEL} {LINE}");
        public readonly static ILogFormatter TIMEONLY_LEVEL_THREAD_LINE = new PatternLogFormatter("{TIMESTAMP:HH:mm:ss.fff} {LEVEL} {THREAD} {LINE}");
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

    public class NopLogger : AbstractLogger
    {
        public override void Log(LogLevel level, string format, params object[] args) { }
    }

    public class TraceLogger : AbstractLogger
    {
        public override void Log(LogLevel level, string format, params object[] args)
        {
            var data = args.Length > 0 ? string.Format(format, args) : format;
            var text = string.Format("{0} {1} {2}", DateTime.Now.ToString("HH:mm:ss.fff"), level, data);
            Trace.WriteLine(text);
            Console.WriteLine(text);
        }
    }
}