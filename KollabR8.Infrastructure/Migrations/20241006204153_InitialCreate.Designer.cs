﻿// <auto-generated />
using KollabR8.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KollabR8.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241006204153_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.17");

            modelBuilder.Entity("DocumentUser", b =>
                {
                    b.Property<int>("CollaboratingDocumentsId")
                        .HasColumnType("int");

                    b.Property<int>("CollaboratorsId")
                        .HasColumnType("int");

                    b.HasKey("CollaboratingDocumentsId", "CollaboratorsId");

                    b.HasIndex("CollaboratorsId");

                    b.ToTable("DocumentCollaborators");
                });

            modelBuilder.Entity("KollabR8.Domain.Entities.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Access")
                        .HasColumnType("longtext");

                    b.Property<string>("Content")
                        .HasColumnType("longtext");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("KollabR8.Domain.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Email = "admin@example.com",
                            PasswordHash = "hashed_password",
                            Username = "admin"
                        });
                });

            modelBuilder.Entity("DocumentUser", b =>
                {
                    b.HasOne("KollabR8.Domain.Entities.Document", null)
                        .WithMany()
                        .HasForeignKey("CollaboratingDocumentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KollabR8.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("CollaboratorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KollabR8.Domain.Entities.Document", b =>
                {
                    b.HasOne("KollabR8.Domain.Entities.User", "Owner")
                        .WithMany("OwnedDocuments")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("KollabR8.Domain.Entities.User", b =>
                {
                    b.Navigation("OwnedDocuments");
                });
#pragma warning restore 612, 618
        }
    }
}
