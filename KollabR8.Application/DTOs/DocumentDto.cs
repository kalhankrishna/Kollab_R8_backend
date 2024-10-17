using KollabR8.Domain.Entities;
using System;
using System.Collections.Generic;

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
        public User Owner { get; set; }
        public ICollection<User> Collaborators { get; set; }
    }
}
