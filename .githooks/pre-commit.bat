@echo off
cd /d "%~dp0"
dotnet restore
git add **/packages.lock.json
exit /b %ERRORLEVEL%
