using powerkit3000.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace consoleapp.Commands
{
    public class CountsCommand
    {
        private readonly AppDbContext _context;
        private readonly ILogger<Program> _logger;

        public CountsCommand(AppDbContext context, ILogger<Program> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var projectCount = await _context.KickstarterProjects.CountAsync();
            var creatorCount = await _context.Creators.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();
            var locationCount = await _context.Locations.CountAsync();

            Console.WriteLine($"项目: {projectCount}");
            Console.WriteLine($"创建者: {creatorCount}");
            Console.WriteLine($"类别: {categoryCount}");
            Console.WriteLine($"位置: {locationCount}");
        }
    }
}
