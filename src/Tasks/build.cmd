SETLOCAL
CD %~dp0
IF "%BinFolder%" == "" SET BinFolder=%~dp0bin

echo "Building AtlasInstallerV0"
pushd AtlasInstallerV0
if %errorlevel% neq 0 exit /b %errorlevel%

call npm install
if %errorlevel% neq 0 exit /b %errorlevel%

call tsc
if %errorlevel% neq 0 exit /b %errorlevel%

popd
if %errorlevel% neq 0 exit /b %errorlevel%

echo "Building AtlasV0"
pushd AtlasV0
if %errorlevel% neq 0 exit /b %errorlevel%

call npm install
if %errorlevel% neq 0 exit /b %errorlevel%

call tsc
if %errorlevel% neq 0 exit /b %errorlevel%

popd
if %errorlevel% neq 0 exit /b %errorlevel%

echo "Building vsix"
call tfx extension create --manifest-globs vss-extension.json --output-path %BinFolder%
if %errorlevel% neq 0 exit /b %errorlevel%

