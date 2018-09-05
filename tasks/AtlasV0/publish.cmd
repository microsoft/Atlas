
SETLOCAL
pushd %~dp0
call bin\token.cmd
REM if %errorlevel% neq 0 exit /b %errorlevel%

del bin\*.vsix
REM if %errorlevel% neq 0 exit /b %errorlevel%

pushd %~dp0runAtlas\src
if %errorlevel% neq 0 exit /b %errorlevel%

call npm install
if %errorlevel% neq 0 exit /b %errorlevel%

call npm run-script tasks:bump
if %errorlevel% neq 0 exit /b %errorlevel%

call tsc
if %errorlevel% neq 0 exit /b %errorlevel%

popd
if %errorlevel% neq 0 exit /b %errorlevel%

call dotnet publish ../../src/Microsoft.Atlas.CommandLine/Microsoft.Atlas.CommandLine.csproj --configuration Release --output "%~dp0\runAtlas\bin"
if %errorlevel% neq 0 exit /b %errorlevel%

call tfx extension create --manifest-globs vss-extension.json --rev-version --output-path bin
if %errorlevel% neq 0 exit /b %errorlevel%

for %%f in (bin\*.vsix) do (
    call tfx extension publish --vsix %%f --share-with asgmicroservices --token %TFX_PUBLISH_TOKEN%
    if !errorlevel! neq 0 exit /b !errorlevel!
)

popd
if %errorlevel% neq 0 exit /b %errorlevel%
