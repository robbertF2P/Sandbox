@echo off
setlocal

cd /d "%~dp0"

echo.
echo Stopping Akka SignalR Vue POC stack...
echo.

docker compose down
if errorlevel 1 (
    echo [ERROR] Failed to stop the stack.
    exit /b 1
)

echo.
echo Stack stopped.
echo.

endlocal
