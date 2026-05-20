using LMSfinal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMSfinal.ApiControllers
{
    [ApiController]
    [Route("api/instructor")]
    public class InstructorController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Instructor");

            var result = usersInRole.Select(u => new
            {
                u.Id,
                Name = !string.IsNullOrEmpty(u.FullName) ? u.FullName : u.UserName
            }).ToList();

            return Ok(result);
        }
    }
}
