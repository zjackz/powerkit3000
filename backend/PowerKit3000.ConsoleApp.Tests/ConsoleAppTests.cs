using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PowerKit3000.Data;

namespace PowerKit3000.ConsoleApp.Tests
{
    public class ConsoleAppTests
    {
        private StringWriter _stringWriter;
        private Action<IServiceCollection> _testServiceConfig;

        [SetUp]
        public void Setup()
        {
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);

            _testServiceConfig = services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()); // Use a unique name for each test
                });
            };
        }

        [TearDown]
        public void Teardown()
        {
            // No specific teardown needed for in-memory DB as it's configured per test run
        }

        [Test]
        public async Task CountsCommand_Should_PrintCounts()
        {
            // Arrange
            var args = new[] { "counts" };

            // Act
            await Program.Main(args);

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
            var importArgs = new[] { "import", "/home/dministrator/code/HappyTools/backend/PowerKit3000.ConsoleApp/data/sample2.json" };
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
    }
}