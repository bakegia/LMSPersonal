using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Service
{
    public interface IAccountService
    {
        // ✅ OVERLOAD 1: Gọi với 5 tham số (gọi overload 2 bên dưới)
        Task<(bool Success, string Message, ApplicationUser User)> CreateStudentAccountAsync(
            string userName, string email, string fullName, string password, string roleId);

        // ✅ OVERLOAD 2: Gọi với 10 tham số (đầy đủ)
        Task<(bool Success, string Message, ApplicationUser User)> CreateStudentAccountAsync(
            string userName, string email, string fullName, string password, string roleId,
            int mssv, string phoneNumber, DateTime dateOfBirth, string gender, string address);
        
        Task<(bool Success, string Message, ApplicationUser User)> CreateInstructorAccountAsync(
            string userName, string email, string fullName, string password, string roleId,
            string phoneNumber, string department, string specializedSubject, string employeeCode, DateTime hireDate);
    }

    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ==================== CREATE STUDENT - OVERLOAD 1 ====================
        public async Task<(bool Success, string Message, ApplicationUser User)> CreateStudentAccountAsync(
            string userName, string email, string fullName, string password, string roleId)
        {
            return await CreateStudentAccountAsync(
                userName, email, fullName, password, roleId,
                0,
                string.Empty,
                DateTime.MinValue,
                string.Empty,
                string.Empty
            );
        }

        // ==================== CREATE STUDENT - OVERLOAD 2 ====================
        public async Task<(bool Success, string Message, ApplicationUser User)> CreateStudentAccountAsync(
            string userName, string email, string fullName, string password, string roleId,
            int mssv, string phoneNumber, DateTime dateOfBirth, string gender, string address)
        {
            try
            {
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null)
                    return (false, "Tên user này đã tồn tại", null);

                var existingEmail = await _userManager.FindByEmailAsync(email);
                if (existingEmail != null)
                    return (false, "Email này đã được sử dụng", null);

                var newUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return (false, $"Lỗi tạo tài khoản: {errors}", null);
                }

                if (!string.IsNullOrEmpty(roleId))
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(newUser, role.Name);
                    }
                    else
                    {
                        return (false, "Role không tồn tại", newUser);
                    }
                }

                var studentProfile = new UserProfile
                {
                    UserId = newUser.Id,
                    Email = email,
                    Fullname = fullName,
                    Name = userName,
                    Mssv = mssv,
                    PhoneNumber = phoneNumber,
                    DateOfBirth = dateOfBirth,
                    Gender = gender,
                    Address = address
                };

                _context.UserProfiles.Add(studentProfile);
                await _context.SaveChangesAsync();

                return (true, "Tạo tài khoản Student thành công", newUser);
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                return (false, $"Lỗi cơ sở dữ liệu: {innerEx}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        // ==================== CREATE INSTRUCTOR ====================
        public async Task<(bool Success, string Message, ApplicationUser User)> CreateInstructorAccountAsync(
            string userName, string email, string fullName, string password, string roleId,
            string phoneNumber, string department, string specializedSubject, string employeeCode, DateTime hireDate)
        {
            try
            {
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null)
                    return (false, "Tên user này đã tồn tại", null);

                var existingEmail = await _userManager.FindByEmailAsync(email);
                if (existingEmail != null)
                    return (false, "Email này đã được sử dụng", null);

                var newUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return (false, $"Lỗi tạo tài khoản: {errors}", null);
                }

                if (!string.IsNullOrEmpty(roleId))
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(newUser, role.Name);
                    }
                    else
                    {
                        return (false, "Role không tồn tại", newUser);
                    }
                }

                // Thêm vào phần tạo Instructor profile trong CreateInstructorAccountAsync

                var instructorProfile = new Instructor
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = newUser.Id,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Department = department,
                    SpecializedSubject = specializedSubject,
                    ProfileImageUrl = "",
                    EmployeeCode = string.IsNullOrWhiteSpace(employeeCode)
                        ? $"EMP_{Guid.NewGuid().ToString().Substring(0, 8)}"
                        : employeeCode,
                    HireDate = hireDate,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Instructors.Add(instructorProfile);
                await _context.SaveChangesAsync();

                return (true, "Tạo tài khoản Instructor thành công", newUser);
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                return (false, $"Lỗi cơ sở dữ liệu: {innerEx}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }
    }
}