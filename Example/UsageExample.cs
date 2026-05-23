/*
 * ============================================================
 * 配置读取使用示例（此文件仅供参考，不参与编译）
 * 
 * 展示如何在 Unity 项目中加载和使用 ExcelTool 导出的配置数据
 * ============================================================
 */
using System.IO;
using UnityEngine;

namespace Game.Cfg
{
    public class CfgManager
    {
        public static GameCfgData CfgData { get; private set; }

        /// <summary>
        /// 初始化配置（游戏启动时调用一次）
        /// </summary>
        public static void Init()
        {
            CfgData = new GameCfgData();
            string filePath = Path.Combine(Application.streamingAssetsPath, "cfg.bytes");

            byte[] data = File.ReadAllBytes(filePath);

            // 如果导出时配置了encryptKey，这里需要用相同的密钥解密
            // string encryptKey = "your-secret-key";
            // for (int i = 0; i < data.Length; i++)
            //     data[i] ^= (byte)encryptKey[i % encryptKey.Length];

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
        /// 示例1：int主键查找（最常用，零GC分配）
        /// </summary>
        public static void Example_FindByIntId()
        {
            var itemContainer = CfgData.GetContainer<ItemCfg>();

            // 直接用int查找 → 走IntKeyMap.TryGetValue，零分配
            ItemCfg sword = itemContainer.Find(1001);

            if (sword != null)
            {
                Debug.Log($"道具: {sword.Name}, 价格: {sword.Price}");
            }
        }

        /// <summary>
        /// 示例2：string主键查找（零GC分配）
        /// </summary>
        public static void Example_FindByStringId()
        {
            var skillContainer = CfgData.GetContainer<SkillCfg>();

            // 直接用string查找 → 走StrKeyMap.TryGetValue，零分配
            SkillCfg skill = skillContainer.Find("atk_001");

            if (skill != null)
            {
                Debug.Log($"技能: {skill.Name}");
            }
        }

        /// <summary>
        /// 示例3：没有Id列的表（自动生成索引Id，string主键）
        /// </summary>
        public static void Example_FindAutoId()
        {
            var tipsContainer = CfgData.GetContainer<TipsCfg>();

            // 用string索引查找 → 走StrKeyMap，零分配
            TipsCfg firstTip = tipsContainer.Find("0");

            // 用int也行 → 降级为ToString后查StrKeyMap（有一次小分配）
            TipsCfg secondTip = tipsContainer.Find(1);
        }

        /// <summary>
        /// 示例4：条件查找
        /// </summary>
        public static void Example_FindByCondition()
        {
            var itemContainer = CfgData.GetContainer<ItemCfg>();

            // 查找第一个匹配
            ItemCfg expensive = itemContainer.Find(x => x.Price > 500);

            // 查找所有匹配
            var stackableItems = itemContainer.FindAll(x => x.Stackable);
            Debug.Log($"可堆叠道具: {stackableItems.Count}个");
        }

        /// <summary>
        /// 示例5：获取全部配置
        /// </summary>
        public static void Example_GetAll()
        {
            var itemContainer = CfgData.GetContainer<ItemCfg>();
            var allItems = itemContainer.FindAll();
            Debug.Log($"道具总数: {allItems.Count}");
        }

        /// <summary>
        /// 示例6：多语言文本
        /// </summary>
        public static void Example_Language()
        {
            var langContainer = CfgData.GetContainer<LanguageCfg>();
            LanguageCfg text = langContainer.Find(10001);  // int主键，零分配
            if (text != null)
            {
                Debug.Log(text.Text);
            }
        }
    }
}
