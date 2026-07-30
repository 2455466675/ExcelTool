using OfficeOpenXml;

namespace ExcelTool
{
    /// <summary>
    /// 主生成流程编排
    /// </summary>
    internal static class Generator
    {
        /// <summary>
        /// 主生成流程：同时生成配置脚本（Core/Model）和字节文件
        /// </summary>
        public static void Generate(CfgModel model)
        {
            GenerateInternal(model, bytesOnly: false);
        }

        /// <summary>
        /// 仅生成字节文件（cfg.bytes），不生成任何 .cs 脚本
        /// </summary>
        public static void GenerateBytesOnly(CfgModel model)
        {
            GenerateInternal(model, bytesOnly: true);
        }

        /// <summary>
        /// 生成流程的内部实现
        /// </summary>
        /// <param name="bytesOnly">为 true 时只生成字节文件，跳过所有脚本生成与脚本目录操作</param>
        private static void GenerateInternal(CfgModel model, bool bytesOnly)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string ns = model.Namespace;

            // 创建临时文件夹用于生成
            string tempBase = Path.Combine(Path.GetTempPath(), $"ExcelTool_{Guid.NewGuid():N}");
            string tempCorePath = Path.Combine(tempBase, "Core");
            string tempModelPath = Path.Combine(tempBase, "Model");
            string tempBytesPath = Path.Combine(tempBase, "Bytes");

            Directory.CreateDirectory(tempCorePath);
            Directory.CreateDirectory(tempModelPath);
            Directory.CreateDirectory(tempBytesPath);

            try
            {
                // 1. 生成基础接口和容器类（仅完整模式）
                if (!bytesOnly)
                    CodeGenerator.GenerateInterface(tempCorePath, ns);

                // 2. 收集所有配置类型名和主键类型
                var allTypes = new List<(string typeName, string keyType)>();
                var allValues = new List<(string, string)>();

                // 3. 处理合并导出的文件夹
                if (model.mergedFolders != null)
                {
                    foreach (var mergedItem in model.mergedFolders)
                    {
                        var result = ProcessMergedFolder(model, mergedItem, ns, tempModelPath, bytesOnly);
                        if (result == null) continue;
                        var (values, keyType) = result.Value;
                        allValues.AddRange(values);
                        allTypes.Add((mergedItem.type, keyType));
                    }
                }

                // 4. 处理单个导出的Excel文件
                if (model.singleFiles != null)
                {
                    foreach (var singleItem in model.singleFiles)
                    {
                        var result = ProcessSingleFile(model, singleItem, ns, tempModelPath, bytesOnly);
                        if (result == null) continue;
                        var (values, keyType) = result.Value;
                        allValues.AddRange(values);
                        allTypes.Add((singleItem.type, keyType));
                    }
                }

                // 5. 生成GameCfgData总管理类（仅完整模式）
                if (!bytesOnly)
                    CodeGenerator.GenerateCfgMap(tempCorePath, allTypes, ns);

                // 6. 写入二进制文件
                string tempBytesFile = Path.Combine(tempBytesPath, "cfg.bytes");
                CfgBinaryWriter.Write(allValues, tempBytesFile, model.encryptKey, model.EncryptionAlgorithm);

                // 7. 生成完成，移动输出文件
                if (!bytesOnly)
                {
                    // 完整模式：清空脚本目标目录并移动 Core / Model 脚本
                    string targetCorePath = Path.Combine(model.scriptOutputPath, "Core");
                    string targetModelPath = Path.Combine(model.scriptOutputPath, "Model");

                    if (Directory.Exists(model.scriptOutputPath))
                    {
                        Directory.Delete(model.scriptOutputPath, true);
                    }

                    Directory.CreateDirectory(targetCorePath);
                    Directory.CreateDirectory(targetModelPath);
                    MoveOutputFiles(tempCorePath, targetCorePath);
                    MoveOutputFiles(tempModelPath, targetModelPath);
                }

                // 字节文件始终输出
                MoveOutputFiles(tempBytesPath, model.bytesOutputPath);

                if (!bytesOnly)
                    Console.WriteLine($"脚本输出: {model.scriptOutputPath}");
                Console.WriteLine($"写入字节文件: {Path.Combine(model.bytesOutputPath, "cfg.bytes")}");
            }
            finally
            {
                if (Directory.Exists(tempBase))
                {
                    Directory.Delete(tempBase, true);
                }
            }
        }

        /// <summary>
        /// 处理合并导出的文件夹
        /// </summary>
        private static (List<(string, string)> values, string keyType)? ProcessMergedFolder(
            CfgModel model, MergedFolderItem item, string ns, string modelOutputPath, bool bytesOnly)
        {
            var values = new List<(string, string)>();

            string folderPath = Path.Combine(model.excelPath, item.folder);
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"警告：合并文件夹不存在，跳过 - {folderPath}");
                return null;
            }

            string[] excelFiles = Directory.GetFiles(folderPath, "*.xlsx", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).StartsWith('~'))
                .ToArray();

            if (excelFiles.Length == 0)
            {
                Console.WriteLine($"警告：合并文件夹中没有xlsx文件，跳过 - {folderPath}");
                return null;
            }

            // 第一遍：扫描所有文件的表头，合并为统一字段列表
            var mergedHead = new List<(string, string)>();
            var fieldNames = new HashSet<string>();
            string firstFile = "";

            for (int i = 0; i < excelFiles.Length; i++)
            {
                string filePath = Path.GetFullPath(excelFiles[i]);
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var package = new ExcelPackage(stream);

                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                var fileHead = ExcelReader.ReadHead(worksheet, worksheet.Dimension.Columns);

                if (i == 0)
                {
                    firstFile = Path.GetFileName(filePath);
                    mergedHead.AddRange(fileHead);
                    foreach (var h in fileHead) fieldNames.Add(h.Item1);
                }
                else
                {
                    foreach (var h in fileHead)
                    {
                        if (!fieldNames.Contains(h.Item1))
                        {
                            fieldNames.Add(h.Item1);
                            mergedHead.Add(h);
                            Console.WriteLine($"  警告：[{item.type}] 文件 {Path.GetFileName(filePath)} " +
                                $"包含额外字段 \"{h.Item1}\"({h.Item2})，已合并到统一结构中" +
                                $"（首个文件 {firstFile} 中无此字段，将填充默认值）");
                        }
                    }
                }
            }

            bool hasIdInMergedHead = mergedHead.Any(h => h.Item1 == "Id");
            mergedHead = TypeHelper.EnsureIdField(mergedHead);
            string keyType = TypeHelper.GetKeyType(mergedHead);

            // 第二遍：按统一表头读取所有文件的数据
            int totalCount = 0;

            for (int i = 0; i < excelFiles.Length; i++)
            {
                string filePath = Path.GetFullPath(excelFiles[i]);
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var package = new ExcelPackage(stream);

                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                totalCount += Math.Max(0, rowCount - 4);

                var colMap = ExcelReader.BuildColumnMap(worksheet, colCount);
                ExcelReader.ReadDataRowsMerged(worksheet, rowCount, mergedHead, colMap, values, hasIdInMergedHead, item.type);
            }

            if (!bytesOnly)
                CodeGenerator.GenerateCfgCs(item.type, modelOutputPath, mergedHead, ns);
            Console.WriteLine($"[合并导出] {item.type} - {excelFiles.Length}个文件, {totalCount}条数据, 主键类型: {keyType}");

            values.Insert(0, ("int", totalCount.ToString()));
            return (values, keyType);
        }

        /// <summary>
        /// 处理单个导出的Excel文件
        /// </summary>
        private static (List<(string, string)> values, string keyType)? ProcessSingleFile(
            CfgModel model, SingleFileItem item, string ns, string modelOutputPath, bool bytesOnly)
        {
            var values = new List<(string, string)>();

            string filePath = Path.Combine(model.excelPath, item.excel);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"警告：Excel文件不存在，跳过 - {filePath}");
                return null;
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var package = new ExcelPackage(stream);

            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            int dataCount = Math.Max(0, rowCount - 4);
            values.Add(("int", dataCount.ToString()));

            var head = ExcelReader.ReadHead(worksheet, colCount);
            bool hasIdInExcel = head.Any(h => h.Item1 == "Id");
            ExcelReader.ReadDataRows(worksheet, rowCount, colCount, values, hasIdInExcel, item.type);

            head = TypeHelper.EnsureIdField(head);
            string keyType = TypeHelper.GetKeyType(head);

            if (!bytesOnly)
                CodeGenerator.GenerateCfgCs(item.type, modelOutputPath, head, ns);
            Console.WriteLine($"[单个导出] {item.type} - {dataCount}条数据, 主键类型: {keyType}");

            return (values, keyType);
        }

        private static void MoveOutputFiles(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            string[] sourceFiles = Directory.GetFiles(sourceDir);
            var extensions = sourceFiles.Select(f => Path.GetExtension(f)).Distinct().ToHashSet();
            foreach (string existing in Directory.GetFiles(targetDir))
            {
                if (extensions.Contains(Path.GetExtension(existing)))
                    File.Delete(existing);
            }

            foreach (string file in sourceFiles)
            {
                File.Move(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }
        }
    }
}
