@echo off
echo ========================================
echo Mess Management System - Setup Script
echo ========================================
echo.

echo [1/3] Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.

echo [2/3] Building the project...
dotnet build
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo [3/3] Starting the application...
echo.
echo ========================================
echo Application is starting...
echo Open your browser and go to:
echo https://localhost:5001
echo.
echo Login with:
echo Username: admin
echo Password: admin123
echo ========================================
echo.

dotnet run

pause
