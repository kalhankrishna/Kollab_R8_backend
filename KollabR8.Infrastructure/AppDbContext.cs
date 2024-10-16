using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using KollabR8.Domain.Entities;

namespace KollabR8.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }

        // Constructor for runtime use with DI
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = "hashed_password" }
            );

            // Define the many-to-many relationship between Users and Documents
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Collaborators)
                .WithMany(u => u.CollaboratingDocuments)
                .UsingEntity(j => j.ToTable("DocumentCollaborators"));

            // Ensure that each Document has an Owner
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Owner)
                .WithMany(u => u.OwnedDocuments)
                .HasForeignKey(d => d.OwnerId);
        }
    }
}
