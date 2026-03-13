@echo off
title GitHub Sync
echo ========================================
echo         Syncing with GitHub...
echo ========================================
echo.

cd /d "%~dp0"

echo [1/4] Downloading latest changes from team...
git pull origin master
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Could not download changes. There may be a conflict.
    echo Please ask a teammate for help.
    pause
    exit /b 1
)

echo.
echo [2/4] Saving your changes...
git add -A

echo.
echo [3/4] Packaging your changes...
git diff --cached --quiet
if %errorlevel% equ 0 (
    echo No new changes to save.
) else (
    git commit -m "Auto-sync: %date% %time%"
)

echo.
echo [4/4] Uploading your changes to GitHub...
git push origin master
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Could not upload changes.
    echo Please ask a teammate for help.
    pause
    exit /b 1
)

echo.
echo ========================================
echo   Done! Everything is up to date.
echo ========================================
echo.
pause
