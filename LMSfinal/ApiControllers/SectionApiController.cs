using LMSfinal.Data;
using LMSfinal.Models.DTOs;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LMSfinal.ApiControllers
{
    [Route("api/section")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var data = await _context.Sections
                .Where(x => x.ClassroomId == courseId)
                .OrderBy(x => x.Order)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SectionDto dto)
        {
            var section = new Section
            {
                Title = dto.Title,
                Order = dto.Order,
                ClassroomId = dto.ClassroomId
            };

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            return Ok(section);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SectionDto dto)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return NotFound();

            section.Title = dto.Title;
            section.Order = dto.Order;

            await _context.SaveChangesAsync();
            return Ok(section);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return NotFound();

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
