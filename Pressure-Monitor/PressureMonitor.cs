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
using ScottPlot.Statistics.Interpolation;

namespace Pressure_Monitor
{   
    [PluginName("Pressure Monitor")]
    public class PressureMonitor : IFilter, IGateFilter, IDisposable
    {
        private Client client = new Client("API");
        private Server server;
        private EventHandler<uint> pressureChanged;
        private CircularBuffer<double> pressureMeasurementsBuffer;
        private double[] timeStamps;
        private string zipEmbeddedResource = "Pressure-Monitor...res.overlay.zip";
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
            {
                pressureMeasurementsBuffer = new CircularBuffer<double>(NumberOfPoints);

                for (int i = 0; i < NumberOfPoints; i++)
                {
                    pressureMeasurementsBuffer.PushFront(0);
                }
            }

            if (timeStamps == null)
            {
                timeStamps = new double[NumberOfPoints];

                for (int i = 0; i < NumberOfPoints; i++)
                {
                    timeStamps[i] = i * PressureMeasurementInterval;
                }
            }

            if (Environment.TickCount - lastPressureMeasurement > PressureMeasurementInterval)
            {
                if (tabletreport is ITabletReport tabletReport)
                {
                    pressureChanged?.Invoke(this, tabletreport.Pressure);
                    lastPressureMeasurement = Environment.TickCount;
                }
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
                return OverlayExtractor.TryExtractingEmbeddedResource(Assembly.GetExecutingAssembly(), zipEmbeddedResource, $"{OverlayExtractor.OverlayDirectory}/PressureMonitor");
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
            (double[] x, double[] y) = Cubic.InterpolateXY(timeStamps, pressureMeasurementsBuffer.ToArray(), NumberOfPoints + (NumberOfPoints * Smoothness));

            _ = SendPointsAsync(x, y);
        }

        public async Task SendPointsAsync(double[] x, double[] y)
        {
            string serializedX = System.Text.Json.JsonSerializer.Serialize(x);
            string serializedY = System.Text.Json.JsonSerializer.Serialize(y);

            await client.Rpc.NotifyAsync("SendDataAsync", "PressureMonitor", "X", serializedX);
            await client.Rpc.NotifyAsync("SendDataAsync", "PressureMonitor", "Y", serializedY);
        }

        public async Task SendMaxPressureAsync()
        {
            await client.Rpc.NotifyAsync("SendDataAsync", "PressureMonitor", "maxPressure", Info.Driver.Tablet.Digitizer.MaxPressure);
        }

#endregion
        public FilterStage FilterStage => FilterStage.PostTranspose;

        [Property("Number of points"),
         DefaultPropertyValue(50),
         Unit("points"),
         ToolTip("The number of points that will be drawn on the graph")
        ]
        public int NumberOfPoints { set; get; }

        [Property("Smoothness"),
         DefaultPropertyValue(1000),
         Unit("%"),
         ToolTip("The number of points that will be drawn on the graph")
        ]
        public int Smoothness
        {
            set
            {
                _smoothness = value / 100;
            }
            get
            {
                return (int)(_smoothness * 100);
            }
        }
        private double _smoothness = 1000;

        [Property("Pressure Measurement Interval"),
         DefaultPropertyValue(100),
         Unit("ms"),
         ToolTip("The interval at which the pressure will be measured in milliseconds")
        ]
        public int PressureMeasurementInterval { set; get; }
    }
}
