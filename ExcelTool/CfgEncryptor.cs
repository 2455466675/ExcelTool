using System.Text;

namespace ExcelTool
{
    /// <summary>
    /// 配置文件加密器（可扩展不同加密算法）
    /// </summary>
    internal static class CfgEncryptor
    {
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="data">原始字节数据</param>
        /// <param name="key">密钥字符串</param>
        /// <returns>加密后的字节数据</returns>
        public static byte[] Encrypt(byte[] data, string key)
        {
            return XorTransform(data, key);
        }

        /// <summary>
        /// 解密数据（当前算法与加密相同）
        /// </summary>
        /// <param name="data">加密后的字节数据</param>
        /// <param name="key">密钥字符串</param>
        /// <returns>解密后的字节数据</returns>
        public static byte[] Decrypt(byte[] data, string key)
        {
            return XorTransform(data, key);
        }

        /// <summary>
        /// 异或变换（加密和解密共用）
        /// </summary>
        private static byte[] XorTransform(byte[] data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            return result;
        }
    }
}
