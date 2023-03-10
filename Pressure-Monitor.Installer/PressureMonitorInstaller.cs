using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace Pressure_Monitor.Installer
{
    [PluginName("Pressure Monitor Installer")]
    public class PressureMonitorInstaller : ITool
    {
        private static string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        private static string pluginLocationDirectory = assemblyLocation.Replace("Pressure-monitor.dll", "");
        private static string pluginDirectory = assemblyLocation.Substring(0, assemblyLocation.LastIndexOf(Path.DirectorySeparatorChar));
        private static string pluginsDirectory = pluginDirectory.Substring(0, pluginDirectory.LastIndexOf(Path.DirectorySeparatorChar));

        string OTDEnhancedOutputModeDirectory = $"{pluginsDirectory}/OTD.EnhancedOutputMode";
        private string dependenciesResourcePath = "Pressure-Monitor.Extraction...res.dependencies.zip";


        public bool Initialize()
        {
            new Thread(new ThreadStart(() => ExtractDlls(dependenciesResourcePath, OTDEnhancedOutputModeDirectory, ForceInstall))).Start();
            return true;
        }

        public void Dispose()
        {
        }

        public bool InstallDepedencies(string group, string resourcePath, string destinationDirectory, bool forceInstall = false)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var dependencies = assembly.GetManifestResourceStream(resourcePath);

            if (dependencies == null)
            {
                Log.Write($"{group} Installer", "Failed to open embedded dependencies.", LogLevel.Error);
                return false;
            }

            int entriesCount = 0;
            int installed = 0;

            using (ZipArchive archive = new ZipArchive(dependencies, ZipArchiveMode.Read))
            {
                var entries = archive.Entries;
                entriesCount = entries.Count;

                foreach (ZipArchiveEntry entry in entries)
                {
                    string destinationPath = $"{destinationDirectory}/{entry.FullName}";

                    if (File.Exists(destinationPath) && !forceInstall)
                        continue;

                    entry.ExtractToFile(destinationPath, true);
                    installed++;
                }
            }

            if (installed > 0)
                Log.Write($"{group} Installer", $"Installed {installed} of {entriesCount} dependencies.", LogLevel.Info);

            return true;
        }

        [BooleanProperty("Force Install", ""),
         DefaultPropertyValue(false),
         ToolTip("Force install the dependencies even if they are already installed.")]
        public bool ForceInstall { get; set; }
    }

}
