using HappyTools.Core.Services;
using HappyTools.Data;
using HappyTools.Data.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace HappyTools.Core.Tests
{
    public class KickstarterDataImportServiceTests
    {
        [Fact]
        public async Task ImportDataAsync_ShouldImportDataSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new AppDbContext(options))
            {
                var service = new KickstarterDataImportService(context);

                // Act
                // Assuming sample.json is in the data directory relative to the project root
                var currentDirectory = AppContext.BaseDirectory;
                string projectRoot = null;
                var directoryInfo = new DirectoryInfo(currentDirectory);

                while (directoryInfo != null && projectRoot == null)
                {
                    if (File.Exists(Path.Combine(directoryInfo.FullName, "HappyTools.sln")))
                    {
                        projectRoot = directoryInfo.FullName;
                    }
                    else
                    {
                        directoryInfo = directoryInfo.Parent;
                    }
                }

                if (projectRoot == null)
                {
                    throw new InvalidOperationException("Could not find project root (HappyTools.sln).");
                }

                var filePath = Path.Combine(projectRoot, "data", "sample2.json"); 
                await service.ImportDataAsync(filePath);

                // Assert
                Assert.True(context.KickstarterProjects.Any());
                Assert.True(context.Creators.Any());
                Assert.True(context.Categories.Any());
                Assert.True(context.Locations.Any());

                // You can add more specific assertions here
                // For example, check the count of imported projects or specific project data
                // Assert.Equal(expectedProjectCount, context.KickstarterProjects.Count());
            }
        }
    }
}
