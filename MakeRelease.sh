#!/bin/bash
set -e
VERSION=$(git describe --tags --always 2>/dev/null || echo "dev")
OUTPUT="LaunchNumbering-${VERSION}.zip"
TEMP=$(mktemp -d)

msbuild LaunchNumbering/LaunchNumbering.csproj /p:Configuration=Release /t:Build /nologo

mkdir -p "${TEMP}/GameData/LaunchNumbering/Plugins"
mkdir -p "${TEMP}/GameData/LaunchNumbering/Textures"

cp LaunchNumbering/bin/Release/LaunchNumbering.dll "${TEMP}/GameData/LaunchNumbering/Plugins/"
cp GameData/LaunchNumbering/Textures/*.png "${TEMP}/GameData/LaunchNumbering/Textures/"
cp README.md LICENSE "${TEMP}/GameData/LaunchNumbering/"

cd "${TEMP}"
zip -r "${OLDPWD}/${OUTPUT}" GameData/
cd "${OLDPWD}"
rm -rf "${TEMP}"
echo "Created ${OUTPUT}"
