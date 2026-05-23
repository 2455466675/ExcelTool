using System.Security.Cryptography;
using System.Text;

namespace ExcelTool
{
    /// <summary>
    /// 加密算法类型
    /// </summary>
    public enum EncryptionAlgorithm
    {
        /// <summary>
        /// 无加密
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 异或加密
        /// </summary>
        XOR = 1,
        
        /// <summary>
        /// AES加密
        /// </summary>
        AES = 2,
        
        /// <summary>
        /// 简单位移加密
        /// </summary>
        Shift = 3
    }

    /// <summary>
    /// 配置文件加密器（支持多种加密算法）
    /// </summary>
    public static class CfgEncryptor
    {
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="data">原始字节数据</param>
        /// <param name="key">密钥字符串</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns>加密后的字节数据</returns>
        public static byte[] Encrypt(byte[] data, string key, EncryptionAlgorithm algorithm = EncryptionAlgorithm.XOR)
        {
            if (string.IsNullOrEmpty(key) || algorithm == EncryptionAlgorithm.None)
                return data;

            return algorithm switch
            {
                EncryptionAlgorithm.XOR => XorTransform(data, key),
                EncryptionAlgorithm.AES => AesTransform(data, key, true),
                EncryptionAlgorithm.Shift => ShiftTransform(data, key, true),
                _ => XorTransform(data, key)
            };
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="data">加密后的字节数据</param>
        /// <param name="key">密钥字符串</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns>解密后的字节数据</returns>
        public static byte[] Decrypt(byte[] data, string key, EncryptionAlgorithm algorithm = EncryptionAlgorithm.XOR)
        {
            if (string.IsNullOrEmpty(key) || algorithm == EncryptionAlgorithm.None)
                return data;

            return algorithm switch
            {
                EncryptionAlgorithm.XOR => XorTransform(data, key),
                EncryptionAlgorithm.AES => AesTransform(data, key, false),
                EncryptionAlgorithm.Shift => ShiftTransform(data, key, false),
                _ => XorTransform(data, key)
            };
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

        /// <summary>
        /// AES变换
        /// </summary>
        private static byte[] AesTransform(byte[] data, string key, bool encrypt)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey(key, 32); // 256-bit key
            aes.IV = DeriveKey(key + "IV", 16); // 128-bit IV
            
            using var transform = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor();
            return transform.TransformFinalBlock(data, 0, data.Length);
        }

        /// <summary>
        /// 位移变换
        /// </summary>
        private static byte[] ShiftTransform(byte[] data, string key, bool encrypt)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                byte keyByte = keyBytes[i % keyBytes.Length];
                int shift = keyByte % 7 + 1; // 1-7位位移
                
                if (encrypt)
                    result[i] = (byte)((data[i] << shift) | (data[i] >> (8 - shift)));
                else
                    result[i] = (byte)((data[i] >> shift) | (data[i] << (8 - shift)));
            }
            
            return result;
        }

        /// <summary>
        /// 从字符串派生密钥
        /// </summary>
        private static byte[] DeriveKey(string key, int size)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            
            // 如果需要的长度小于哈希长度，截取；如果大于，扩展
            if (size <= hash.Length)
                return hash[..size];
            
            // 扩展密钥
            byte[] result = new byte[size];
            int copied = 0;
            while (copied < size)
            {
                int toCopy = Math.Min(hash.Length, size - copied);
                Array.Copy(hash, 0, result, copied, toCopy);
                copied += toCopy;
                
                // 如果需要更多数据，重新哈希
                if (copied < size)
                    hash = sha256.ComputeHash(hash);
            }
            
            return result;
        }
    }
}
