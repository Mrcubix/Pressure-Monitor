using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using Proxy_API.Lib.Dependencies;

namespace Pressure_Monitor.Installer
{
    [PluginName("Pressure Monitor Installer")]
    public class PressureMonitorInstaller : ITool
    {
        private static string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        private static string pluginLocationDirectory = assemblyLocation.Replace("Pressure-Monitor.dll", "");
        private static string pluginDirectory = assemblyLocation.Substring(0, assemblyLocation.LastIndexOf(Path.DirectorySeparatorChar));
        private static string pluginsDirectory = pluginDirectory.Substring(0, pluginDirectory.LastIndexOf(Path.DirectorySeparatorChar));

        string OTDEnhancedOutputModeDirectory = $"{pluginsDirectory}/OTD.EnhancedOutputMode";
        private string dependenciesResourcePath = "Pressure-Monitor.Installer...res.dependencies.zip";
        private string group = "Pressure Monitor";
        private Assembly assembly = Assembly.GetExecutingAssembly();


        public bool Initialize()
        {
            new Thread(new ThreadStart(() => DependencyInstaller.Install(assembly, group, dependenciesResourcePath, OTDEnhancedOutputModeDirectory, ForceInstall))).Start();
            return true;
        }

        public void Dispose()
        {
        }

        [BooleanProperty("Force Install", ""),
         DefaultPropertyValue(false),
         ToolTip("Force install the dependencies even if they are already installed.")]
        public bool ForceInstall { get; set; }
    }

}
