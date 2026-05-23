@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo === 发布命令行版 ===
dotnet publish ExcelTool/ExcelTool.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
if errorlevel 1 ( echo 命令行版发布失败 & pause & exit /b 1 )

echo.
echo === 发布GUI版 ===
dotnet publish ExcelToolGUI/ExcelToolGUI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish-gui
if errorlevel 1 ( echo GUI版发布失败 & pause & exit /b 1 )

echo.
echo === 发布完成 ===
echo 命令行版: publish\ExcelTool.exe
echo GUI版:    publish-gui\ExcelToolGUI.exe
pause
