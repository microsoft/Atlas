ATLAS_CSPROJ="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )/src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj"

function atlas {
  dotnet run -v q --no-restore --no-launch-profile --project ${ATLAS_CSPROJ} -- "$@"
}

atlas "$@"
