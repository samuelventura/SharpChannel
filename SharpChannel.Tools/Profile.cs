using System;
using System.Diagnostics;

namespace SharpChannel.Tools
{
    public class Profile
    {
        private static bool DEBUG;

        [Conditional("DEBUG")]
        public static void SetDebug()
        {
            DEBUG = true;
        }

        public static bool IsDebug()
        {
            SetDebug();
            return DEBUG;
        }

        public static void Trace(string format, params object[] args)
        {
            if (!IsDebug()) return;
            if (args.Length > 0) format = string.Format(format, args);
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Console.WriteLine("{0} TRACE {1}", now, format);
        }
    }
}
