namespace ExcelTool
{
    /// <summary>
    /// JSON配置文件的数据模型（只包含JSON中的字段）
    /// </summary>
    internal class CfgModel
    {
        /// <summary>
        /// 加密密钥（可选，为空则不加密）
        /// </summary>
        public string encryptKey = "";

        /// <summary>
        /// 合并导出的文件夹列表
        /// </summary>
        public List<MergedFolderItem>? mergedFolders;

        /// <summary>
        /// 单个导出的Excel文件列表
        /// </summary>
        public List<SingleFileItem>? singleFiles;

        // === 以下字段由外部（GUI或命令行）传入，不在JSON中 ===

        /// <summary>
        /// Excel文件的根目录路径
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string excelPath = "";

        /// <summary>
        /// 配置脚本的导出位置
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string scriptOutputPath = "";

        /// <summary>
        /// 字节文件的导出位置
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string bytesOutputPath = "";

        /// <summary>
        /// 命名空间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string ns = "GameConfig";

        /// <summary>
        /// 获取命名空间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Namespace => string.IsNullOrWhiteSpace(ns) ? "GameConfig" : ns;

        /// <summary>
        /// 是否启用加密
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsEncrypt => !string.IsNullOrEmpty(encryptKey);
    }

    internal class MergedFolderItem
    {
        public string folder = "";
        public string type = "";
    }

    internal class SingleFileItem
    {
        public string excel = "";
        public string type = "";
    }
}
