using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using powerkit3000.data;

namespace powerkit3000.consoleapp.Tests
{
    public class consoleappTests
    {
        private StringWriter _stringWriter = null!;
        private Action<IServiceCollection> _testServiceConfig;
        private string _inMemoryDatabaseName = string.Empty;
        private TextWriter? _originalOut;
        private InMemoryDatabaseRoot _databaseRoot = null!;

        private static string GetSampleDataPath()
        {
            var samplePath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory,
                "../../../../powerkit3000.consoleapp/data/sample2.json"));
            if (!File.Exists(samplePath))
            {
                Assert.Fail($"Sample data file not found at path: {samplePath}");
            }

            return samplePath;
        }

        private static string GetDataDirectoryPath(string directoryName)
        {
            var directoryPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory,
                $"../../../../powerkit3000.consoleapp/data/{directoryName}"));
            if (!Directory.Exists(directoryPath))
            {
                Assert.Fail($"Data directory not found at path: {directoryPath}");
            }

            return directoryPath;
        }

        private static string CreateLargeDatasetFile(int recordCount)
        {
            JObject? template = null;
            var samplePath = GetSampleDataPath();

            using (var reader = File.OpenText(samplePath))
            using (var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true })
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        template = JObject.Load(jsonReader);
                        break;
                    }
                }
            }

            if (template == null)
            {
                Assert.Fail("Failed to load template Kickstarter record for generating test data.");
            }

            var tempFile = Path.Combine(Path.GetTempPath(), $"powerkit3000_large_{Guid.NewGuid():N}.json");

            using (var writer = new StreamWriter(tempFile))
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.None })
            {
                for (var i = 0; i < recordCount; i++)
                {
                    var clone = (JObject)template!.DeepClone();
                    var data = (JObject)clone["data"]!;

                    data["id"] = 10_000_000_000L + i;
                    data["name"] = $"{data.Value<string>("name")}-batch-{i:D5}";
                    data["slug"] = $"{data.Value<string>("slug")}-batch-{i:D5}";
                    data["goal"] = 5000 + i;
                    data["pledged"] = 2500 + i;
                    data["usd_pledged"] = (2500 + i).ToString();

                    if (data["creator"] is JObject creator)
                    {
                        creator["id"] = 20_000_000_000L + i;
                        creator["name"] = $"Creator {i:D5}";
                    }

                    if (data["category"] is JObject category)
                    {
                        category["id"] = 30_000_000_000L + (i % 25);
                        category["name"] = $"Category {(i % 25):D2}";
                        category["slug"] = $"category-{(i % 25):D2}";
                    }

                    if (data["location"] is JObject location)
                    {
                        location["id"] = 40_000_000_000L + (i % 50);
                        location["name"] = $"Location {(i % 50):D2}";
                        location["displayable_name"] = $"Location {(i % 50):D2}";
                    }

                    clone.WriteTo(jsonWriter);
                    jsonWriter.WriteWhitespace(Environment.NewLine);
                }
            }

            return tempFile;
        }

        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            _testServiceConfig(services);
            return services.BuildServiceProvider();
        }

        [SetUp]
        public void Setup()
        {
            _originalOut = Console.Out;
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);

            _inMemoryDatabaseName = $"powerkit3000Tests_{Guid.NewGuid()}";
            _databaseRoot = new InMemoryDatabaseRoot();
            _testServiceConfig = services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_inMemoryDatabaseName, _databaseRoot);
                });
            };
        }

        [TearDown]
        public void Teardown()
        {
            Console.SetOut(_originalOut ?? TextWriter.Null);
        }

        [Test]
        public async Task CountsCommand_Should_PrintCounts()
        {
            // Arrange
            var args = new[] { "counts" };

            // Act
            await Program.RunAppAsync(args, _testServiceConfig);

            // Assert
            var output = _stringWriter.ToString();
            Assert.That(output, Does.Contain("项目:"));
            Assert.That(output, Does.Contain("创建者:"));
            Assert.That(output, Does.Contain("类别:"));
            Assert.That(output, Does.Contain("位置:"));
        }

        [Test]
        public async Task QueryCommand_Should_ReturnFilteredProjects()
        {
            // Arrange
            var samplePath = GetSampleDataPath();
            await Program.RunAppAsync(new[] { "import", samplePath }, _testServiceConfig);

            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);
            var args = new[] { "query", "--state", "successful", "--country", "US" };

            // Act
            await Program.RunAppAsync(args, _testServiceConfig);

            // Assert
            var output = _stringWriter.ToString();
            Assert.That(output, Does.Contain("- Taylor Allyn Pop Princess Music Video"));
            Assert.That(output, Does.Contain("- Nymphonomicon #7: Tentacles of Tokyo, Part 1"));
        }

        [Test]
        public async Task ImportCommand_Should_ImportSampleJson()
        {
            // Arrange
            var clearDbArgs = new[] { "clear-db" };
            var samplePath = GetSampleDataPath();
            var importArgs = new[] { "import", samplePath };
            var countsArgs = new[] { "counts" };

            // Act - Clear database
            await Program.RunAppAsync(clearDbArgs, _testServiceConfig);

            // Act - Import sample.json
            await Program.RunAppAsync(importArgs, _testServiceConfig);

            // Act - Get counts after import
            _stringWriter = new StringWriter(); // Reset StringWriter for new output
            Console.SetOut(_stringWriter);
            await Program.RunAppAsync(countsArgs, _testServiceConfig);

            // Assert
            var output = _stringWriter.ToString();
            Assert.That(output, Does.Contain("项目: 13"));
            Assert.That(output, Does.Contain("创建者: 13"));
            Assert.That(output, Does.Contain("类别: 11"));
            Assert.That(output, Does.Contain("位置: 9"));
        }

        [Test]
        public async Task ImportCommand_Should_ImportDirectoryData()
        {
            // Arrange
            var clearDbArgs = new[] { "clear-db" };
            var directoryPath = GetDataDirectoryPath("data-1");
            var importArgs = new[] { "import", directoryPath };

            await Program.RunAppAsync(clearDbArgs, _testServiceConfig);

            // Act
            var stopwatch = Stopwatch.StartNew();
            await Program.RunAppAsync(importArgs, _testServiceConfig);
            stopwatch.Stop();

            using var provider = BuildServiceProvider();
            using var context = provider.GetRequiredService<AppDbContext>();
            var projectCount = await context.KickstarterProjects.CountAsync();

            Assert.That(projectCount, Is.GreaterThan(0), "导入目录后应存在项目记录。");
            TestContext.WriteLine($"data-1 目录导入耗时: {stopwatch.Elapsed}。");
        }

        [Test]
        public async Task ImportCommand_Should_HandleLargeDatasetEfficiently()
        {
            // Arrange
            var largeDatasetPath = CreateLargeDatasetFile(10_000);

            try
            {
                await Program.RunAppAsync(new[] { "clear-db" }, _testServiceConfig);

                var stopwatch = Stopwatch.StartNew();
                await Program.RunAppAsync(new[] { "import", largeDatasetPath }, _testServiceConfig);
                stopwatch.Stop();

                using var provider = BuildServiceProvider();
                using var context = provider.GetRequiredService<AppDbContext>();

                var projectCount = await context.KickstarterProjects.CountAsync();
                var creatorCount = await context.Creators.CountAsync();

                Assert.That(projectCount, Is.EqualTo(10_000), "应导入 10000 条项目数据。");
                Assert.That(creatorCount, Is.EqualTo(10_000), "每条记录应生成唯一的创作者。");
                Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(20)), "导入 10000 条记录耗时超过预期上限 (20s)。");

                TestContext.WriteLine($"导入 10000 条记录耗时: {stopwatch.Elapsed}。");
            }
            finally
            {
                if (File.Exists(largeDatasetPath))
                {
                    File.Delete(largeDatasetPath);
                }
            }
        }
    }
}
