@echo off
cd /d "%~dp0"

set LOGFILE=%~dp0sync-log.txt
echo [%date% %time%] Starting sync... >> "%LOGFILE%"

git pull origin master >> "%LOGFILE%" 2>&1
if %errorlevel% neq 0 (
    echo [%date% %time%] ERROR: Pull failed. >> "%LOGFILE%"
    exit /b 1
)

git add -A >> "%LOGFILE%" 2>&1

git diff --cached --quiet
if %errorlevel% neq 0 (
    git commit -m "Auto-sync: %date% %time%" >> "%LOGFILE%" 2>&1
)

git push origin master >> "%LOGFILE%" 2>&1
if %errorlevel% neq 0 (
    echo [%date% %time%] ERROR: Push failed. >> "%LOGFILE%"
    exit /b 1
)

echo [%date% %time%] Sync complete. >> "%LOGFILE%"
