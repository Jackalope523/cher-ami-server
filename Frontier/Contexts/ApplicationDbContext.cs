using CrazyLizard.Entities;
using CrazyLizard.Entities.Reports;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static CrazyLizard.Entities.Reports.Report;

namespace CrazyLizard.Contexts
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, IdentityRole<long>, long>(options)
    {
        internal DbSet<Issue> Issues { get; set; }
        internal DbSet<Circle> Circles { get; set; }
        internal DbSet<Recipient> Recipients { get; set; }
        internal DbSet<Report> Reports { get; set; }
        internal DbSet<UserReport> UserReports { get; set; }
        internal DbSet<PostReport> PostReports { get; set; }
        internal DbSet<Block> Blocks { get; set; }
        internal DbSet<Post> Posts { get; set; }
        internal DbSet<Snapshot> Snapshots { get; set; }
        internal DbSet<Caption> Captions { get; set; }
        internal DbSet<Subscription> Subscriptions { get; set; }
        internal DbSet<Feedback> Feedback { get; set; }
        internal DbSet<Notification> Notifications { get; set; }
        internal DbSet<Word> Words { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=dev.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.SoftDeleted);

            modelBuilder.Entity<User>()
              .HasData(new User()
              {
                  Id = 2,
                  PhoneNumber = "+15734922666",
                  FirstName = "CANARY",
                  IsPhoneConfirmed = true,

              });

            modelBuilder.Entity<User>()
                .HasData(new User()
                {
                    Id = 7,
                    PhoneNumber = "+11002003007",
                    FirstName = "Apple Test Account",
                    IsPhoneConfirmed = true,
                });

            modelBuilder.Entity<User>()
               .HasData(new User()
               {
                   Id = 8,
                   PhoneNumber = "+11002003008",
                   FirstName = "Google Test Account",
                   IsPhoneConfirmed = true,
               });

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.Title)
                .HasMaxLength(25);

            modelBuilder.Entity<User>()
               .Property(u => u.FirstName)
               .HasMaxLength(100);

            modelBuilder.Entity<User>()
               .Property(u => u.LastName)
               .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.NormalizedEmail)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<User>()
                .Property(u => u.SecurityStamp)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.AvatarPath)
                .HasMaxLength(1024);

            modelBuilder.Entity<User>()
               .Property(u => u.LastName)
               .HasMaxLength(100);

            modelBuilder.Entity<User>()
               .Property(u => u.StripeCustomerId)
               .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.StripeSubscriptionId)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.IssuePosts)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.IssueReminders)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReporterList)
                .WithOne(r => r.FilingUser)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReportedList)
                .WithOne(r => r.ReportedUser)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.BlockerList)
                .WithOne(b => b.Blocker)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.BlockedList)
                .WithOne(b => b.Blocked)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SnapshotReports)
                .WithOne(r => r.FilingUser)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.Author)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Feedback)
                .WithOne(f => f.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.Recipient)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
               .HasMany(u => u.Feedback)
               .WithOne(f => f.User)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
               .HasMany(u => u.Recipients)
               .WithOne(r => r.Manager)
               .OnDelete(DeleteBehavior.Restrict);

            // Circle
            modelBuilder.Entity<Circle>()
                .HasQueryFilter(g => !g.SoftDeleted);

            modelBuilder.Entity<Circle>()
                .Property(c => c.CircleCode)
                .HasMaxLength(100);

            modelBuilder.Entity<Circle>()
                .HasIndex(c => c.CircleCode)
                .IsUnique();

            modelBuilder.Entity<Circle>()
                .Property(g => g.Title)
                .HasMaxLength(100);

            modelBuilder.Entity<Circle>()
                .Property(c => c.HeaderPath)
                .HasMaxLength(1024);

            modelBuilder.Entity<Circle>()
               .HasMany(c => c.Notifications)
               .WithOne(n => n.Circles)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Circle>()
               .HasMany(c => c.Issues)
               .WithOne(i => i.Circle)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Circle>()
               .HasMany(c => c.Members)
               .WithOne(m => m.Circle)
               .OnDelete(DeleteBehavior.Restrict);


            // Recipient
            modelBuilder.Entity<Recipient>()
               .HasQueryFilter(g => !g.SoftDeleted);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.Title)
               .HasMaxLength(25);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.FirstName)
               .HasMaxLength(100);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.LastName)
               .HasMaxLength(100);

            modelBuilder.Entity<Recipient>()
              .Property(r => r.UnitNumber)
              .HasMaxLength(15);

            modelBuilder.Entity<Recipient>()
              .Property(r => r.StreetAddress)
              .HasMaxLength(150);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.City)
               .HasMaxLength(50);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.ProvinceOrState)
               .HasMaxLength(50);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.PostalCode)
               .HasMaxLength(20);

            modelBuilder.Entity<Recipient>()
               .Property(r => r.Country)
               .HasMaxLength(56);

            // Issue
            modelBuilder.Entity<Issue>()
                .HasQueryFilter(g => !g.SoftDeleted);

            modelBuilder.Entity<Issue>()
                .Property(i => i.Title)
                .HasMaxLength(100);

            modelBuilder.Entity<Issue>()
                .Property(i => i.HeaderPath)
                .HasMaxLength(100);

            modelBuilder.Entity<Issue>()
                .HasMany(i => i.Posts)
                .WithOne(p => p.Issue)
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription
            modelBuilder.Entity<Subscription>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.DeviceToken)
                .HasMaxLength(500);

            // Blocks
            modelBuilder.Entity<Block>()
                .HasQueryFilter(s => !s.SoftDeleted);

            // Reports
            modelBuilder.Entity<Report>()
                .HasQueryFilter(w => !w.SoftDeleted);

            modelBuilder.Entity<Report>()
                .Property(r => r.Notes)
                .HasMaxLength(2000);

            modelBuilder.Entity<Report>()
                .HasDiscriminator<ReportDiscriminator>("Discriminator")
                .HasValue<UserReport>(ReportDiscriminator.UserReport)
                .HasValue<PostReport>(ReportDiscriminator.PostReport);

            modelBuilder.Entity<PostReport>()
                .Property(r => r.Type)
                .HasColumnName("Type");

            modelBuilder.Entity<UserReport>()
                .Property(r => r.Type)
                .HasColumnName("Type");

            modelBuilder.Entity<PostReport>()
                .Property(r => r.Type)
                .HasColumnName("Type");

            // Post
            modelBuilder.Entity<Post>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Snapshots)
                .WithOne(s => s.Post)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Captions)
                .WithOne(c => c.Post)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
               .HasMany(p => p.Reports)
               .WithOne(r => r.Post)
               .OnDelete(DeleteBehavior.Restrict);

            // Snapshot
            modelBuilder.Entity<Snapshot>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Snapshot>()
                .Property(s => s.Path)
                .HasMaxLength(1024);

            // Caption
            modelBuilder.Entity<Caption>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Caption>()
                .Property(c => c.Text)
                .HasMaxLength(200);

            // Feedback
            modelBuilder.Entity<Feedback>()
                .HasQueryFilter(f => !f.SoftDeleted);

            modelBuilder.Entity<Feedback>().Property(f => f.Comments)
                .HasMaxLength(300);

            // Notifications
            modelBuilder.Entity<Notification>()
                .HasQueryFilter(n => !n.SoftDeleted);

            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationId)
                .HasMaxLength(36);

            // Words
            modelBuilder.Entity<Word>()
                .HasQueryFilter(w => !w.SoftDeleted);

            modelBuilder.Entity<Word>()
                .Property(w => w.Text)
                .HasMaxLength(50);
        }
    }
}