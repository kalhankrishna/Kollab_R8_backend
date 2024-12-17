using KollabR8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KollabR8.Application.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string Access { get; set; }
        public int OwnerId { get; set; }
        public UserDto Owner { get; set; }
        public ICollection<UserDto> Collaborators { get; set; }
    }

    public class CreateDocumentDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Access { get; set; }
#nullable enable
        public List<int>? CollaboratorIds { get; set; }
#nullable disable
    }

    public class UpdateDocumentDto
    {
        [Required]
        public string Title { get; set; }
        public string content { get; set; }
    }
}
