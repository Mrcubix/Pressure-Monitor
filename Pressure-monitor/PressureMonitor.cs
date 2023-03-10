using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using CircularBuffer;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Lib.Interface;
using Proxy_API.Lib.Overlay.Extraction;
using Proxy_API.Lib.Pipes;

namespace Pressure_monitor
{   
    [PluginName("Pressure Monitor")]
    public class PressureMonitor : IFilter, IGateFilter, IDisposable
    {
        private Client client = new Client("API");
        private Server server;
        private EventHandler<uint> pressureChanged;
        private CircularBuffer<uint> pressureMeasurementsBuffer;
        private string zipEmbeddedResource = "Pressure-monitor...res.overlay.zip";
        private int lastPressureMeasurement = Environment.TickCount;

        public PressureMonitor()
        {
            _ = Task.Run(InitializeAsync);
            _ = Task.Run(SendMaxPressureAsync);

            pressureChanged += OnPressureChange;
        }

        public Vector2 Filter(Vector2 point)
        {
            return point;
        }

        public bool Pass(IDeviceReport report, ref ITabletReport tabletreport)
        {
            if (pressureMeasurementsBuffer == null)
                pressureMeasurementsBuffer = new CircularBuffer<uint>(NumberOfPoints);

            Log.Write("PressureMonitor", $"Time since last pressure measurement: {Environment.TickCount - lastPressureMeasurement}", LogLevel.Debug);

            if (Environment.TickCount - lastPressureMeasurement > PressureMeasurementInterval)
            {
                if (tabletreport is ITabletReport tabletReport)
                    pressureChanged?.Invoke(this, tabletreport.Pressure);
            }

            return true;
        }

#region Initialization

        public async Task InitializeAsync()
        {
            Log.Debug("PressureMonitor", $"Extracting overlays if neccessary...");

            // check if overlays have been extracted, if not, extract them
            if (!ExtractOverlays())
            {
                Log.Write("PressureMonitor", $"Overlays could not be extracted", LogLevel.Error);
                return;
            }

            server = new Server("PressureMonitor", this);
            _ = server.StartAsync();

            // connect to the API
            await client.StartConnectionAsync();
        }

        public bool ExtractOverlays()
        {
            if (OverlayExtractor.AssemblyHasAlreadyBeenExtracted(zipEmbeddedResource))
            {
                Log.Write("PressureMonitor", $"Overlays have already been extracted", LogLevel.Debug);
                return true;
            }

            try
            {
                return OverlayExtractor.TryExtractingEmbeddedResource(Assembly.GetExecutingAssembly(), zipEmbeddedResource, $"{OverlayExtractor.overlayDirectory}/PressureMonitor");
            }
            catch (Exception e)
            {
                Log.Write("PressureMonitor", $"Exception while trying to extract overlays: {e}", LogLevel.Error);
                return false;
            }
        }

        public void Dispose()
        {
            server.Dispose();
            client.Dispose();
        }

#endregion

#region networking
#nullable enable
        public void OnPressureChange(object? sender, uint pressure)
        {
            pressureMeasurementsBuffer.PushFront(pressure);
            _ = SendPointsAsync();
        }

        public async Task SendPointsAsync()
        {
            string serializedBuffer = System.Text.Json.JsonSerializer.Serialize(pressureMeasurementsBuffer.ToArray());
            await client.Rpc.NotifyAsync("SendDataAsync", "PressureMonitor", "points", serializedBuffer);
        }

        public async Task SendMaxPressureAsync()
        {
            await client.Rpc.NotifyAsync("SendDataAsync", "PressureMonitor", "maxPressure", Info.Driver.Tablet.Digitizer.MaxPressure);
        }

#endregion
        public FilterStage FilterStage => FilterStage.PostTranspose;

        [Property("Number of points"),
         DefaultPropertyValue(50),
         ToolTip("The number of points that will be drawn on the graph")
        ]
        public int NumberOfPoints { set; get; }

        [Property("Pressure Measurement Interval"),
         DefaultPropertyValue(100),
         ToolTip("The interval at which the pressure will be measured in milliseconds")
        ]
        public int PressureMeasurementInterval { set; get; }
    }
}
