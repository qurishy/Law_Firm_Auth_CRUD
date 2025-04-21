using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class AplicationDB : IdentityDbContext<ApplicationUser>
    {
        public AplicationDB(DbContextOptions<AplicationDB> options) : base(options)
        {

        }

        // Register your entity models as DbSets
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Client> Clients { get; set; }

        public DbSet<Personnel> Personnel { get; set; }

        public DbSet<LegalCase> LegalCases { get; set; }
       
        public DbSet<Documented> Documents { get; set; }
   
        public DbSet<Appointment> Appointments { get; set; }

  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-one relationship between ApplicationUser and Client/Personnel
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Client)
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.UserId);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Personnel)
                .WithOne(p => p.User)
                .HasForeignKey<Personnel>(p => p.UserId);

            // Configure case-lawyer relationship
            modelBuilder.Entity<LegalCase>()
                .HasOne(c => c.AssignedLawyer)
                .WithMany(p => p.AssignedCases)
                .HasForeignKey(c => c.AssignedLawyerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure case-client relationship
            modelBuilder.Entity<LegalCase>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Cases)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);




        }
    }
}