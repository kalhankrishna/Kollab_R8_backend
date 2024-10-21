using KollabR8.Application.DTOs;
using KollabR8.Application.Interfaces;
using KollabR8.Domain.Entities;
using KollabR8.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Application.Services
{
    public class CollaborationService : ICollaborationService
    {
        private readonly AppDbContext _dbContext;

        public CollaborationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddCollaboratorAsync(int documentId, int userId, int collaboratorId)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);

            if(document == null)
            {
                throw new Exception("Document not found!");
            }

            if(document.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the document owner can add collaborators!");
            }

            var collaborator = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id==collaboratorId);

            if(collaborator == null)
            {
                throw new Exception("Collaborator not found!");
            }

            if (!document.Collaborators.Contains(collaborator))
            {
                document.Collaborators.Add(collaborator);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<string>> AddMultipleCollaboratorsAsync(int documentId, int userId, List<int> collaboratorIds)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            if (document.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the document owner can add collaborators!");
            }

            var failedCollaborators = new List<string>();

            foreach(var collaboratorId in collaboratorIds)
            {
                var collaborator = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == collaboratorId);

                if (collaborator == null)
                {
                    failedCollaborators.Add($"User with ID {collaboratorId} not found.");
                    continue;
                }

                if (document.Collaborators.Any(c => c.Id == collaboratorId))
                {
                    failedCollaborators.Add($"User with ID {collaboratorId} is already a collaborator.");
                    continue;
                }

                document.Collaborators.Add(collaborator);
            }

            await _dbContext.SaveChangesAsync();

            return failedCollaborators;
        }

        public async Task<bool> RemoveCollaboratorAsync(int documentId, int userId, int collaboratorId)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            if (document.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Only the document owner can add collaborators!");
            }

            var collaborator = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == collaboratorId);

            if (collaborator == null)
            {
                throw new Exception("Collaborator not found!");
            }

            if (document.Collaborators.Contains(collaborator))
            {
                document.Collaborators.Remove(collaborator);
                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<List<User>> GetCollaboratorsAsync(int documentId)
        {
            var document = await _dbContext.Documents
                .Include(d => d.Collaborators)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new Exception("Document not found");
            }

            return document.Collaborators.ToList();
        }
    }
}
