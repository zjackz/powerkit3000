using PowerKit3000.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Commands
{
    public class ClearDbCommand
    {
        private readonly AppDbContext _context;
        private readonly ILogger<Program> _logger;

        public ClearDbCommand(AppDbContext context, ILogger<Program> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("正在清空数据库。");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"KickstarterProjects\" RESTART IDENTITY CASCADE;");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Creators\" RESTART IDENTITY CASCADE;");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Categories\" RESTART IDENTITY CASCADE;");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Locations\" RESTART IDENTITY CASCADE;");
            Console.WriteLine("数据库清空成功。");
        }
    }
}