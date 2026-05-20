using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.ApiControllers
{
    [Route("api/roles")]
    [ApiController]
    public class RoleApiController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleApiController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // 1. GET: api/roles (Lấy danh sách)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        // 2. POST: api/roles (Tạo mới - Chỉ cần truyền string name)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Tên role không được để trống.");

            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (roleExist)
                return BadRequest("Role này đã tồn tại.");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (result.Succeeded)
                return Ok(new { message = $"Đã tạo role {roleName} thành công." });

            return BadRequest(result.Errors);
        }

        // 3. PUT: api/roles/{id} (Cập nhật tên role)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] string newRoleName)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound("Không tìm thấy Role.");

            role.Name = newRoleName;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
                return Ok(new { message = "Cập nhật thành công." });

            return BadRequest(result.Errors);
        }

        // 4. DELETE: api/roles/{id} (Xóa role)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound("Không tìm thấy Role.");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                return Ok(new { message = "Xóa role thành công." });

            return BadRequest(result.Errors);
        }
    }
}
