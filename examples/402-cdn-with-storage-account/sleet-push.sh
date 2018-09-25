dotnet tool install -g sleet
sleet init --source packages
sleet push {{ latest.drop }}/tools --source packages
