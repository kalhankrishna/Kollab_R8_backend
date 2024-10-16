using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // For storing hashed passwords

        // Navigation property for documents owned by the user
        public ICollection<Document> OwnedDocuments { get; set; }

        // Navigation property for documents the user collaborates on
        public ICollection<Document> CollaboratingDocuments { get; set; }
    }

}
