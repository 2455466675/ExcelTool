using System;
using System.Text;
using ExcelTool;

class TestEncryption
{
    static void Main()
    {
        Console.WriteLine("测试加密解密功能");
        Console.WriteLine("==================");
        
        string testData = "Hello, World! 这是一个测试数据。";
        string key = "MySecretKey123";
        
        byte[] originalData = Encoding.UTF8.GetBytes(testData);
        Console.WriteLine($"原始数据: {testData}");
        Console.WriteLine($"原始数据长度: {originalData.Length} 字节");
        Console.WriteLine($"密钥: {key}");
        Console.WriteLine();
        
        // 测试XOR加密
        TestAlgorithm("XOR", originalData, key, EncryptionAlgorithm.XOR);
        
        // 测试AES加密
        TestAlgorithm("AES", originalData, key, EncryptionAlgorithm.AES);
        
        // 测试Shift加密
        TestAlgorithm("Shift", originalData, key, EncryptionAlgorithm.Shift);
        
        Console.WriteLine("\n所有测试完成！");
    }
    
    static void TestAlgorithm(string name, byte[] originalData, string key, EncryptionAlgorithm algorithm)
    {
        Console.WriteLine($"\n测试 {name} 加密算法:");
        Console.WriteLine($"------------------------");
        
        // 加密
        byte[] encrypted = CfgEncryptor.Encrypt(originalData, key, algorithm);
        Console.WriteLine($"加密后数据长度: {encrypted.Length} 字节");
        Console.WriteLine($"加密后数据 (Hex): {BitConverter.ToString(encrypted).Replace("-", "")}");
        
        // 解密
        byte[] decrypted = CfgEncryptor.Decrypt(encrypted, key, algorithm);
        string decryptedText = Encoding.UTF8.GetString(decrypted);
        
        Console.WriteLine($"解密后数据: {decryptedText}");
        Console.WriteLine($"解密成功: {decryptedText == Encoding.UTF8.GetString(originalData)}");
    }
}