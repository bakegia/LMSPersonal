using LMSfinal.Data;
using LMSfinal.Models.DTOs;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.ApiControllers
{
    [ApiController]
    [Route("api/course")]
    public class CourseApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseApiController> _logger;

        public CourseApiController(ApplicationDbContext context, ILogger<CourseApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/course?pageNumber=1&pageSize=10&search=&sortBy=&categoryId=
        [HttpGet]
        public async Task<IActionResult> GetAllCourses(int pageNumber = 1, int pageSize = 10, string search = "", string sortBy = "title", int? categoryId = null)
        {
            try
            {
                var query = _context.Courses.Include(c => c.Category).AsQueryable();

                // Apply ID filter
                if (categoryId.HasValue && categoryId > 0)
                {
                    query = query.Where(x => x.CategoryId == categoryId.Value);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(x => x.Title.Contains(search) ||
                                             x.CourseCode.Contains(search) ||
                                             x.Slug.Contains(search));
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "id" => query.OrderBy(x => x.Id),
                    "id_desc" => query.OrderByDescending(x => x.Id),
                    "code" => query.OrderBy(x => x.CourseCode),
                    "code_desc" => query.OrderByDescending(x => x.CourseCode),
                    "title_desc" => query.OrderByDescending(x => x.Title),
                    _ => query.OrderBy(x => x.Title)
                };

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Apply pagination
                var data = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        id = c.Id,
                        courseCode = c.CourseCode,
                        title = c.Title,
                        slug = c.Slug,
                        description = c.Description,
                        imageUrl = c.ImageUrl,
                        categoryId = c.CategoryId,
                        category = c.Category == null
                            ? null
                            : new
                            {
                                categoryId = c.Category.CategoryId,
                                name = c.Category.Name
                            }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items = data,
                    totalCount = totalCount,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    totalPages = totalPages,
                    hasNextPage = pageNumber < totalPages,
                    hasPreviousPage = pageNumber > 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllCourses: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var c = await _context.Courses
                    .Where(x => x.Id == id)
                    .Select(x => new
                    {
                        x.Id,
                        x.CourseCode,
                        x.Title,
                        x.Slug,
                        x.Description,
                        x.ImageUrl,
                        x.CategoryId
                    })
                    .FirstOrDefaultAsync();

                if (c == null) return NotFound();

                return Ok(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetById: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CourseCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var course = new Course
                {
                    CourseCode = dto.CourseCode,
                    Title = dto.Title,
                    Slug = dto.Slug,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId
                };

                if (dto.ImageUpload != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/courses", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageUpload.CopyToAsync(stream);
                    }

                    course.ImageUrl = $"/images/courses/{fileName}";
                }

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] CourseCreateDto dto)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound();

                course.CourseCode = dto.CourseCode;
                course.Title = dto.Title;
                course.Slug = dto.Slug;
                course.Description = dto.Description;
                course.CategoryId = dto.CategoryId;

                if (dto.ImageUpload != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageUpload.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/courses", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageUpload.CopyToAsync(stream);
                    }

                    course.ImageUrl = $"/images/courses/{fileName}";
                }

                await _context.SaveChangesAsync();

                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Update: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null) return NotFound();

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}