using KollabR8.Application.DTOs;
using KollabR8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Application.Interfaces
{
    public interface IDocumentService
    {
#nullable enable
        Task<int> CreateDocumentAsync(string title, string accessLevel, int ownerId, List<int>? collaboratorIds=null);
#nullable disable
        Task<DocumentDto> GetDocumentbyIdAsync(int documentId, int userId);
        Task<DocumentDto> UpdateDocumentAsync(int documentId, string title, string content, int userId);
        Task<bool> DeleteDocumentAsync(int documentId, int userId);
        Task<List<DocumentDto>> GetOwnedDocumentsbyUser(int userId);
        Task<List<DocumentDto>> GetCollaboratingDocumentsbyUser(int userId);
        Task<bool> ModifyAccessAsync(int documentId, int userId, string accessLevel);
    }
}
