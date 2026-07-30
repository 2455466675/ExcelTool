/*
 * ============================================================
 * 配置读取使用示例（此文件仅供参考，不参与编译）
 *
 * 展示如何在 Unity / .NET 项目中加载并使用 ExcelTool 导出的配置数据。
 * 对应的数据类见 Model/ 目录，基础设施见 Core/ 目录。
 * ============================================================
 */
using System.IO;
using UnityEngine;

namespace Game.Cfg
{
    public static class CfgManager
    {
        public static GameCfgData CfgData { get; private set; }

        // 导出时如果设置了 encryptKey，这里要填相同的密钥与算法；否则保持 None。
        private const string EncryptKey = "";
        private const EncryptionAlgorithm Algorithm = EncryptionAlgorithm.None;

        /// <summary>
        /// 初始化配置（游戏启动时调用一次）
        /// </summary>
        public static void Init()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "cfg.bytes");
            byte[] data = File.ReadAllBytes(filePath);

            // 若导出时启用了加密，使用生成的 CfgDecryptor 解密（密钥/算法需与导出时一致）
            if (!string.IsNullOrEmpty(EncryptKey) && Algorithm != EncryptionAlgorithm.None)
                data = CfgDecryptor.Decrypt(data, EncryptKey, Algorithm);

            CfgData = new GameCfgData();
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                CfgData.Deserialize(reader);
            }

            Debug.Log("配置加载完成");
        }

        // ============================================================
        // 查找示例
        // ============================================================

        /// <summary>
        /// 示例1：int 主键查找（最常用，走 IntKeyMap，零分配）
        /// 对应 Model/ItemCfg.cs，Excel 中 Id 列类型为 int
        /// </summary>
        public static void Example_FindByIntId()
        {
            var itemContainer = CfgData.GetContainer<ItemCfg>();

            ItemCfg sword = itemContainer.Find(1001);
            if (sword != null)
            {
                Debug.Log($"道具: {sword.Name}, 价格: {sword.Price}");
                Debug.Log($"标签[0]: {sword.Tags[0]}");      // 数组字段
                Debug.Log($"攻击力: {sword.Attrs["atk"]}");  // 字典字段
            }
        }

        /// <summary>
        /// 示例2：string 主键查找（走 StrKeyMap，零分配）
        /// 对应 Model/SkillCfg.cs，Excel 中 Id 列类型为 string
        /// </summary>
        public static void Example_FindByStringId()
        {
            var skillContainer = CfgData.GetContainer<SkillCfg>();

            SkillCfg skill = skillContainer.Find("atk_001");
            if (skill != null)
                Debug.Log($"技能: {skill.Name}, 伤害: {skill.Damage}");
        }

        /// <summary>
        /// 示例3：没有 Id 列的表（工具自动生成 string 主键，值为 "类型名_索引"）
        /// 对应 Model/TipsCfg.cs，第一条记录的 Id 为 "TipsCfg_0"
        /// </summary>
        public static void Example_FindAutoId()
        {
            var tipsContainer = CfgData.GetContainer<TipsCfg>();

            TipsCfg firstTip = tipsContainer.Find("TipsCfg_0");
            if (firstTip != null)
                Debug.Log($"第一条提示: {firstTip.Content}");
        }

        /// <summary>
        /// 示例4：条件查找（Find / FindAll 接受谓词）
        /// </summary>
        public static void Example_FindByCondition()
        {
            var itemContainer = CfgData.GetContainer<ItemCfg>();

            // 第一个匹配
            ItemCfg expensive = itemContainer.Find(x => x.Price > 500);

            // 所有匹配（返回 ItemCfg[]）
            ItemCfg[] stackableItems = itemContainer.FindAll(x => x.Stackable);
            Debug.Log($"可堆叠道具: {stackableItems.Length} 个");
        }

        /// <summary>
        /// 示例5：获取全部配置（返回数组）
        /// </summary>
        public static void Example_GetAll()
        {
            var itemContainer = CfgData.GetContainer<ItemCfg>();
            ItemCfg[] allItems = itemContainer.FindAll();
            Debug.Log($"道具总数: {allItems.Length}");
        }

        /// <summary>
        /// 示例6：多语言文本（合并导出文件夹的示例，int 主键）
        /// 对应 Model/LanguageCfg.cs
        /// </summary>
        public static void Example_Language()
        {
            var langContainer = CfgData.GetContainer<LanguageCfg>();
            LanguageCfg text = langContainer.Find(10001);
            if (text != null)
                Debug.Log($"中文: {text.Cn}, 英文: {text.En}");
        }
    }
}
