@echo off
echo ========================================
echo   SharpMono Injector Build Script
echo ========================================
echo.

:: Check if .NET SDK is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK is not installed!
    echo Please download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1] Building AntiCheat DLL...
cd AntiCheatDLL
dotnet build AntiCheatStandalone.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to build AntiCheat DLL
    pause
    exit /b 1
)
echo [OK] AntiCheat.dll built successfully!
echo.

echo [2] Building SharpMono Injector GUI...
cd ..\SharpMonoInjectorGUI
dotnet build SharpMonoInjectorGUI.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to build SharpMono Injector GUI
    pause
    exit /b 1
)
echo [OK] SharpMonoInjector.exe built successfully!
echo.

echo ========================================
echo   BUILD COMPLETED SUCCESSFULLY!
echo ========================================
echo.
echo Output files:
echo - AntiCheat.dll: AntiCheatDLL\bin\Release\netstandard2.0\AntiCheat.dll
echo - Injector.exe:  SharpMonoInjectorGUI\bin\Release\net6.0-windows\SharpMonoInjector.exe
echo.
pause