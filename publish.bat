rmdir /s /q "build-windows"
dotnet publish GameClient/GameClient.csproj -r win-x64 -c Release --output build-windows
del /s "NetCoreBeauty"
pause