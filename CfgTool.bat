@echo off
set jsonFilePath=".\Excel\CfgImport.json"
set exePath=".\ExcelTool\ExcelTool.exe"
%exePath% %jsonFilePath%
pause