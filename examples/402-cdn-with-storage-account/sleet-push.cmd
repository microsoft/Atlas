dotnet tool install sleet --tool-path .\bin
.\bin\sleet init --source packages
.\bin\sleet push {{ latest.drop }}\tools --source packages
