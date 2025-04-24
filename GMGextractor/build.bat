@echo off
echo GMGextractor Build Script
echo ========================

:: Check if MSBuild is available
where /q msbuild
if %ERRORLEVEL% NEQ 0 (
    echo MSBuild not found in PATH
    echo Please run this from a Visual Studio Developer Command Prompt
    echo or ensure MSBuild is in your PATH
    pause
    exit /b 1
)

:: Build the project
echo Building GMGextractor...
msbuild GMGextractor.csproj /p:Configuration=Release

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Build completed successfully!
echo The application is available in bin\Release folder
pause