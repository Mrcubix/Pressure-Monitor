tar -acf ./Pressure-monitor/res/overlay.zip --directory=./Pressure-monitor/res/overlay/ *
dotnet publish Pressure-monitor -c Release -o build
tar -acf ./Pressure-monitor.Extraction/res/dependencies.zip -T ./Pressure-monitor/res/dependencies.txt --directory="./build"
rm -rf ./build/*
dotnet publish Pressure-monitor.Extraction -c Release -o build