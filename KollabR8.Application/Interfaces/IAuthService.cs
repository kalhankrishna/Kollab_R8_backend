using KollabR8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Application
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string email, string password);
        Task<string> LoginAsync(string userCredential, string password);
        string GenerateJwtToken(User user);
    }
}
