tar -acf ./Pressure-Monitor/res/overlay.zip --directory=./Pressure-Monitor/res/overlay/ *
dotnet publish Pressure-Monitor -c Release -o build
tar -acf ./Pressure-Monitor.Installer/res/dependencies.zip -T ./Pressure-Monitor/res/dependencies.txt --directory="./build"
rm -rf ./build/*
dotnet publish Pressure-Monitor.Installer/Pressure-Monitor.Installer.csproj -c Release -o build