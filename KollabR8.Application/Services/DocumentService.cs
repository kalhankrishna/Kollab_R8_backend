using KollabR8.Application.DTOs;
using KollabR8.Application.Interfaces;
using KollabR8.Domain.Entities;
using KollabR8.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _dbContext;
        private readonly string _documentsRootPath;

        public DocumentService(AppDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _documentsRootPath = Path.Combine(environment.ContentRootPath, "Documents");

            // Ensure the Documents folder exists
            if (!Directory.Exists(_documentsRootPath))
            {
                Directory.CreateDirectory(_documentsRootPath);
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(string title, string content, string accessLevel, int ownerId)
        {
            var filePath = Path.Combine(_documentsRootPath, $"{Guid.NewGuid()}.txt");

            await File.WriteAllTextAsync(filePath, content);

            var document = new Document
            {
                Title = title,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Access = accessLevel,
                OwnerId = ownerId
            };

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            return new DocumentDto
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
        }

        public async Task<DocumentDto> GetDocumentbyIdAsync(int documentId)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

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

            return documentDto;
        }

        public async Task<DocumentDto> UpdateDocumentAsync(int documentId, string Title, string Content, string accessLevel, int ownerId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null || document.OwnerId!=ownerId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this document");
            }

            document.Title = Title;
            document.Access = accessLevel;
            document.LastUpdatedAt = DateTime.UtcNow;

            await File.WriteAllTextAsync(document.FilePath, Content);

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

            return documentDto;
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
            {
                return false;
            }

            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }

            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
