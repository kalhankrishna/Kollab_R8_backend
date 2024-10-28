using KollabR8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Application.Interfaces
{
    public interface ICollaborationService
    {
        Task<bool> AddCollaboratorAsync(int documentId, int userId, int collaboratorId);
        Task<bool> RemoveCollaboratorAsync(int documentId, int userId, int collaboratorId);
        Task<List<User>> GetCollaboratorsAsync(int documentId);
    }
}
