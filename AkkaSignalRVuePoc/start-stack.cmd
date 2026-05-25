@echo off
setlocal

cd /d "%~dp0"

echo.
echo Akka SignalR Vue POC - Docker stack
echo ===================================
echo.

echo Checking Docker Desktop...
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker Desktop is not running or not available in PATH.
    echo Start Docker Desktop, wait until it is ready, then run this script again.
    exit /b 1
)

echo Building and starting containers...
docker compose up --build -d
if errorlevel 1 (
    echo [ERROR] Failed to start the stack.
    exit /b 1
)

echo.
echo Stack is running:
echo   Vue client  http://localhost:5173
echo   API         http://localhost:5000
echo   Swagger     http://localhost:5000/swagger
echo   Seq         http://localhost:8081
echo   SQL Server  localhost:1433
echo.
echo View logs : docker compose logs -f
echo Stop stack: docker compose down
echo.

endlocal
