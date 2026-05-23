# ExcelTool

Unity 游戏项目的 Excel 配置导出工具。从 Excel 表格生成 C# 配置类和二进制数据文件。

## 功能特性

- 支持基础类型（int、string、bool）、数组类型（int[]、string[]、bool[]）、字典类型（<int,string> 等）
- 单个文件导出 / 合并文件夹导出（结构差异自动兼容）
- 主键自动检测（int/string），无主键自动生成
- 类型转换容错（警告+默认值，不崩溃）
- 可选异或加密
- 临时目录生成，原子性移动（失败不破坏旧文件）
- 生成代码访问性封装，使用者只看到必要的公开 API
- GUI 版 + 命令行版双模式

## 项目结构

```
ExcelTool/
├── ExcelTool/              ← 核心逻辑（命令行版入口）
│   ├── Program.cs          ← 命令行入口
│   ├── Generator.cs        ← 主流程编排
│   ├── ExcelReader.cs      ← Excel读取
│   ├── CodeGenerator.cs    ← C#代码生成
│   ├── CfgBinaryWriter.cs  ← 二进制写入
│   ├── CfgEncryptor.cs     ← 加密模块
│   ├── TypeHelper.cs       ← 类型辅助
│   └── CfgModel.cs         ← 数据模型
├── ExcelToolGUI/           ← GUI版（WinForms壳）
├── Example/                ← 生成脚本示例（仅供参考）
├── publish/                ← 命令行版发布产出
├── publish-gui/            ← GUI版发布产出
├── CfgImport.json          ← 配置文件模板
├── CfgTool.bat             ← bat启动脚本
└── publish.bat             ← 一键发布脚本
```

## 使用方式

### GUI 版（推荐给策划/美术）

双击 `publish-gui/ExcelToolGUI.exe`：

1. 选择配置 JSON 文件（定义导出哪些表）
2. 选择 Excel 根目录
3. 选择脚本输出目录
4. 选择字节文件输出目录
5. 填写命名空间（默认 GameConfig）
6. 点击"导出配置"

所有路径会自动记住，下次打开无需重新选择。

### 命令行版（适合 CI/脚本）

```
ExcelTool.exe <jsonPath> <excelPath> <scriptOutputPath> <bytesOutputPath> [namespace]
```

示例：
```
ExcelTool.exe CfgImport.json ./Excel ./Output/Scripts/Config ./Output/StreamingAssets Game.Cfg
```

### bat 脚本

```
CfgTool.bat CfgImport.json
```

## CfgImport.json 配置

```json
{
    "encryptKey": "",
    "mergedFolders": [
        { "folder": "language/", "type": "LanguageCfg" }
    ],
    "singleFiles": [
        { "excel": "item.xlsx", "type": "ItemCfg" }
    ]
}
```

| 字段 | 说明 |
|------|------|
| `encryptKey` | 加密密钥，为空则不加密 |
| `mergedFolders` | 合并导出：文件夹下所有 Excel 合并为一个配置类 |
| `singleFiles` | 单个导出：每个 Excel 对应一个独立的配置类 |

其他参数（namespace、excelPath、scriptOutputPath、bytesOutputPath）通过 GUI 界面或命令行参数传入。

## 发布

修改代码后，双击 `publish.bat` 一键发布，或手动执行：

```bash
# 命令行版（约21MB，裁剪）
dotnet publish ExcelTool/ExcelTool.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

# GUI版（约159MB）
dotnet publish ExcelToolGUI/ExcelToolGUI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish-gui
```

## 开发环境

- .NET 8.0 SDK
- 依赖：EPPlus 7.5.1、Newtonsoft.Json 13.0.3
