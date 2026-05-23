using System;
using System.IO;

namespace GameConfig
{
    class DecryptExample
    {
        static void Main()
        {
            Console.WriteLine("解密脚本使用示例");
            Console.WriteLine("==================");
            
            // 假设我们有一个加密的配置文件
            string encryptedFile = "cfg.bytes";
            string key = "MySecretKey123";
            
            // 检查文件是否存在
            if (!File.Exists(encryptedFile))
            {
                Console.WriteLine($"文件 {encryptedFile} 不存在，创建一个示例文件...");
                CreateSampleEncryptedFile(encryptedFile, key);
            }
            
            // 读取加密文件
            byte[] encryptedData = File.ReadAllBytes(encryptedFile);
            Console.WriteLine($"加密文件大小: {encryptedData.Length} 字节");
            
            // 使用不同的算法解密
            Console.WriteLine("\n使用不同算法解密:");
            Console.WriteLine("------------------");
            
            // XOR解密
            byte[] xorDecrypted = CfgDecryptor.Decrypt(encryptedData, key, EncryptionAlgorithm.XOR);
            Console.WriteLine($"XOR解密后大小: {xorDecrypted.Length} 字节");
            
            // AES解密
            try
            {
                byte[] aesDecrypted = CfgDecryptor.Decrypt(encryptedData, key, EncryptionAlgorithm.AES);
                Console.WriteLine($"AES解密后大小: {aesDecrypted.Length} 字节");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AES解密失败: {ex.Message}");
            }
            
            // Shift解密
            byte[] shiftDecrypted = CfgDecryptor.Decrypt(encryptedData, key, EncryptionAlgorithm.Shift);
            Console.WriteLine($"Shift解密后大小: {shiftDecrypted.Length} 字节");
            
            Console.WriteLine("\n解密脚本已成功生成在Core文件夹下的CfgDecryptor.cs文件中！");
            Console.WriteLine("该文件提供了Decrypt方法，参数为(byte[] data, string key, EncryptionAlgorithm algorithm)");
        }
        
        static void CreateSampleEncryptedFile(string filePath, string key)
        {
            // 创建一些示例数据
            string sampleData = "这是一个示例配置文件数据。";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(sampleData);
            
            // 使用XOR加密（默认算法）
            byte[] encrypted = CfgEncryptor.Encrypt(data, key, EncryptionAlgorithm.XOR);
            
            // 保存文件
            File.WriteAllBytes(filePath, encrypted);
            Console.WriteLine($"已创建示例加密文件: {filePath}");
        }
    }
}