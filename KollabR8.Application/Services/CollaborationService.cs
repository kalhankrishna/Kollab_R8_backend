using AutoMapper;
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
        private readonly IMapper _mapper;

        public CollaborationService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<bool> AddCollaboratorAsync(int documentId, int userId, int collaboratorId)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);
            var user = await _dbContext.Users.FindAsync(userId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            if (document.Owner != user)
            {
                throw new UnauthorizedAccessException("Only the document owner can add collaborators!");
            }

            var collaborator = await _dbContext.Users.FindAsync(collaboratorId);

            if (collaborator == null)
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

        public async Task<bool> RemoveCollaboratorAsync(int documentId, int userId, int collaboratorId)
        {
            var document = await _dbContext.Documents.Include(d => d.Owner).Include(d => d.Collaborators).FirstOrDefaultAsync(d => d.Id == documentId);
            var user = await _dbContext.Users.FindAsync(userId);

            if (document == null)
            {
                throw new Exception("Document not found!");
            }

            if (document.Owner != user)
            {
                throw new UnauthorizedAccessException("Only the document owner can add collaborators!");
            }

            var collaborator = await _dbContext.Users.FindAsync(collaboratorId);

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

        public async Task<List<UserDto>> GetCollaboratorsAsync(int documentId)
        {
            var document = await _dbContext.Documents
            .Include(d => d.Collaborators)
            .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new Exception("Document not found");
            }

            var collaborators =  document.Collaborators.ToList();

            List<UserDto> collaboratorDtos = _mapper.Map<List<UserDto>>(collaborators);

            return collaboratorDtos;
        }
    }
}
