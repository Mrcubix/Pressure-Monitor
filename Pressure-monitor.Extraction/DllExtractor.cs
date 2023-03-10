using System.IO;
using System.Reflection;
using System.Threading;
using OpenTabletDriver.Plugin;

namespace Pressure_monitor.Extraction
{
    public class DllExtractor : ITool
    {
        private static string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        private static string pluginLocationDirectory = assemblyLocation.Replace("Pressure-monitor.dll", "");
        
        private string pluginDirectory = assemblyLocation.Substring(0, assemblyLocation.LastIndexOf(Path.DirectorySeparatorChar));
        private string[] dependencies = new string[]
        {
            "CircularBuffer.dll",
            "MessagePack.Annotations.dll",
            "MessagePack.dll",
            "Microsoft.Bcl.AsyncInterfaces.dll",
            "Microsoft.VisualStudio.Threading.dll",
            "Microsoft.VisualStudio.Validation.dll",
            "NerdBank.Streams.dll",
            "Newtonsoft.Json.dll",
            "OTD.EnhancedOutputMode.Lib.dll",
            "Pressure-monitor.dll",
            "Proxy_API.Lib.dll",
            "StreamJsonRpc.dll",
            "System.Diagnostics.DiagnosticSource.dll",
            "System.IO.Pipelines.dll"
        };

        public bool Initialize()
        {
            new Thread(new ThreadStart(() => ExtractDlls())).Start();
            return true;
        }

        public void Dispose()
        {
        }

        public bool ExtractDlls()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string OTDEnhancedOutputModeDirectory = $"{pluginDirectory}/OTD.EnhancedOutputMode";

            var dependencies = assembly.GetManifestResourceNames();

            foreach (string dependency in dependencies)
            {
                Log.Write("DllExtractor", $"Extracting {dependency} to {OTDEnhancedOutputModeDirectory}", LogLevel.Info);

                if (File.Exists($"{OTDEnhancedOutputModeDirectory}/{dependency}"))
                    continue;
            }

            return true;
        }
    }

}
