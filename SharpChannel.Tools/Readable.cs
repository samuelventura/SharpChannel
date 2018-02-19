using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpChannel.Tools
{
    public static class Readable
    {
        public static string Make(string response)
        {
            var sb = new StringBuilder();
            foreach (var c in response)
                sb.Append(Make(c));
            return sb.ToString();
        }

        public static string Make(char c)
        {
            if (Char.IsControl(c) || Char.IsWhiteSpace(c))
                return string.Format("[{0:X2}]", (int)c);
            return c.ToString();
        }

        public static string Make(IEnumerable items)
        {
            var strings = new List<string>();

            foreach (var item in items)
            {
                strings.Add(Make(Convert.ToString(item)));
            }

            return "[" + string.Join(", ", strings) + "]";
        }
    }
}
