using Microsoft.EntityFrameworkCore;
using Repository.Databases.Entities;
using Repository.Databases.Entities.Chats;
using Repository.Databases.Entities.Messages;
using Repository.Databases.Entities.Reports;
using static Repository.Databases.Entities.Reports.Report;
using UserReport = Repository.Databases.Entities.Reports.UserReport;

namespace Repository.Databases.Contexts
{
    internal abstract class CanaryContext : DbContext
    {
        internal DbSet<User> Users { get; set; }
        internal DbSet<Issue> Issues { get; set; }
        internal DbSet<Circle> Circles { get; set; }
        internal DbSet<CircleMembership> CircleMemberships { get; set; }
        internal DbSet<Report> Reports { get; set; }
        internal DbSet<UserReport> UserReports { get; set; }
        internal DbSet<SnapshotReport> SnapshotReports { get; set; }
        internal DbSet<Post> Posts { get; set; }
        internal DbSet<Snapshot> Snapshots { get; set; }
        internal DbSet<Caption> Captions { get; set; }
        internal DbSet<Subscription> Subscriptions { get; set; }
        internal DbSet<Feedback> Feedback { get; set; }
        internal DbSet<Notification> Notifications { get; set; }
        internal DbSet<Word> Words { get; set; }
        internal DbSet<Chat> Chats { get; set; }
        internal DbSet<PrivateChat> PrivateChats { get; set; }
        internal DbSet<CircleChat> CircleChats { get; set; }
        internal DbSet<BroadcastChat> BroadcastChats { get; set; }
        internal DbSet<ChatMembership> ChatMemberships { get; set; }
        internal DbSet<Connection> Connections { get; set; }
        internal DbSet<Message> Messages { get; set; }
        internal DbSet<TextMessage> TextMessages { get; set; }
        internal DbSet<PhotoMessage> PhotoMessages { get; set; }
        internal DbSet<IssueMessage> IssueMessages { get; set; }
        internal DbSet<PostMessage> PostMessages { get; set; }
        internal DbSet<ProfileMessage> ProfileMessages { get; set; }
        internal DbSet<ActivityMessage> ActivityMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Entity
            modelBuilder.Ignore<Entity>();

            // User
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => !u.SoftDeleted);

            modelBuilder.Entity<User>()
              .HasData(new User()
              {
                  Id = -2,
                  PhoneNumber = "15734922666",
                  FirstName = "CANARY",
                  IsPhoneConfirmed = true,

              });

            modelBuilder.Entity<User>()
                .HasData(new User()
                {
                    Id = -7,
                    PhoneNumber = "11002003007",
                    FirstName = "Apple Test Account",
                    IsPhoneConfirmed = true,
                });

            modelBuilder.Entity<User>()
               .HasData(new User()
               {
                   Id = -8,
                   PhoneNumber = "11002003008",
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
                .Property(u => u.SocialInvitations)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.CompanionActivity)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.GatheringReminders)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.GatheringActivity)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.GatheringDiscovery)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .HasMany(u => u.InitiatedUserRelationships)
                .WithOne(l => l.Self)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.TargetUserRelationships)
                .WithOne(l => l.Other)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.GatheringLinks)
                .WithOne(l => l.User)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReporterList)
                .WithOne(r => r.FilingUser)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReporteeList)
                .WithOne(r => r.Other)
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
             .HasMany(u => u.ChatLinks)
             .WithOne(l => l.User)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
             .HasMany(u => u.Messages)
             .WithOne(m => m.User)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
             .HasMany(u => u.Shares)
             .WithOne(m => m.Profile)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
             .HasMany(u => u.Connections)
             .WithOne(c => c.User)
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
               .HasMany(c => c.Notifications)
               .WithOne(n => n.Gathering)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Circle>()
               .HasOne(c => c.Chat)
               .WithOne(c => c.Circle)
               .OnDelete(DeleteBehavior.Restrict);

            // Issue
            modelBuilder.Entity<Issue>()
                .HasQueryFilter(g => !g.SoftDeleted);

            // Subscription
            modelBuilder.Entity<Subscription>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.DeviceToken)
                .HasMaxLength(500);

            // Reports
            modelBuilder.Entity<Report>()
                .HasQueryFilter(w => !w.SoftDeleted);

            modelBuilder.Entity<Report>()
                .Property(r => r.Notes)
                .HasMaxLength(2000);

            modelBuilder.Entity<Report>()
                .HasDiscriminator<ReportDiscriminator>("Discriminator")
                .HasValue<SnapshotReport>(ReportDiscriminator.SnapshotReport)
                .HasValue<UserReport>(ReportDiscriminator.UserReport)
                .HasValue<CaptionReport>(ReportDiscriminator.CaptionReport);

            modelBuilder.Entity<SnapshotReport>()
                .Property(r => r.Type)
                .HasColumnName("Type");

            modelBuilder.Entity<UserReport>()
                .Property(r => r.Type)
                .HasColumnName("Type");

            modelBuilder.Entity<CaptionReport>()
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

            // Snapshot
            modelBuilder.Entity<Snapshot>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Snapshot>()
                .HasMany(s => s.Reports)
                .WithOne(r => r.Snapshot)
                .OnDelete(DeleteBehavior.Restrict);


            // Caption
            modelBuilder.Entity<Caption>()
                .HasQueryFilter(s => !s.SoftDeleted);

            modelBuilder.Entity<Caption>()
                .HasMany(s => s.Reports)
                .WithOne(r => r.Caption)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Caption>()
                .Property(c => c.Text)
                .HasMaxLength(200);

            // Feedback
            modelBuilder.Entity<Feedback>()
                .HasQueryFilter(f => !f.SoftDeleted);

            modelBuilder.Entity<Feedback>().Property(f => f.Comments)
                .HasMaxLength(300);

            // User Relationship
            modelBuilder.Entity<UserRelationship>()
                .HasQueryFilter(r => !r.SoftDeleted);

            // Circle Membership
            modelBuilder.Entity<CircleMembership>()
                .HasQueryFilter(l => !l.SoftDeleted);

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

            // Chats
            modelBuilder.Entity<Chat>()
                .HasQueryFilter(w => !w.SoftDeleted);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.ChatLinks)
                .WithOne(l => l.Chat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BroadcastChat>()
                .Property(c => c.Title)
                .HasColumnName("Title")
                .HasMaxLength(200);

            modelBuilder.Entity<Chat>()
               .HasDiscriminator<ChatType>("Type")
               .HasValue<PrivateChat>(ChatType.Individual)
               .HasValue<CircleChat>(ChatType.Circle)
               .HasValue<BroadcastChat>(ChatType.Broadcast);

            modelBuilder.Entity<BroadcastChat>()
                .HasData(new BroadcastChat()
                {
                    Id = -2,
                    Type = ChatType.Broadcast,
                    Title = "CANARY Team"
                });

            // Messages
            modelBuilder.Entity<Message>()
                .HasQueryFilter(w => !w.SoftDeleted);

            modelBuilder.Entity<Message>()
                .HasDiscriminator<MessageType>("Type")
                .HasValue<TextMessage>(MessageType.Text)
                .HasValue<PhotoMessage>(MessageType.Photo)
                .HasValue<ActivityMessage>(MessageType.Activity)
                .HasValue<ProfileMessage>(MessageType.Profile)
                .HasValue<PostMessage>(MessageType.Post)
                .HasValue<IssueMessage>(MessageType.Issue);

            modelBuilder.Entity<PostMessage>()
                .Property(g => g.GatheringId)
                .HasColumnName("GatheringId");

            modelBuilder.Entity<IssueMessage>()
                .Property(g => g.GatheringId)
                .HasColumnName("GatheringId");

            modelBuilder.Entity<TextMessage>()
                .Property(m => m.Text)
                .HasColumnName("Text")
                .HasMaxLength(2000);

            modelBuilder.Entity<ActivityMessage>()
               .Property(m => m.Text)
               .HasColumnName("Text")
               .HasMaxLength(2000);

            // Chat Links
            modelBuilder.Entity<ChatMembership>()
                .HasQueryFilter(w => !w.SoftDeleted);

            modelBuilder.Entity<ChatMembership>()
                .HasData(new ChatMembership()
                {
                    Id = -2,
                    UserId = -2,
                    ConversationId = -2,
                    Type = ChatMembershipType.Owner,
                    Muted = false,
                });

            // Connections
            modelBuilder.Entity<Connection>()
              .HasQueryFilter(c => !c.SoftDeleted);

            modelBuilder.Entity<Connection>()
             .Property(c => c.ConnectionId)
             .HasMaxLength(36);
        }
    }
}