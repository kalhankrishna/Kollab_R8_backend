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
            try
            {
                if (await _userManager.FindByEmailAsync(email) != null)
                {
                    throw new Exception("Email already exists!");
                }

                if (await _userManager.FindByNameAsync(username) != null)
                {
                    throw new Exception("Username already exists!");
                }

                var user = new User
                {
                    UserName = username,
                    Email = email,
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email
                };

                return userDto;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> LoginAsync(string userCredential, string password)
        {
            try
            {
                var isEmail = userCredential.Contains("@");
                var user = isEmail ? await _userManager.Users.SingleOrDefaultAsync(u => u.Email == userCredential) : await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == userCredential);

                if (user == null)
                {
                    throw new Exception("Invalid email or password!");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!isPasswordValid)
                {
                    throw new Exception("Invalid email or password!");
                }

                return GenerateJwtToken(user);
            }
            catch
            {
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            try
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
            catch
            {
                throw;
            }
        }
    }
}
