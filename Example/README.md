# 示例文件说明

此文件夹包含工具自动生成的 C# 脚本示例，仅供阅读参考，不参与实际编译。

## Excel 表格格式约定

```
第1行：字段名（首字母会自动大写）
第2行：注释/描述（工具跳过，仅供人阅读）
第3行：字段类型
第4行：保留行（工具跳过）
第5行起：实际数据
```

## 支持的字段类型

| 类型 | C# 类型 | Excel 内容示例 |
|------|---------|---------------|
| `int` | `int` | `100` |
| `string` | `string` | `铁剑` |
| `bool` | `bool` | `0` 或 `1`（也支持 `true`/`false`） |
| `int[]` | `int[]` | `1,2,3`（逗号分隔，空值=空数组） |
| `string[]` | `string[]` | `weapon,melee`（逗号分隔） |
| `bool[]` | `bool[]` | `1,0,1`（逗号分隔） |
| `<int,int>` | `Dictionary<int, int>` | `1:100,2:200`（冒号分隔键值，逗号分隔项） |
| `<int,string>` | `Dictionary<int, string>` | `1:warrior,2:mage` |
| `<int,bool>` | `Dictionary<int, bool>` | `1:true,2:false` |
| `<string,int>` | `Dictionary<string, int>` | `atk:50,def:30` |
| `<string,string>` | `Dictionary<string, string>` | `cn:中文,en:English` |
| `<string,bool>` | `Dictionary<string, bool>` | `vip:true,guest:false` |

## 主键规则

- 主键字段名为 `Id`，支持 `int` 和 `string` 两种类型
- 如果 Excel 中没有 Id 列，自动生成 string 类型 Id，值为 `"类型名_索引"`
- `Find(int)` 仅对 int 主键有效，`Find(string)` 仅对 string 主键有效

## 生成的脚本结构

```
scriptOutputPath/
├── Core/
│   ├── IBinarySerialize.cs    ← 接口定义（ICfg公开，其余internal）
│   ├── CfgContainerBase.cs    ← 容器基类（Find/FindAll/Count等公开API）
│   └── GameCfgData.cs         ← 总管理类（Deserialize + GetContainer<T>）
└── Model/
    ├── LanguageCfg.cs          ← 数据类 + 容器类
    ├── ItemCfg.cs
    └── ...
```

## 公开 API

```csharp
// 加载
var cfgData = new GameCfgData();
cfgData.Deserialize(reader);

// 获取容器
var container = cfgData.GetContainer<ItemCfg>();

// 查找
container.Find(1001);                    // int主键查找
container.Find("atk_001");              // string主键查找
container.Find(x => x.Price > 100);     // 条件查找
container.FindAll();                     // 获取全部
container.FindAll(x => x.Stackable);    // 条件查找全部
container.GetValues();                   // 获取枚举
container.Count;                         // 数量

// 访问属性
item.Id
item.Name
item.Tags[0]
item.Attrs["atk"]
```

## 加密解密

如果导出时配置了 `encryptKey`，Unity 端加载前需解密：

```csharp
byte[] data = File.ReadAllBytes(filePath);
string key = "your-secret-key";
byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
for (int i = 0; i < data.Length; i++)
    data[i] ^= keyBytes[i % keyBytes.Length];
// 然后用 MemoryStream + BinaryReader 反序列化
```
