DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
BinFolder="${DIR}/bin"
set -e

dotnet restore --force
dotnet build -c Release
dotnet test test/Microsoft.Atlas.CommandLine.Tests/Microsoft.Atlas.CommandLine.Tests.csproj -c Release

dotnet publish src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -c Release -o ${BinFolder}/atlas
dotnet pack src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -c Release -o ${BinFolder}/tools
dotnet msbuild src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj /t:Restore,CreateTarball /p:RuntimeIdentifier=linux-x64 /p:TargetFramework=netcoreapp2.1 /p:Configuration=Release /p:ArchiveDir=${BinFolder}/downloads
dotnet msbuild src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj /t:Restore,CreateZip /p:RuntimeIdentifier=win10-x64 /p:TargetFramework=netcoreapp2.1 /p:Configuration=Release /p:ArchiveDir=${BinFolder}/downloads
dotnet pack src/Microsoft.Atlas.CommandLine.Chocolatey/Microsoft.Atlas.CommandLine.Chocolatey.csproj -c Release -o ${BinFolder}/chocolatey
