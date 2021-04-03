using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bsatovidal1_Job_Bank.Models;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace bsatovidal1_Job_Bank.Data
{
    public class JobbBankContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName
        public string UserName
        {
            get; private set;
        }

        public JobbBankContext(DbContextOptions<JobbBankContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
            UserName = UserName ?? "Unknown";
        }

        public JobbBankContext(DbContextOptions<JobbBankContext> options)
            : base(options)
        {
            UserName = "SeedData";
        }

        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Posting> Postings { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<ApplicantSkill> ApplicantSkills { get; set; }
        public DbSet<RetrainingProgram> RetrainingPrograms { get; set; }
        public DbSet<PositionSkill> PositionSkills { get; set; } 
        public DbSet<PostingFile> PostingFiles { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<ApplicantPhoto> ApplicantPhotos { get; set; }
        public DbSet<ApplicantFile> ApplicantFiles { get; set; }
        public DbSet<UploadedPhoto> UploadedPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Default Schema
            modelBuilder.HasDefaultSchema("JB");

            //Unique constraint for the Position Name
            modelBuilder.Entity<Position>()
                .HasIndex(e => e.Name)
                .IsUnique();

            //Unique Combined Constraint for Posting using PositionID and ClosingDate
            modelBuilder.Entity<Posting>()
                .HasIndex(e => new { e.PositionID, e.ClosingDate })
                .IsUnique();

            //Unique constraint for the Applicant email
            modelBuilder.Entity<Applicant>()
                .HasIndex(e => e.Email)
                .IsUnique()
                .HasName("IX_Unique_Applicant_email");

            //Unique Combined Constraint for Application using ApplicantID and PostingID
            modelBuilder.Entity<Application>()
                .HasIndex(e => new { e.ApplicantID, e.PostingID })
                .IsUnique()
                .HasName("IX_Unique_Application");

            //Unique constraint for the Skill Name
            modelBuilder.Entity<Skill>()
                .HasIndex(e => e.Name)
                .IsUnique();

            //Unique constraint for the Retraining Program Name
            modelBuilder.Entity<RetrainingProgram>()
                .HasIndex(e => e.Name)
                .IsUnique();

            //Many-to-Many Intersection
            modelBuilder.Entity<ApplicantSkill>()
                .HasKey(e => new { e.ApplicantID, e.SkillID });

            //Many to Many Position Skill Primary Key
            modelBuilder.Entity<PositionSkill>()
                .HasKey(e => new { e.PositionID, e.SkillID });

            //Preventing a Cascading Delete from Posting to Position and Position to Occupation.
            modelBuilder.Entity<Position>()
                .HasOne(p => p.Occupation)
                .WithMany(o => o.Positions)
                .HasForeignKey(p => p.OccupationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Posting>()
                .HasOne(pg => pg.Position)
                .WithMany(pn => pn.Postings)
                .HasForeignKey(pg => pg.PositionID)
                .OnDelete(DeleteBehavior.Restrict);

            //Preventing Cascading Delete from Posting to Application
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Posting)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.PostingID)
                .OnDelete(DeleteBehavior.Restrict);

            //Preventing Cascading Delete from Skill to ApplicantSkill
            modelBuilder.Entity<ApplicantSkill>()
                .HasOne(a => a.Skill)
                .WithMany(s => s.ApplicantSkills)
                .HasForeignKey(a => a.SkillID)
                .OnDelete(DeleteBehavior.Restrict);

            //Preventing Cascading Delete from PositionSkill to Skill
            modelBuilder.Entity<PositionSkill>()
                .HasOne(p => p.Skill)
                .WithMany(s => s.PositionSkills)
                .HasForeignKey(p => p.SkillID)
                .OnDelete(DeleteBehavior.Restrict);

            //Add this so you DO get Cascade Delete
            modelBuilder.Entity<UploadedPhoto>()
                .HasOne<PhotoContent>(p => p.PhotoContentFull)
                .WithOne(p => p.PhotoFull)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UploadedPhoto>()
                .HasOne<PhotoContent>(p => p.PhotoContentThumb)
                .WithOne(p => p.PhotoThumb)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}
