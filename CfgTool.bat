@echo off
chcp 65001 >nul

if "%~1"=="" (
    echo 用法: CfgTool.bat [json配置文件路径]
    echo 示例: CfgTool.bat .\CfgImport.json
    pause
    exit /b 1
)

set "jsonPath=%~f1"
set "exePath=%~dp0publish\ExcelTool.exe"

if not exist "%jsonPath%" (
    echo 错误：配置文件不存在 - %jsonPath%
    pause
    exit /b 1
)

if not exist "%exePath%" (
    echo 错误：ExcelTool.exe 不存在，请先执行发布
    echo 命令: dotnet publish ExcelTool/ExcelTool.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish
    pause
    exit /b 1
)

"%exePath%" "%jsonPath%"
pause
