using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollabR8.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        // Navigation property for documents owned by the user
        public ICollection<Document> OwnedDocuments { get; set; }

        // Navigation property for documents the user collaborates on
        public ICollection<Document> CollaboratingDocuments { get; set; }
    }

}
