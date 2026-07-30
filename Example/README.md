# 生成产物示例（Example）

本文件夹是 **ExcelTool 导出后生成的 C# 脚本示例**，按工具真实输出的目录结构组织，仅供阅读参考，不参与实际编译（`UsageExample.cs` 依赖 Unity，仅作演示）。

实际运行工具时，这些文件会输出到你指定的 `脚本输出目录` 下的 `Core/` 和 `Model/` 两个子目录中。

## 目录结构

```
脚本输出目录/
├── Core/                       ← 基础设施（与具体表无关，每次固定生成）
│   ├── IBinarySerialize.cs     ← 接口定义：ICfg(公开) / IBinarySerialize(internal) / ICfgContainer(internal)
│   ├── CfgContainerBase.cs     ← 容器基类，提供 Find / FindAll / Count 等公开 API
│   ├── GameCfgData.cs          ← 总管理类，负责反序列化全部表并按类型分发容器
│   └── CfgDecryptor.cs         ← 解密器（XOR / AES / Shift），运行时按需调用
└── Model/                      ← 每张表一个文件（数据类 + 容器类）
    ├── LanguageCfg.cs          ← 合并导出文件夹示例（int 主键）
    ├── ItemCfg.cs              ← 单文件示例，含数组 + 字典字段（int 主键）
    ├── SkillCfg.cs             ← string 主键示例，含 int[] 字段
    └── TipsCfg.cs              ← 无 Id 列示例（自动生成 string 主键）
```

> 注意：`GameCfgData.cs` 中容器的注册顺序与 `CfgImport.json` 中表的声明顺序一致，
> 且 **反序列化顺序必须与导出时的写入顺序严格对应**，因此不要手动调整。

## 本示例覆盖的四种典型场景

| 文件 | 场景 | 主键 | 容器键 | 特殊字段 |
|------|------|------|--------|---------|
| `Model/ItemCfg.cs` | 单文件导出 | `int` Id | IntKeyMap | `string[] Tags`、`Dictionary<string,int> Attrs` |
| `Model/LanguageCfg.cs` | 合并文件夹导出 | `int` Id | IntKeyMap | 多个文件合并为一张表 |
| `Model/SkillCfg.cs` | 单文件导出 | `string` Id | StrKeyMap | `int[] Levels` |
| `Model/TipsCfg.cs` | 无 Id 列 | 自动 `string` Id | StrKeyMap | Id 自动填充为 `TipsCfg_0`、`TipsCfg_1` … |

## Excel 表格格式约定

每张表前 4 行是表头，第 5 行起是数据：

```
第1行：字段名（首字母会被自动大写，如 name → Name）
第2行：注释/描述（工具跳过，仅供人阅读）
第3行：字段类型（见下表，会被转为小写）
第4行：保留行（工具跳过）
第5行起：实际数据
```

示例（`item.xlsx`）：

| id | name | price | stackable | tags | attrs |
|----|------|-------|-----------|------|-------|
| 道具ID | 道具名称 | 售价 | 可否堆叠 | 标签列表 | 属性字典 |
| int | string | int | bool | string[] | &lt;string,int&gt; |
| (保留行) | | | | | |
| 1001 | 铁剑 | 100 | 0 | weapon,melee | atk:50,crit:10 |
| 1002 | 生命药水 | 50 | 1 | potion | hp:200 |
| 1003 | 金币 | 1 | 1 | | |

## 支持的字段类型

| Excel 写法 | 生成的 C# 类型 | 单元格内容示例 | 说明 |
|------------|---------------|---------------|------|
| `int` | `int` | `100` | |
| `string` | `string` | `铁剑` | |
| `bool` | `bool` | `0` / `1`（也支持 `true`/`false`） | `0`、`false`、空 视为 false |
| `int[]` | `int[]` | `1,2,3` | 逗号分隔，空值 = 空数组 |
| `string[]` | `string[]` | `weapon,melee` | 逗号分隔 |
| `bool[]` | `bool[]` | `1,0,1` | 逗号分隔 |
| `<int,int>` | `Dictionary<int,int>` | `1:100,2:200` | 冒号分隔键值，逗号分隔项 |
| `<int,string>` | `Dictionary<int,string>` | `1:warrior,2:mage` | |
| `<int,bool>` | `Dictionary<int,bool>` | `1:true,2:false` | |
| `<string,int>` | `Dictionary<string,int>` | `atk:50,def:30` | |
| `<string,string>` | `Dictionary<string,string>` | `cn:中文,en:English` | |
| `<string,bool>` | `Dictionary<string,bool>` | `vip:true,guest:false` | |

> 字典键类型仅支持 `int` / `string`；值类型仅支持 `int` / `string` / `bool`。
> 未识别的类型会按 `string` 兜底处理。

## 主键规则

- 主键字段名固定为 `Id`，支持 `int` 与 `string` 两种类型。
- 若表中没有 `Id` 列，工具会自动插入 `string` 类型的 Id，值为 `"类型名_行索引"`（如 `TipsCfg_0`），容器使用 StrKeyMap。
- `Find(int)` 仅对 int 主键有效，`Find(string)` 仅对 string 主键有效；类型不匹配返回 `null`。

## 公开 API（CfgContainerBase&lt;T&gt;）

```csharp
// 加载
var cfgData = new GameCfgData();
cfgData.Deserialize(reader);

// 获取某张表的容器
var container = cfgData.GetContainer<ItemCfg>();

// 查找
ItemCfg a = container.Find(1001);              // int 主键
SkillCfg b = skillContainer.Find("atk_001");   // string 主键
ItemCfg c = container.Find(x => x.Price > 100); // 条件查找（第一个匹配）

// 批量（注意：返回数组 T[]，不是 List<T>）
ItemCfg[] all       = container.FindAll();
ItemCfg[] stackable = container.FindAll(x => x.Stackable);

int total = container.Count;

// 访问字段
int id = a.Id;
string name = a.Name;
string firstTag = a.Tags[0];
int atk = a.Attrs["atk"];
```

> 设计上 `GetValues()` 为 `internal`，因此使用方通过 `FindAll()` 获取全部数据。

## 加密解密

导出时若设置了 `encryptKey`，`cfg.bytes` 会被加密。运行时加载前需用相同密钥与算法解密，
直接调用生成的 `CfgDecryptor`：

```csharp
byte[] data = File.ReadAllBytes(path);
data = CfgDecryptor.Decrypt(data, "your-secret-key", EncryptionAlgorithm.AES);
// 再用 MemoryStream + BinaryReader 反序列化
```

支持的算法：`None` / `XOR` / `AES` / `Shift`。

> AES 采用 CBC + PKCS7，每次加密随机生成 IV 并写入密文头部（前 16 字节），
> 解密时自动从头部提取 IV，因此密文格式与旧版固定 IV 不兼容，升级后需重新导出。

完整加载流程见同目录下的 `UsageExample.cs`。
