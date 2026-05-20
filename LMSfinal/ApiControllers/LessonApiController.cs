using LMSfinal.Data;
using LMSfinal.Models.DTOs;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LMSfinal.ApiControllers
{
    [Route("api/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LessonController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{sectionId}")]
        public async Task<IActionResult> GetBySection(int sectionId)
        {
            var data = await _context.Lessons
                .Where(x => x.SectionId == sectionId)
                .OrderBy(x => x.Order)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        [RequestSizeLimit(524288000)] // 500MB
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<IActionResult> Create([FromForm] LessonDto dto)
        {
            var lesson = new Lesson
            {
                Title = dto.Title,
                Content = dto.Content,
                Order = dto.Order,
                SectionId = dto.SectionId
            };

            // Upload video
            if (dto.VideoUpload != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.VideoUpload.FileName)}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.VideoUpload.CopyToAsync(stream);

                lesson.VideoUrl = "/videos/" + fileName;
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return Ok(lesson);
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(524288000)] // 500MB
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<IActionResult> Update(int id, [FromForm] LessonDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            lesson.Title = dto.Title;
            lesson.Content = dto.Content;
            lesson.Order = dto.Order;

            if (dto.VideoUpload != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.VideoUpload.FileName)}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.VideoUpload.CopyToAsync(stream);

                lesson.VideoUrl = "/videos/" + fileName;
            }

            await _context.SaveChangesAsync();
            return Ok(lesson);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
