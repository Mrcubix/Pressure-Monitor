@echo off

tar -acf ./Pressure-Monitor/res/overlay.zip --directory=./Pressure-Monitor/res/overlay/ *
dotnet publish Pressure-Monitor -c Release -o build

echo ----------------------------------------------------------------
echo Compressing dependencies...
echo ----------------------------------------------------------------

cd build

move CircularBuffer.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move MessagePack.Annotations.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move MessagePack.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move Microsoft.Bcl.AsyncInterfaces.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move Microsoft.VisualStudio.Threading.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move Microsoft.VisualStudio.Validation.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move NerdBank.Streams.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move Newtonsoft.Json.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move OTD.EnhancedOutputMode.Lib.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move Pressure-monitor.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move Proxy-API.Lib.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move StreamJsonRpc.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move System.Diagnostics.DiagnosticSource.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move System.IO.Pipelines.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul
move ScottPlot.dll ../Pressure-Monitor.Installer/res/dependencies/ > nul

cd ..

tar -acf ./Pressure-Monitor.Installer/res/dependencies.zip --directory=./Pressure-Monitor.Installer/res/dependencies/ *

echo ----------------------------------------------------------------
echo Dependencies should now be compressed and ready to be installed.
echo Cleaning up...
echo ----------------------------------------------------------------

rmdir /S /Q build > nul

dotnet publish Pressure-Monitor.Installer/Pressure-Monitor.Installer.csproj -c Release -o build

echo ----------------------------------------------------------------
echo Cleaning up useless files...
echo ----------------------------------------------------------------

cd build

move Pressure-Monitor.Installer.dll ../bin/Pressure-Monitor.Installer.dll > nul
move Proxy-API.Lib.dll ../bin/Proxy-API.Lib.dll > nul

cd ..

rmdir /S /Q build > nul

mkdir build

cd build

move ../bin/Pressure-Monitor.Installer.dll Pressure-Monitor.Installer.dll > nul
move ../bin/Proxy-API.Lib.dll Proxy-API.Lib.dll > nul

cd ..

echo ----------------------------------------------------------------
echo Done
echo ----------------------------------------------------------------