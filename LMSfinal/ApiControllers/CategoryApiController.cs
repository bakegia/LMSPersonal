using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.ApiControllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories?pageNumber=1&pageSize=10&search=&sortBy=&categoryId=
        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string search = "", string sortBy = "name", int? categoryId = null)
        {
            var query = _context.Categories.AsQueryable();

            // Apply ID filter
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search) || 
                                         x.CategoryCode.Contains(search) || 
                                         x.Slug.Contains(search));
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "id" => query.OrderBy(x => x.CategoryId),
                "id_desc" => query.OrderByDescending(x => x.CategoryId),
                "code" => query.OrderBy(x => x.CategoryCode),
                "code_desc" => query.OrderByDescending(x => x.CategoryCode),
                "name_desc" => query.OrderByDescending(x => x.Name),
                _ => query.OrderBy(x => x.Name)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.CategoryId,
                    x.Name,
                    x.Description,
                    x.Slug,
                    x.CategoryCode,
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

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category model)
        {
            if (id != model.CategoryId) return BadRequest();

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}