SETLOCAL
SET BinFolder=%~dp0bin

dotnet restore src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj
dotnet build src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -c Release
dotnet publish src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -c Release -o %BinFolder%\atlas
dotnet msbuild src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj /t:Restore,CreateTarball /p:RuntimeIdentifier=linux-x64 /p:TargetFramework=netcoreapp2.0 /p:Configuration=Release /p:ArchiveDir=%BinFolder%\downloads
dotnet msbuild src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj /t:Restore,CreateZip /p:RuntimeIdentifier=win10-x64 /p:TargetFramework=netcoreapp2.0 /p:Configuration=Release /p:ArchiveDir=%BinFolder%\downloads
dotnet pack src/Microsoft.Atlas.CommandLine.Chocolatey/Microsoft.Atlas.CommandLine.Chocolatey.csproj -o %BinFolder%\chocolatey
