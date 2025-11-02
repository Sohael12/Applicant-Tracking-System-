using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Stageproject_ATS_AP2025Q2.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<InterviewNote> InterviewNotes { get; set; }
        public DbSet<InterviewSchedule> Interviews { get; set; }
        public DbSet<StatusHistory> StatusHistories { get; set; }
        public new DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Template> Templates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // JSON conversion voor InvitedUserEmails
            modelBuilder.Entity<InterviewNote>()
                .Property(n => n.InvitedUserEmails)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                );
        }
    }
}
