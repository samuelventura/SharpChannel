using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using SharpChannel.Tools;

namespace SharpChannel.Manager
{
    public class ChannelUtils
    {
        public static string SetupString(ChannelInstance instance)
        {
            return string.Format("{0} {1} {2} {3}",
                                 instance.Type,
                                 instance.Access,
                                 instance.Port,
                                 instance.Config
                                );
        }

        public static bool IsThirdParty(string type)
        {
            var path = Executable.Relative("views", $"Edit{type}Channel.html");
            return !File.Exists(path);
        }

        public static string ExecutableName(string type)
        {
            var name = string.Format("SharpChannel.Channels.{0}Channel", type);
            if (!IsThirdParty(type)) return $"{name}.exe";
            return Path.Combine("thirdparty", name, $"{name}.exe");
        }

        public static string IPAccess(ChannelAccess access)
        {
            switch (access)
            {
                case ChannelAccess.Local: return IPAddress.Loopback.ToString();
                case ChannelAccess.Remote: return IPAddress.Any.ToString();
            }
            return null;
        }

        public static List<string> List(string type)
        {
            var filename = ExecutableName(type);
            var filepath = Executable.Relative(filename);
            var pi = new ProcessStartInfo(filepath)
            {
                Arguments = "--list",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                //WorkingDirectory = Path.GetDirectoryName(filepath),
            };
            var disposer = new Disposer();
            using (disposer)
            {
                var proc = Process.Start(pi);
                disposer.Add(proc);
                disposer.Add(proc.Kill);
                var list = new List<string>();
                var line = proc.StandardOutput.ReadLine();
                while(line != null)
                {
                    list.Add(line);
                    line = proc.StandardOutput.ReadLine();
                }
                return list;
            }
        }
    }
}
