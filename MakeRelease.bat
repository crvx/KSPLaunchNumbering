@echo off
setlocal enabledelayedexpansion
for /f %%i in ('git describe --tags --always 2^>nul') do set VERSION=%%i
if "%VERSION%"=="" set VERSION=dev
set OUTPUT=LaunchNumbering-%VERSION%.zip
set TEMP=%TEMP%\LNRelease

msbuild LaunchNumbering\LaunchNumbering.csproj /p:Configuration=Release /t:Build /nologo

if exist "%TEMP%" rmdir /s /q "%TEMP%"
mkdir "%TEMP%\GameData\LaunchNumbering\Plugins"
mkdir "%TEMP%\GameData\LaunchNumbering\Textures"

copy LaunchNumbering\bin\Release\LaunchNumbering.dll "%TEMP%\GameData\LaunchNumbering\Plugins\"
copy GameData\LaunchNumbering\Textures\*.png "%TEMP%\GameData\LaunchNumbering\Textures\"
copy README.md "%TEMP%\GameData\LaunchNumbering\"
copy LICENSE "%TEMP%\GameData\LaunchNumbering\"

cd /d "%TEMP%"
tar -acf "%CD%\%OUTPUT%" GameData
move "%OUTPUT%" "%OLDPWD%" > nul
cd /d "%OLDPWD%"
rmdir /s /q "%TEMP%"
echo Created %OUTPUT%
