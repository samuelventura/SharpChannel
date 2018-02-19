using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace SharpChannel.Tools
{
    public class Executable
    {
        public static int GetProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        public static string GetAssemblyName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        public static string Relative(string filename)
        {
            var entry = Assembly.GetEntryAssembly().Location;
            var folder = Path.GetDirectoryName(entry);
            return Path.Combine(folder, filename);
        }

        public static string Relative(string subfolder, string filename)
        {
            var entry = Assembly.GetEntryAssembly().Location;
            var folder = Path.GetDirectoryName(entry);
            return Path.Combine(folder, subfolder, filename);
        }
    }
}