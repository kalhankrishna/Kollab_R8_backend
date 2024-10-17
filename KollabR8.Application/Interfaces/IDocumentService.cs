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
        Task<DocumentDto> CreateDocumentAsync(string title, string content, string accessLevel, int ownerId);
        Task<DocumentDto> GetDocumentbyIdAsync(int documentId);
        Task<DocumentDto> UpdateDocumentAsync(int documentId, string Title, string Content, string accessLevel, int ownerId);
        Task<bool> DeleteDocumentAsync(int documentId);
    }
}
