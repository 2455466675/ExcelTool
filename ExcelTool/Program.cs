using System.Diagnostics;
using Newtonsoft.Json;

namespace ExcelTool
{
    /// <summary>
    /// 程序入口（命令行模式）
    /// 用法: ExcelTool.exe [jsonPath] [excelPath] [scriptOutputPath] [bytesOutputPath] [namespace]
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ExcelTool 配置导出工具 ===");

            if (args == null || args.Length < 4)
            {
                Console.WriteLine("用法: ExcelTool.exe <jsonPath> <excelPath> <scriptOutputPath> <bytesOutputPath> [namespace]");
                Console.WriteLine("  jsonPath         - 配置JSON文件路径");
                Console.WriteLine("  excelPath        - Excel文件根目录");
                Console.WriteLine("  scriptOutputPath - 脚本输出目录");
                Console.WriteLine("  bytesOutputPath  - 字节文件输出目录");
                Console.WriteLine("  namespace        - 命名空间（可选，默认GameConfig）");
                return;
            }

            string jsonPath = Path.GetFullPath(args[0]);
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"错误：json文件不存在 - {jsonPath}");
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            CfgModel? model = JsonConvert.DeserializeObject<CfgModel>(jsonContent);

            if (model == null)
            {
                Console.WriteLine($"错误：配置数据反序列化失败 - {jsonPath}");
                return;
            }

            // 从命令行参数填充路径
            model.excelPath = Path.GetFullPath(args[1]);
            model.scriptOutputPath = Path.GetFullPath(args[2]);
            model.bytesOutputPath = Path.GetFullPath(args[3]);
            model.ns = args.Length >= 5 ? args[4] : "GameConfig";

            Console.WriteLine($"命名空间: {model.Namespace}");
            Console.WriteLine($"Excel根路径: {model.excelPath}");
            Console.WriteLine($"脚本输出: {model.scriptOutputPath}");
            Console.WriteLine($"字节文件输出: {model.bytesOutputPath}");

            if (!Directory.Exists(model.excelPath))
            {
                Console.WriteLine($"错误：Excel根路径不存在 - {model.excelPath}");
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                Generator.Generate(model);
            }
            catch (Exception e)
            {
                Console.WriteLine($"错误：生成过程异常\n{e}");
                return;
            }

            stopwatch.Stop();
            Console.WriteLine($"\n=== 导出完成，耗时: {stopwatch.Elapsed} ===");
        }
    }
}
