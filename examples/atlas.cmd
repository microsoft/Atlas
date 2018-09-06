
@echo off
SETLOCAL
SET ATLAS_CSPROJ=%~dp0..\src\Microsoft.Atlas.CommandLine\Microsoft.Atlas.CommandLine.csproj

dotnet run --project "%ATLAS_CSPROJ%" --no-launch-profile -- %*
