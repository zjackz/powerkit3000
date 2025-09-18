using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PowerKit3000.ConsoleApp.Tests
{
    public class ConsoleAppTests
    {
        private StringWriter _stringWriter;

        [SetUp]
        public void Setup()
        {
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);
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
            await Program.Main(args);

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
            var importArgs = new[] { "import", "/home/dministrator/code/PowerKit3000/ConsoleApp/data/sample2.json" };
            var countsArgs = new[] { "counts" };

            // Act - Clear database
            await Program.Main(clearDbArgs);

            // Act - Import sample.json
            await Program.Main(importArgs);

            // Act - Get counts after import
            _stringWriter = new StringWriter(); // Reset StringWriter for new output
            Console.SetOut(_stringWriter);
            await Program.Main(countsArgs);

            // Assert
            var output = _stringWriter.ToString();
            Assert.That(output, Does.Contain("项目: 13"));
            Assert.That(output, Does.Contain("创建者: 13"));
            Assert.That(output, Does.Contain("类别: 11"));
            Assert.That(output, Does.Contain("位置: 9"));
        }
    }
}