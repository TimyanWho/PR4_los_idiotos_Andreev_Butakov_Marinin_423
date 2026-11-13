using Api_pendejos.DTO;
using Api_pendejos.Models;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Api_pendejos.DTO;

namespace Api_pendejos.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AndButMar_MarketplaceContext _context;
        private readonly IPasswordHasher<object> _hasher;
        private readonly SymmetricSecurityKey _jwtKey; // Добавляем поле для хранения ключа

        public AuthController(AndButMar_MarketplaceContext context, IPasswordHasher<object> hasher, SymmetricSecurityKey jwtKey)
        {
            _context = context;
            _hasher = hasher;
            _jwtKey = jwtKey;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var account = _context.Users
                .Include(a => a.Role)
                .FirstOrDefault(a => a.Login == request.Login);

            if (account == null || string.IsNullOrWhiteSpace(request.Password))
            {
                return Challenge();
            }

            var result = _hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Challenge();
            }

            return Ok(new
            {
                access_token = CreateToken(account),
                token_type = "Bearer"
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Address) ||
                string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Заполните все поля."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Пароль должен содержать минимум 8 символов."
                });
            }

            bool loginExists = await _context.Users
                .AnyAsync(u => u.Login == request.Login);

            if (loginExists)
            {
                return Conflict(new
                {
                    message = "Такой логин уже существует."
                });
            }

            var user = new Users
            {
                Login = request.Login,
                RoleId = 2,
                CreatedAt = DateTime.Now,
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/users/{user.Id}",
                new
                {
                    id = user.Id,
                    login = user.Login,
                    role = "Пользователь"
                });
        }

        private string CreateToken(Users user)
        {
            var claims = new[]
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Login),
                new Claim("role", user.Role.Name)
            };

            var token = new JwtSecurityToken(
                issuer: "JwtDemo",
                audience: "JwtDemoApi",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(
                    _jwtKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
