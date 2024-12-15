using KollabR8.Application.DTOs;
using KollabR8.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<UserDto> RegisterAsync(string username, string email, string password)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                throw new ArgumentException("Email already exists!", nameof(email));
            }

            if (await _userManager.FindByNameAsync(username) != null)
            {
                throw new ArgumentException("Username already exists!", nameof(email));
            }

            var user = new User
            {
                UserName = username,
                Email = email,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"User registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return userDto;
        }

        public async Task<string> LoginAsync(string userCredential, string password)
        {
            if (string.IsNullOrWhiteSpace(userCredential) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Credentials cannot be empty.", nameof(userCredential));
            }

            var user = userCredential.Contains("@")
                ? await _userManager.Users.SingleOrDefaultAsync(u => u.Email == userCredential)
                : await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == userCredential);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                throw new ArgumentException("Invalid email or password!", nameof(userCredential));
            }

            return GenerateJwtToken(user);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:JwtSettings:Issuer"],
                audience: _configuration["Authentication:JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Authentication:JwtSettings:ExpiryInMinutes"])),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
