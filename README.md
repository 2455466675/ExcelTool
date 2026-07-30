# ExcelTool 配置导出工具

把策划维护的 Excel 表格一键导出为：

- **二进制数据文件** `cfg.bytes`（紧凑、加载快，可选加密）
- **强类型 C# 读取脚本**（`Core/` 基础设施 + `Model/` 每表一个数据类/容器类）

游戏运行时通过生成的 `GameCfgData` 反序列化 `cfg.bytes`，用 `GetContainer<T>().Find(id)` 以零/低分配的方式查表。

> 生成代码的结构、字段类型、API 与完整示例见 [`Example/README.md`](Example/README.md)。

## 项目结构

```
ExcelTool/
├── ExcelTool/            ← 核心库 + 命令行程序（net8.0）
│   ├── Program.cs            命令行入口
│   ├── Generator.cs          导出主流程编排
│   ├── ExcelReader.cs        读取 Excel（EPPlus）
│   ├── CodeGenerator.cs      生成 C# 脚本
│   ├── CfgBinaryWriter.cs    写入二进制
│   ├── CfgEncryptor.cs       加密（XOR / AES / Shift）
│   ├── CfgModel.cs           JSON 配置模型
│   └── TypeHelper.cs         类型解析辅助
├── ExcelToolGUI/         ← 图形界面（net8.0-windows，WinForms）
│   └── MainForm.cs           可视化导出窗口
├── Example/             ← 生成产物示例 + 说明（仅供阅读）
├── CfgImport.json      ← 导出配置（声明要导出哪些表）
└── CfgTool.bat         ← 命令行批处理封装
```

## 两种使用方式

### 方式一：图形界面（推荐）

运行 `ExcelToolGUI`，在窗口中填写路径并选择加密算法，点击按钮导出：

- **导出配置**：生成 C# 脚本（`Core/` + `Model/`）**和** `cfg.bytes`。
- **仅生成 Bytes**：只生成 `cfg.bytes`，不生成脚本（适合表结构没变、只更新数据的场景，此时无需填写脚本输出目录）。

界面会记住上次填写的路径与算法（保存在程序目录的 `settings.json`），路径输入框支持拖拽文件/文件夹。

构建/运行：

```bat
dotnet run --project ExcelToolGUI/ExcelToolGUI.csproj
```

发布为独立 exe：

```bat
dotnet publish ExcelToolGUI/ExcelToolGUI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish-gui
```

### 方式二：命令行

```
ExcelTool.exe <jsonPath> <excelPath> <scriptOutputPath> <bytesOutputPath> [namespace]
```

| 参数 | 说明 |
|------|------|
| `jsonPath` | 导出配置 JSON（如 `CfgImport.json`） |
| `excelPath` | Excel 文件根目录 |
| `scriptOutputPath` | C# 脚本输出目录（会生成 `Core/` 与 `Model/`） |
| `bytesOutputPath` | `cfg.bytes` 输出目录 |
| `namespace` | 可选，生成代码的命名空间，默认 `GameConfig` |

发布命令行版本并用批处理调用：

```bat
dotnet publish ExcelTool/ExcelTool.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish

CfgTool.bat .\CfgImport.json
```

> `CfgTool.bat` 会调用 `publish\ExcelTool.exe`，并把传入的 JSON 路径作为参数。脚本/字节输出目录等其余参数可按需在 bat 中补充。

## 导出配置（CfgImport.json）

```json
{
    "encryptKey": "",
    "encryptAlgorithm": "XOR",
    "mergedFolders": [
        { "folder": "language/", "type": "LanguageCfg" }
    ],
    "singleFiles": [
        { "excel": "gm.xlsx", "type": "GmCfg" },
        { "excel": "battle/battle_role_action.xlsx", "type": "BattleRoleActionCfg" }
    ]
}
```

| 字段 | 含义 |
|------|------|
| `encryptKey` | 加密密钥；**留空表示不加密** |
| `encryptAlgorithm` | 加密算法：`NONE` / `XOR` / `AES` / `SHIFT`（GUI 中可下拉选择） |
| `mergedFolders` | 合并导出：把一个文件夹下的多个 `.xlsx` 合并为同一张表（`type` 为生成的类名） |
| `singleFiles` | 单文件导出：每个 `.xlsx` 生成一张表（`excel` 为相对 `excelPath` 的路径） |

- 合并导出会以第一个文件的表头为基准，自动合并其余文件的额外字段（缺失字段填默认值），并在日志中提示。
- `folder` / `excel` 路径相对于命令行/GUI 指定的 `excelPath`。

## 导出流程

```
Excel(.xlsx)
   │  EPPlus 读取（第1行字段名/第3行类型/第5行起数据）
   ▼
内存中的 (类型, 值) 列表
   ├─► CodeGenerator → Core/*.cs + Model/*.cs   （完整导出时）
   └─► CfgBinaryWriter → cfg.bytes
            │  若配置了 encryptKey
            ▼
        CfgEncryptor 加密
```

写入顺序与生成的 `GameCfgData.Deserialize` 读取顺序严格对应，因此运行时无需任何额外的格式描述即可还原数据。

## Excel 表格格式与字段类型

前 4 行为表头（字段名 / 注释 / 类型 / 保留行），第 5 行起为数据。支持 `int`、`string`、`bool`、对应数组类型，以及键为 `int`/`string`、值为 `int`/`string`/`bool` 的字典。

完整的类型对照表、主键规则、空值处理与读取 API 见 [`Example/README.md`](Example/README.md)。

## 加密说明

| 算法 | 说明 |
|------|------|
| `NONE` | 不加密 |
| `XOR` | 与密钥逐字节异或，轻量、可逆 |
| `AES` | AES-256-CBC + PKCS7；密钥由 SHA-256 派生，**每次随机生成 IV 并写入密文头部（前 16 字节）** |
| `SHIFT` | 按密钥字节做 1–7 位循环位移 |

运行时使用工具生成的 `Core/CfgDecryptor.cs`，以**相同的密钥和算法**解密后再反序列化。

> 安全提示：`XOR` 与 `SHIFT` 仅能起到混淆作用，不具备真正的保密强度；对安全性有要求时请使用 `AES`。
> 由于 AES 改用随机 IV，旧版（固定 IV）导出的 `cfg.bytes` 与新版不兼容，升级后需重新导出。

## 环境要求

- .NET 8 SDK
- 命令行库跨平台可用；**GUI 仅限 Windows**（WinForms，`net8.0-windows`）
- 依赖：[EPPlus](https://www.nuget.org/packages/EPPlus)（按 NonCommercial 许可使用）、Newtonsoft.Json
