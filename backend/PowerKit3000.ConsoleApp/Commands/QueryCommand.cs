using PowerKit3000.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Commands
{
    public class QueryCommand
    {
        private readonly AppDbContext _context;
        private readonly ILogger<Program> _logger;

        public QueryCommand(AppDbContext context, ILogger<Program> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ExecuteAsync(string[] args)
        {
            var query = _context.KickstarterProjects.AsQueryable();
            foreach (var arg in args)
            {
                var parts = arg.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0];
                var value = parts[1];

                Console.WriteLine($"应用过滤器：{key}={value}");

                switch (key)
                {
                    case "State":
                        query = query.Where(p => p.State == value);
                        break;
                    case "Country":
                        query = query.Where(p => p.Country == value);
                        break;
                    // 在此处为其他可查询字段添加其他 case。
                }
            }
            var projects = await query.ToListAsync();
            Console.WriteLine($"找到 {projects.Count} 个项目。");
            foreach (var project in projects)
            {
                Console.WriteLine($"- {project.Name}");
            }
        }
    }
}
