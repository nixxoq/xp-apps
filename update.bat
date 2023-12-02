@echo off

echo Updating XP-apps...

set "current_path=%~dp0"
set "CURL_PATH=%current_path%\tools\curl\curl.exe"
set "ZIP_PATH=%current_path%\tools\7z.exe"

"%CURL_PATH%" -# -L -o install.bat https://raw.githubusercontent.com/Snaky1a/xp-apps/main/install.bat

echo Done

pause
