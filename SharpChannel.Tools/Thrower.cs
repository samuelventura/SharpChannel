using System;

namespace SharpChannel.Tools
{
    public static class Thrower
    {
        public static void Throw(string format, params object[] args)
        {
            var message = format;
            if (args.Length > 0)
            {
                message = string.Format(format, args);
            }
            throw new Exception(message);
        }

        public static void Throw(Exception inner, string format, params object[] args)
        {
            var message = format;
            if (args.Length > 0)
            {
                message = string.Format(format, args);
            }
            throw new Exception(message, inner);
        }
    }
}
