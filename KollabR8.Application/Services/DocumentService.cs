using KollabR8.Application.ConnectionHub;
using KollabR8.Application.DTOs;
using KollabR8.Application.Interfaces;
using KollabR8.Domain.Entities;
using KollabR8.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KollabR8.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _dbContext;
        private readonly string _documentsRootPath;
        private readonly IHubContext<DocumentHub> _hubContext;

        public DocumentService(AppDbContext dbContext, IWebHostEnvironment environment, IHubContext<DocumentHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _documentsRootPath = Path.Combine(environment.ContentRootPath, "Documents");

            // Ensure the Documents folder exists
            if (!Directory.Exists(_documentsRootPath))
            {
                Directory.CreateDirectory(_documentsRootPath);
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(string title, string content, string accessLevel, int ownerId, List<int>? collaboratorIds=null)
        {
            var filePath = Path.Combine(_documentsRootPath, $"{Guid.NewGuid()}.json");

            await File.WriteAllTextAsync(filePath, content);

            var collaborators = new List<User>();

            if(collaboratorIds!=null && collaboratorIds.Any())
            {
                collaborators = await _dbContext.Users.Where(u => collaboratorIds.Contains(u.Id)).ToListAsync();
            }

            var document = new Document
            {
                Title = title,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Access = accessLevel,
                OwnerId = ownerId,
                Collaborators = collaborators
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            var documentDto = new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                FilePath = document.FilePath,
                Content = content,
                Access = document.Access,
                CreatedAt = document.CreatedAt,
                LastUpdatedAt = document.LastUpdatedAt,
                OwnerId = document.OwnerId,
                Owner = document.Owner,
                Collaborators = document.Collaborators
            };

            //await _hubContext.Groups.AddToGroupAsync(ownerId.ToString(), document.Id.ToString());

            /*foreach (var collaborator in collaborators)
            {
                await _hubContext.Groups.AddToGroupAsync(collaborator.Id.ToString(), document.Id.ToString());
                await _hubContext.Clients.User(collaborator.Id.ToString()).SendAsync("NotifyDocumentCreated", documentDto);
            }*/

            await _hubContext.Clients.Group(document.Id.ToString()).SendAsync("NotifyDocumentCreated", documentDto);

            return documentDto;
        }

        public async Task<DocumentDto> GetDocumentbyIdAsync(int documentId, int userId)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(document.OwnerId != userId || !document.Collaborators.Contains(user) || document.Access != "Public")
            {
                throw new UnauthorizedAccessException("You do not have permission to view this document!");
            }

            var contentJson = await File.ReadAllTextAsync(document.FilePath);

            var documentDto = new DocumentDto
            {
                Id = documentId,
                Title = document.Title,
                FilePath = document.FilePath,
                Content = contentJson,
                CreatedAt = document.CreatedAt,
                LastUpdatedAt = document.LastUpdatedAt,
                Access = document.Access,
                OwnerId = document.OwnerId,
                Owner = document.Owner,
                Collaborators = document.Collaborators
            };

            return documentDto;
        }

        public async Task<DocumentDto> UpdateDocumentAsync(int documentId, string title, string content, string accessLevel, int userId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            var updatingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            if((document.Collaborators != null && !document.Collaborators.Contains(updatingUser)) || document.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this document!");
            }

            document.Title = title;
            document.Access = accessLevel;
            document.LastUpdatedAt = DateTime.UtcNow;

            await File.WriteAllTextAsync(document.FilePath, content);

            _dbContext.Documents.Update(document);
            await _dbContext.SaveChangesAsync();

            var documentDto = new DocumentDto
            {
                Id = documentId,
                Title = document.Title,
                FilePath = document.FilePath,
                Content = await File.ReadAllTextAsync(document.FilePath),
                CreatedAt = document.CreatedAt,
                LastUpdatedAt = document.LastUpdatedAt,
                Access = document.Access,
                OwnerId = document.OwnerId,
                Owner = document.Owner,
                Collaborators = document.Collaborators
            };

            await _hubContext.Clients.Group(documentId.ToString()).SendAsync("NotifyDocumentUpdate", documentDto);

            return documentDto;
        }

        public async Task<bool> DeleteDocumentAsync(int documentId, int userId)
        {
            var document = await _dbContext.Documents.Include(d=>d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
            {
                return false;
            }

            if(document.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this document!");
            }

            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }

            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();

            /*foreach (var collaborator in document.Collaborators)
            {
                await _hubContext.Clients.User(collaborator.Id.ToString()).SendAsync("NotifyDocumentDeleted", documentId);
                await _hubContext.Groups.RemoveFromGroupAsync(collaborator.Id.ToString(), documentId.ToString());
            }*/

            await _hubContext.Clients.Group(document.Id.ToString()).SendAsync("NotifyDocumentDeleted", documentId);

            return true;
        }

        public async Task<List<Document>> GetOwnedDocumentsbyUser(int userId)
        {
            var documents = await _dbContext.Documents.Include(d=>d.Owner).Include(d=>d.Collaborators).Where(d=>d.OwnerId==userId).ToListAsync();
            return documents;
        }

        public async Task<List<Document>> GetCollaboratingDocumentsbyUser(int userId)
        {
            var documents = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).Where(d => d.Collaborators.Any(c => c.Id == userId)).ToListAsync();
            return documents;
        }

        public async Task<Document> ModifyAccessAsync(int documentId, int userId, string accessLevel)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(d=>d.Id==documentId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            if (document.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify the access level of this document!");
            }

            document.Access = accessLevel;

            _dbContext.Documents.Update(document);
            await _dbContext.SaveChangesAsync();

            await _hubContext.Clients.Group(document.Id.ToString()).SendAsync("NotifyAccessUpdated", documentId);

            return document;
        }
    }
}
