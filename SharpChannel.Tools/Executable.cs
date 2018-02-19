using System.IO;
using System.Reflection;

namespace SharpChannel.Tools
{
    public class Executable
    {
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
