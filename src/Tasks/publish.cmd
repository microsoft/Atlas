
SETLOCAL
pushd %~dp0
IF "%BinFolder%" == "" SET BinFolder=%~dp0bin

del %BinFolder%\*.vsix

call build.cmd
if %errorlevel% neq 0 exit /b %errorlevel%

for %%f in (%BinFolder%\*.vsix) do (
    call tfx extension publish --vsix %%f --share-with atlas-test --token %TFX_PUBLISH_TOKEN%
    if !errorlevel! neq 0 exit /b !errorlevel!
)

popd
if %errorlevel% neq 0 exit /b %errorlevel%
