using JsonSocialNetwork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace JsonSocialNetwork.MigrationEF
{
    public class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(File.ReadAllText(@"..\..\Data\ConnectionString.txt"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasAlternateKey(e => e.Phone);

            modelBuilder.Entity<Avatar>()
                .HasOne<Account>()
                .WithOne()
                .HasForeignKey<Avatar>(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Avatar>()
                .HasOne<Content>()
                .WithMany()
                .HasForeignKey(e => e.ContentFileName)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Block>()
                .HasKey(e => new { e.BlockerAccountId, e.BlockedAccountId });
            modelBuilder.Entity<Block>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.BlockerAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Block>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.BlockedAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.AuthorAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Comment>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(e => e.OwnerPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Content>()
                .HasData(
                    new Content { ContentType = "image/png", FileName = "default_avatar.png" },
                    new Content { ContentType = "image/png", FileName = "404_favicon.png" },
                    new Content { ContentType = "image/jpeg", FileName = "404_page.jpg" }
                );

            modelBuilder.Entity<Conversation>()
                .HasAlternateKey(e => new { e.OwnerAccountId, e.PartnerAccountId });
            modelBuilder.Entity<Conversation>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.OwnerAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Conversation>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.PartnerAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Friend>()
                .HasKey(e => new { e.SmallerAccountId, e.BiggerAccountId });
            modelBuilder.Entity<Friend>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.SmallerAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Friend>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.BiggerAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasKey(e => new { e.SenderAccountId, e.ReceiverAccountId });
            modelBuilder.Entity<FriendRequest>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.SenderAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<FriendRequest>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.ReceiverAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Like>()
                .HasKey(e => new { e.AuthorAccountId, e.OwnerPostId });
            modelBuilder.Entity<Like>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.AuthorAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Like>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(e => e.OwnerPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.AuthorAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Message>()
                .HasOne<Conversation>()
                .WithMany()
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasOne<Account>()
                .WithMany()
                .HasForeignKey(e => e.AuthorAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostContent>()
                .HasNoKey();
            modelBuilder.Entity<PostContent>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostContent>()
                .HasOne<Content>()
                .WithMany()
                .HasForeignKey(e => e.ContentFileName)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne<Post>()
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
