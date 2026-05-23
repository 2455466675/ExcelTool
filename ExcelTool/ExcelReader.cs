using OfficeOpenXml;

namespace ExcelTool
{
    /// <summary>
    /// Excel读取相关方法
    /// </summary>
    internal static class ExcelReader
    {
        /// <summary>
        /// 读取Excel表头（第1行字段名，第3行字段类型）
        /// </summary>
        public static List<(string, string)> ReadHead(ExcelWorksheet worksheet, int colCount)
        {
            var head = new List<(string, string)>();
            for (int col = 1; col <= colCount; col++)
            {
                var nameCell = worksheet.Cells[1, col].Value;
                var typeCell = worksheet.Cells[3, col].Value;
                if (nameCell == null || typeCell == null) continue;

                string propName = nameCell.ToString()!;
                propName = char.ToUpper(propName[0]) + propName[1..];
                string valueType = typeCell.ToString()!.ToLower();
                head.Add((propName, valueType));
            }
            return head;
        }

        /// <summary>
        /// 构建字段名→列号映射
        /// </summary>
        public static Dictionary<string, int> BuildColumnMap(ExcelWorksheet worksheet, int colCount)
        {
            var colMap = new Dictionary<string, int>();
            for (int col = 1; col <= colCount; col++)
            {
                var nameCell = worksheet.Cells[1, col].Value;
                var typeCell = worksheet.Cells[3, col].Value;
                if (nameCell == null || typeCell == null) continue;
                string propName = nameCell.ToString()!;
                propName = char.ToUpper(propName[0]) + propName[1..];
                colMap[propName] = col;
            }
            return colMap;
        }

        /// <summary>
        /// 读取Excel数据行（第5行起，用于单个导出）
        /// </summary>
        public static void ReadDataRows(ExcelWorksheet worksheet, int rowCount, int colCount,
            List<(string, string)> values, bool hasIdInExcel, string typeName)
        {
            int autoIndex = 0;
            for (int row = 5; row <= rowCount; row++)
            {
                if (!hasIdInExcel)
                {
                    values.Add(("string", $"{typeName}_{autoIndex}"));
                    autoIndex++;
                }

                for (int col = 1; col <= colCount; col++)
                {
                    var nameCell = worksheet.Cells[1, col].Value;
                    var typeCell = worksheet.Cells[3, col].Value;
                    if (nameCell == null || typeCell == null) continue;

                    string valueType = typeCell.ToString()!.ToLower();
                    string value;

                    if (worksheet.Cells[row, col].Value == null)
                    {
                        value = TypeHelper.GetDefaultValue(valueType);
                    }
                    else
                    {
                        value = worksheet.Cells[row, col].Value.ToString() ?? "";
                        if (valueType == "bool")
                        {
                            value = string.Equals(value, "0") ||
                                    string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
                                ? "false" : "true";
                        }
                    }
                    values.Add((valueType, value));
                }
            }
        }

        /// <summary>
        /// 按统一表头顺序读取数据行（用于合并导出）
        /// </summary>
        public static void ReadDataRowsMerged(ExcelWorksheet worksheet, int rowCount,
            List<(string, string)> mergedHead, Dictionary<string, int> colMap,
            List<(string, string)> values, bool hasIdInMergedHead, string typeName)
        {
            for (int row = 5; row <= rowCount; row++)
            {
                for (int fieldIdx = 0; fieldIdx < mergedHead.Count; fieldIdx++)
                {
                    var (fieldName, fieldType) = mergedHead[fieldIdx];

                    if (fieldName == "Id" && !hasIdInMergedHead)
                    {
                        int currentRowIndex = (values.Count - fieldIdx) / mergedHead.Count;
                        values.Add(("string", $"{typeName}_{currentRowIndex}"));
                        continue;
                    }

                    if (colMap.TryGetValue(fieldName, out int col))
                    {
                        string value;
                        if (worksheet.Cells[row, col].Value == null)
                        {
                            value = TypeHelper.GetDefaultValue(fieldType);
                        }
                        else
                        {
                            value = worksheet.Cells[row, col].Value.ToString() ?? "";
                            if (fieldType == "bool")
                            {
                                value = string.Equals(value, "0") ||
                                        string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
                                    ? "false" : "true";
                            }
                        }
                        values.Add((fieldType, value));
                    }
                    else
                    {
                        values.Add((fieldType, TypeHelper.GetDefaultValue(fieldType)));
                    }
                }
            }
        }
    }
}
