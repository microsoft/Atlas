
@echo off
SETLOCAL
cd %~dp0

SET ATLAS_CSPROJ=../src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj

FOR /D %%G IN (???-*) DO (
    echo Executing %%G
    dotnet run --project "%ATLAS_CSPROJ%" --no-launch-profile -- deploy %%G --dry-run
)

