SETLOCAL
CD %~dp0
SET BinFolder=%~dp0bin

dotnet restore --force
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet build src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -c Release
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet build test/Microsoft.Atlas.CommandLine.Tests/Microsoft.Atlas.CommandLine.Tests.csproj -c Release
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet test test/Microsoft.Atlas.CommandLine.Tests/Microsoft.Atlas.CommandLine.Tests.csproj -c Release
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet publish src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -c Release -o %BinFolder%\atlas
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet pack src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj -o %BinFolder%\tool
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet msbuild src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj /t:Restore,CreateTarball /p:RuntimeIdentifier=linux-x64 /p:TargetFramework=netcoreapp2.1 /p:Configuration=Release /p:ArchiveDir=%BinFolder%\downloads
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet msbuild src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj /t:Restore,CreateZip /p:RuntimeIdentifier=win10-x64 /p:TargetFramework=netcoreapp2.1 /p:Configuration=Release /p:ArchiveDir=%BinFolder%\downloads
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet pack src/Microsoft.Atlas.CommandLine.Chocolatey/Microsoft.Atlas.CommandLine.Chocolatey.csproj -o %BinFolder%\chocolatey
if %errorlevel% neq 0 exit /b %errorlevel%

call src/Tasks/build.cmd
if %errorlevel% neq 0 exit /b %errorlevel%
