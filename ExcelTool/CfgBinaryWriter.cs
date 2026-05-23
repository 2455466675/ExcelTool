using System.Text;

namespace ExcelTool
{
    /// <summary>
    /// 二进制配置文件写入器
    /// </summary>
    internal static class CfgBinaryWriter
    {
        /// <summary>
        /// 将配置数据写入二进制字节文件
        /// </summary>
        public static void Write(List<(string, string)> values, string filePath, string encryptKey = "", EncryptionAlgorithm algorithm = EncryptionAlgorithm.XOR)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var memStream = new MemoryStream();
            using (var writer = new BinaryWriter(memStream, Encoding.UTF8, leaveOpen: true))
            {
                for (int idx = 0; idx < values.Count; idx++)
                {
                    var (type, raw) = values[idx];
                    switch (type)
                    {
                        case "int":
                            if (!int.TryParse(raw, out int intVal))
                            {
                                Console.WriteLine($"  警告：值 \"{raw}\" 无法转为int，已使用默认值0 (索引:{idx})");
                                intVal = 0;
                            }
                            writer.Write(intVal);
                            break;
                        case "bool":
                            writer.Write(raw == "true");
                            break;
                        case "string":
                            writer.Write(raw);
                            break;
                        case "int[]":
                            WriteIntArray(writer, raw, idx);
                            break;
                        case "string[]":
                            WriteStringArray(writer, raw);
                            break;
                        case "bool[]":
                            WriteBoolArray(writer, raw);
                            break;
                        default:
                            if (TypeHelper.TryParseDictType(type, out string dictKeyType, out string dictValType))
                                WriteDict(writer, raw, dictKeyType, dictValType, idx);
                            else
                            {
                                Console.WriteLine($"  警告：未知类型 \"{type}\"，值 \"{raw}\" 将作为string写入 (索引:{idx})");
                                writer.Write(raw);
                            }
                            break;
                    }
                }
                writer.Flush();
            }

            // 获取原始字节数据
            byte[] data = memStream.ToArray();

            // 如果配置了加密密钥，进行加密
            if (!string.IsNullOrEmpty(encryptKey))
            {
                data = CfgEncryptor.Encrypt(data, encryptKey, algorithm);
                Console.WriteLine($"  已加密（算法: {algorithm}, 密钥长度: {Encoding.UTF8.GetByteCount(encryptKey)}字节）");
            }

            File.WriteAllBytes(filePath, data);
        }

        private static void WriteIntArray(BinaryWriter writer, string raw, int idx)
        {
            if (string.IsNullOrEmpty(raw)) { writer.Write(0); return; }
            var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            writer.Write(parts.Length);
            foreach (var p in parts)
            {
                if (!int.TryParse(p.Trim(), out int val))
                {
                    Console.WriteLine($"  警告：int[]元素 \"{p.Trim()}\" 无法转为int，已使用默认值0 (索引:{idx})");
                    val = 0;
                }
                writer.Write(val);
            }
        }

        private static void WriteStringArray(BinaryWriter writer, string raw)
        {
            if (string.IsNullOrEmpty(raw)) { writer.Write(0); return; }
            var parts = raw.Split(',');
            writer.Write(parts.Length);
            foreach (var p in parts) writer.Write(p.Trim());
        }

        private static void WriteBoolArray(BinaryWriter writer, string raw)
        {
            if (string.IsNullOrEmpty(raw)) { writer.Write(0); return; }
            var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            writer.Write(parts.Length);
            foreach (var p in parts)
            {
                string v = p.Trim();
                writer.Write(!(string.Equals(v, "0") || string.Equals(v, "false", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(v)));
            }
        }

        private static void WriteDict(BinaryWriter writer, string raw, string dictKeyType, string dictValType, int idx)
        {
            if (string.IsNullOrEmpty(raw)) { writer.Write(0); return; }
            var pairs = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            writer.Write(pairs.Length);
            foreach (var pair in pairs)
            {
                int colonIdx = pair.IndexOf(':');
                if (colonIdx < 0)
                {
                    Console.WriteLine($"  警告：字典项 \"{pair}\" 缺少冒号分隔符，已跳过 (索引:{idx})");
                    WriteDefault(writer, dictKeyType);
                    WriteDefault(writer, dictValType);
                    continue;
                }
                WriteTyped(writer, dictKeyType, pair[..colonIdx].Trim(), idx);
                WriteTyped(writer, dictValType, pair[(colonIdx + 1)..].Trim(), idx);
            }
        }

        private static void WriteTyped(BinaryWriter writer, string valueType, string raw, int idx)
        {
            switch (valueType)
            {
                case "int":
                    if (!int.TryParse(raw, out int intVal))
                    {
                        Console.WriteLine($"  警告：值 \"{raw}\" 无法转为int，已使用默认值0 (索引:{idx})");
                        intVal = 0;
                    }
                    writer.Write(intVal);
                    break;
                case "bool":
                    writer.Write(!(string.Equals(raw, "0") || string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(raw)));
                    break;
                case "string":
                    writer.Write(raw);
                    break;
            }
        }

        private static void WriteDefault(BinaryWriter writer, string valueType)
        {
            switch (valueType)
            {
                case "int": writer.Write(0); break;
                case "bool": writer.Write(false); break;
                case "string": writer.Write(""); break;
            }
        }
    }
}
