using System.IO;
using HappyTools.Core.Services;
using HappyTools.Data;
using HappyTools.WebApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HappyTools.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KickstarterController : ControllerBase
    {
        private readonly KickstarterDataImportService _importService;
        private readonly AppDbContext _context;

        public KickstarterController(KickstarterDataImportService importService, AppDbContext context)
        {
            _importService = importService;
            _context = context;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportData([FromForm] FileUploadRequest request)
        {
            // For debugging, use local sample2.json directly
            var filePath = "../../data/sample2.json"; // Use sample2.json directly

            try
            {
                // Bypass file upload for now
                // if (request.File == null || request.File.Length == 0)
                // {
                //     return BadRequest("请上传有效的文件。");
                // }
                // var tempFilePath = Path.GetTempFileName();
                // using (var stream = new FileStream(tempFilePath, FileMode.Create))
                // {
                //     await request.File.CopyToAsync(stream);
                // }
                // await _importService.ImportDataAsync(tempFilePath);

                await _importService.ImportDataAsync(filePath); // Pass the local file path
                return Ok("Data imported successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {
                // if (System.IO.File.Exists(tempFilePath))
                // {
                //     System.IO.File.Delete(tempFilePath);
                // }
            }
        }

        [HttpGet("query")]
        public async Task<IActionResult> QueryProjects([FromQuery] KickstarterQueryDto queryDto)
        {
            try
            {
                var query = _context.KickstarterProjects
                    .Include(p => p.Creator)
                    .Include(p => p.Category)
                    .Include(p => p.Location)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(queryDto.State))
                {
                    query = query.Where(p => p.State == queryDto.State);
                }

                if (!string.IsNullOrWhiteSpace(queryDto.Country))
                {
                    query = query.Where(p => p.Country == queryDto.Country);
                }

                if (!string.IsNullOrWhiteSpace(queryDto.CategoryName))
                {
                    query = query.Where(p => p.Category != null && p.Category.Name.Contains(queryDto.CategoryName));
                }

                if (!string.IsNullOrWhiteSpace(queryDto.ProjectName))
                {
                    query = query.Where(p => p.Name != null && p.Name.Contains(queryDto.ProjectName));
                }

                if (queryDto.MinGoal.HasValue)
                {
                    query = query.Where(p => p.Goal >= queryDto.MinGoal.Value);
                }

                if (queryDto.MaxGoal.HasValue)
                {
                    query = query.Where(p => p.Goal <= queryDto.MaxGoal.Value);
                }

                if (queryDto.MinPledged.HasValue)
                {
                    query = query.Where(p => p.Pledged >= queryDto.MinPledged.Value);
                }

                if (queryDto.MaxPledged.HasValue)
                {
                    query = query.Where(p => p.Pledged <= queryDto.MaxPledged.Value);
                }

                if (queryDto.MinBackersCount.HasValue)
                {
                    query = query.Where(p => p.BackersCount >= queryDto.MinBackersCount.Value);
                }

                if (queryDto.MaxBackersCount.HasValue)
                {
                    query = query.Where(p => p.BackersCount <= queryDto.MaxBackersCount.Value);
                }

                var totalCount = await query.CountAsync();

                var projects = await query
                    .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                    .Take(queryDto.PageSize)
                    .ToListAsync();

                return Ok(new { TotalCount = totalCount, Projects = projects });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("counts")]
        public async Task<IActionResult> GetCounts()
        {
            try
            {
                var projectCount = await _context.KickstarterProjects.CountAsync();
                var creatorCount = await _context.Creators.CountAsync();
                var categoryCount = await _context.Categories.CountAsync();
                var locationCount = await _context.Locations.CountAsync();

                return Ok(new
                {
                    ProjectCount = projectCount,
                    CreatorCount = creatorCount,
                    CategoryCount = categoryCount,
                    LocationCount = locationCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
