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
            _context.KickstarterProjects.RemoveRange(_context.KickstarterProjects);
            _context.Creators.RemoveRange(_context.Creators);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Locations.RemoveRange(_context.Locations);
            await _context.SaveChangesAsync();
            Console.WriteLine("数据库清空成功。");
        }
    }
}