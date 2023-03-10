@echo off

tar -acf ./Pressure-Monitor/res/overlay.zip --directory=./Pressure-Monitor/res/overlay/ *
dotnet publish Pressure-Monitor -c Release -o build
echo "built Pressure-Monitor"

cd build

echo "compressing dependencies..."
mkdir dependencies

move CircularBuffer.dll dependencies
move MessagePack.Annotations.dll dependencies
move MessagePack.dll dependencies
move Microsoft.Bcl.AsyncInterfaces.dll dependencies
move Microsoft.VisualStudio.Threading.dll dependencies
move Microsoft.VisualStudio.Validation.dll dependencies
move NerdBank.Streams.dll dependencies
move Newtonsoft.Json.dll dependencies
move OTD.EnhancedOutputMode.Lib.dll dependencies
move Pressure-monitor.dll dependencies
move Proxy-API.Lib.dll dependencies
move StreamJsonRpc.dll dependencies
move System.Diagnostics.DiagnosticSource.dll dependencies
move System.IO.Pipelines.dll dependencies


tar -acf ../Pressure-Monitor.Installer/res/dependencies.zip --directory=./dependencies/ *

cd ..
del /S /Q build

dotnet publish Pressure-Monitor.Installer -c Release -o build
echo "built Pressure-Monitor.Installer"