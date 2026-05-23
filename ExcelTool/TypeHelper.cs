namespace ExcelTool
{
    /// <summary>
    /// 类型相关辅助方法
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// 确保表头中有Id字段，没有则插入string类型的Id
        /// </summary>
        public static List<(string, string)> EnsureIdField(List<(string, string)> head)
        {
            if (!head.Any(h => h.Item1 == "Id"))
                head.Insert(0, ("Id", "string"));
            return head;
        }

        /// <summary>
        /// 获取主键类型（Id字段的类型）
        /// </summary>
        public static string GetKeyType(List<(string, string)> head)
        {
            var idField = head.FirstOrDefault(h => h.Item1 == "Id");
            return idField.Item2 ?? "string";
        }

        /// <summary>
        /// 获取字段类型的默认值
        /// </summary>
        public static string GetDefaultValue(string fieldType)
        {
            return fieldType switch
            {
                "int" => "0",
                "bool" => "false",
                _ => string.Empty
            };
        }

        /// <summary>
        /// 尝试解析字典类型标识，如 "<int,string>"
        /// </summary>
        public static bool TryParseDictType(string typeStr, out string keyType, out string valType)
        {
            keyType = "";
            valType = "";
            if (typeStr.StartsWith('<') && typeStr.EndsWith('>'))
            {
                string inner = typeStr[1..^1];
                int commaIdx = inner.IndexOf(',');
                if (commaIdx > 0)
                {
                    keyType = inner[..commaIdx].Trim();
                    valType = inner[(commaIdx + 1)..].Trim();
                    return (keyType == "int" || keyType == "string") &&
                           (valType == "int" || valType == "string" || valType == "bool");
                }
            }
            return false;
        }

        /// <summary>
        /// 获取类型对应的C#类型字符串
        /// </summary>
        public static string GetCsType(string fieldType)
        {
            if (TryParseDictType(fieldType, out string k, out string v))
                return $"Dictionary<{k}, {v}>";
            return fieldType;
        }

        /// <summary>
        /// 获取类型对应的BinaryReader读取方法名
        /// </summary>
        public static string GetReadMethod(string simpleType)
        {
            return simpleType switch
            {
                "int" => "ReadInt32",
                "bool" => "ReadBoolean",
                "string" => "ReadString",
                _ => "ReadString"
            };
        }
    }
}
