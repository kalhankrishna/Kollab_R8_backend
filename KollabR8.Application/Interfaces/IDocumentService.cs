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
        Task<DocumentDto> CreateDocumentAsync(string title, string content, string accessLevel, int ownerId, List<int>? collaboratorIds=null);
        Task<DocumentDto> GetDocumentbyIdAsync(int documentId, int userId);
        Task<DocumentDto> UpdateDocumentAsync(int documentId, string title, string content, string accessLevel, int userId);
        Task<bool> DeleteDocumentAsync(int documentId, int userId);
        Task<List<Document>> GetOwnedDocumentsbyUser(int userId);
        Task<List<Document>> GetCollaboratingDocumentsbyUser(int userId);
        Task<Document> ModifyAccessAsync(int documentId, int userId, string accessLevel);
    }
}
