using LMSfinal.Data;
using LMSfinal.Models;
using LMSfinal.Models.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LMSfinal.Services
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user);
        Task<(string AccessToken, string RefreshToken)?> VerifyAndGenerateRefreshTokenAsync(string token, string refreshToken);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            // 1. Tận dụng Identity UserManager để lấy danh sách quyền (Roles) của User
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            // Đẩy Roles vào Claims giúp [Authorize(Roles = "Admin")] trên Controller hoạt động bình thường
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var accessToken = jwtTokenHandler.WriteToken(token);

            // 2. Tạo mã Refresh Token ngẫu nhiên mã hóa an toàn
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // 3. Lưu vết cặp Token vào Database để quản lý phiên
            var userRefreshToken = new UserRefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                Token = refreshToken,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]))
            };

            await _context.UserRefreshTokens.AddAsync(userRefreshToken);
            await _context.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        public async Task<(string AccessToken, string RefreshToken)?> VerifyAndGenerateRefreshTokenAsync(string expiredToken, string refreshToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // Tắt để đọc thông tin từ Access Token đã hết hạn
            };

            try
            {
                // Kiểm tra tính hợp lệ của Access Token cũ
                var tokenInVerification = jwtTokenHandler.ValidateToken(expiredToken, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result) return null;
                }

                // Kiểm tra Refresh Token trong database của Identity
                var storedToken = await _context.UserRefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
                if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
                    return null;

                // Đối chiếu mã định danh Token cũ (JTI)
                var jti = tokenInVerification.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (storedToken.JwtId != jti) return null;

                // Thu hồi token cũ (Xoay vòng mã bảo mật)
                storedToken.IsRevoked = true;
                _context.UserRefreshTokens.Update(storedToken);

                // Tìm thông tin User thông qua Id lấy từ bản ghi lưu trữ
                var user = await _userManager.FindByIdAsync(storedToken.UserId);
                if (user == null) return null;

                // Cấp lại cặp chuỗi token mới
                return await GenerateTokensAsync(user);
            }
            catch
            {
                return null;
            }
        }
    }
}
