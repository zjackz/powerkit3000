using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace consoleapp.Commands
{
    public class SplitCommand
    {
        private readonly ILogger<Program> _logger;

        public SplitCommand(ILogger<Program> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(string filePath, int numberOfFiles)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"错误: 文件 '{filePath}' 不存在。");
                return;
            }

            if (numberOfFiles <= 0)
            {
                Console.WriteLine("错误: 拆分数量必须为正整数。");
                return;
            }

            Console.WriteLine($"正在拆分文件 '{filePath}' 为 {numberOfFiles} 个小文件。");

            try
            {
                int totalRecords = 0;
                // First pass to count total records
                using (StreamReader file = File.OpenText(filePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    reader.SupportMultipleContent = true;
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            totalRecords++;
                            JObject jsonObject = (JObject)JToken.ReadFrom(reader); // Read to advance reader
                        }
                    }
                }

                if (totalRecords == 0)
                {
                    Console.WriteLine("文件中没有找到 JSON 对象，无需拆分。");
                    return;
                }

                int recordsPerFile = Math.Max(1, (int)Math.Ceiling((double)totalRecords / numberOfFiles));

                Console.WriteLine($"总记录数: {totalRecords}，每个文件约 {recordsPerFile} 条记录。");

                int currentFileIndex = 0;
                int recordsInCurrentFile = 0;
                List<JObject> currentBatch = new List<JObject>();

                using (StreamReader file = File.OpenText(filePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    reader.SupportMultipleContent = true;
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                            currentBatch.Add(jsonObject);
                            recordsInCurrentFile++;

                            if (recordsInCurrentFile >= recordsPerFile && currentFileIndex < numberOfFiles - 1)
                            {
                                string outputFileName = BuildOutputPath(filePath, currentFileIndex + 1);
                                await File.WriteAllTextAsync(outputFileName, string.Join(Environment.NewLine, currentBatch.Select(j => j.ToString(Formatting.None))));
                                Console.WriteLine($"已创建文件: {outputFileName}，包含 {currentBatch.Count} 条记录。进度: {(int)((double)(currentFileIndex + 1) / numberOfFiles * 100)}%");

                                currentBatch.Clear();
                                recordsInCurrentFile = 0;
                                currentFileIndex++;
                            }
                        }
                    }
                }

                // Write any remaining records to the last file
                if (currentBatch.Any())
                {
                    string outputFileName = BuildOutputPath(filePath, currentFileIndex + 1);
                    await File.WriteAllTextAsync(outputFileName, string.Join(Environment.NewLine, currentBatch.Select(j => j.ToString(Formatting.None))));
                    Console.WriteLine($"已创建文件: {outputFileName}，包含 {currentBatch.Count} 条记录。进度: 100%");
                }

                Console.WriteLine("文件拆分完成。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"文件拆分过程中发生错误：{ex.Message}");
            }
        }

        private static string BuildOutputPath(string sourcePath, int partIndex)
        {
            var directory = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
            var baseName = Path.GetFileNameWithoutExtension(sourcePath);
            return Path.Combine(directory, $"{baseName}_part{partIndex}.json");
        }
    }
}
