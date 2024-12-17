using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KollabR8.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string Access { get; set; } // Public, Restricted, Private

        // Foreign key for the owner of the document
        public int OwnerId { get; set; }

        public User Owner { get; set; }

        // Collaborators list (many-to-many relationship)
        public ICollection<User> Collaborators { get; set; }
    }

}
