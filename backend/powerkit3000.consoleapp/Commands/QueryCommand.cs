using powerkit3000.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace consoleapp.Commands
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
            var filters = ParseFilters(args);
            var query = _context.KickstarterProjects.AsQueryable();

            foreach (var filter in filters)
            {
                var key = filter.Key;
                var value = filter.Value;

                Console.WriteLine($"应用过滤器：{key}={value}");

                switch (key)
                {
                    case "state":
                        query = query.Where(p => p.State == value);
                        break;
                    case "country":
                        query = query.Where(p => p.Country == value);
                        break;
                    // 在此处为其他可查询字段添加其他 case。
                }
            }

            var projects = await query.ToListAsync();
            Console.WriteLine($"找到 {projects.Count} 个项目。");
            foreach (var project in projects)
            {
                var displayName = string.IsNullOrWhiteSpace(project.NameCn) ? project.Name : project.NameCn;
                Console.WriteLine($"- {displayName}");
            }
        }

        private static IDictionary<string, string> ParseFilters(string[] args)
        {
            var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < args.Length; i++)
            {
                var token = args[i];
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (token.StartsWith("--", StringComparison.Ordinal))
                {
                    var key = token.TrimStart('-');
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    {
                        filters[key] = args[++i];
                    }
                    else
                    {
                        filters[key] = "true";
                    }
                    continue;
                }

                var parts = token.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    filters[parts[0]] = parts[1];
                }
            }

            return filters.ToDictionary(kvp => kvp.Key.ToLowerInvariant(), kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
