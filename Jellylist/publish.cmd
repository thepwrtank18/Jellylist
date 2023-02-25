@echo off
echo Publishing for Windows...
dotnet publish -c Release /p:PublishProfile=Properties/PublishProfiles/Windows.pubxml --self-contained true
echo Publishing for macOS...
dotnet publish -c Release /p:PublishProfile=Properties/PublishProfiles/macOS.pubxml --self-contained true
echo Publishing for Linux...
dotnet publish -c Release /p:PublishProfile=Properties/PublishProfiles/Linux.pubxml --self-contained true
echo Done!
@echo on