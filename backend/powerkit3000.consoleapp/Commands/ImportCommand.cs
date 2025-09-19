using powerkit3000.core.services;
using powerkit3000.data;
using Microsoft.Extensions.Logging;

namespace consoleapp.Commands
{
    public class ImportCommand
    {
        private readonly KickstarterDataImportService _importService;
        private readonly ILogger<Program> _logger;

        public ImportCommand(KickstarterDataImportService importService, ILogger<Program> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        public async Task ExecuteAsync(string path)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"正在从文件导入数据：{path}");
                var progressIndicator = new Progress<int>(percent =>
                {
                    Console.WriteLine($"导入进度: {percent}%");
                });
                await _importService.ImportDataAsync(path, progressIndicator);
                Console.WriteLine("数据导入成功。");
            }
            else if (Directory.Exists(path))
            {
                Console.WriteLine($"正在从目录 '{path}' 导入数据。");
                var jsonFiles = Directory.GetFiles(path, "*.json");
                if (jsonFiles.Length == 0)
                {
                    Console.WriteLine($"目录 '{path}' 中没有找到 JSON 文件。");
                    return;
                }

                int totalFiles = jsonFiles.Length;
                int filesProcessed = 0;

                foreach (var filePath in jsonFiles)
                {
                    Console.WriteLine($"正在导入文件：{filePath}");
                    var fileProgressIndicator = new Progress<int>(percent =>
                    {
                        Console.WriteLine($"文件 '{Path.GetFileName(filePath)}' 导入进度: {percent}%");
                    });
                    await _importService.ImportDataAsync(filePath, fileProgressIndicator);
                    filesProcessed++;
                    Console.WriteLine($"已导入 {filesProcessed} / {totalFiles} 个文件。总进度: {(int)((double)filesProcessed / totalFiles * 100)}%");
                }
                Console.WriteLine("所有文件导入完成。");
            }
            else
            {
                Console.WriteLine($"错误: 路径 '{path}' 不存在或不是有效的文件/目录。");
            }
        }
    }
}