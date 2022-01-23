using Dapper;
using JsonSocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public class PostRepository : BaseRepository
    {
        #region Post
        public Task<Post> GetAsync(int id)
        {
            string sql = @"SELECT * FROM [dbo].[posts] WHERE id=@Id";
            return DBConnection.QuerySingleAsync<Post>(sql, new { Id = id });
        }

        public Task<IEnumerable<Post>> GetAllAsync(int index, int count)
        {
            return DBConnection.QueryAsync<Post>(
                @"SELECT TOP(@Count) * FROM [dbo].[posts] WHERE id>=@Index",
                new { Count = count, Index = index });
        }

        public Task<Post> GetLastAsync()
        {
            return DBConnection.QuerySingleAsync<Post>(@"SELECT TOP (1) * FROM [dbo].[posts] ORDER BY id DESC");
        }

        public Task<int> CountNewAsync(int index, int count)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(id) FROM [dbo].[posts] WHERE id>=@Index",
                new { Index = index + count });
        }

        public Task<int> AddAsync(string described, int accountId)
        {
            DateTime dateTime = DateTime.Now;
            return DBConnection.QuerySingleAsync<int>(
                @"INSERT INTO [dbo].[posts] (body,date_created,date_modified,author_account_id) VALUES (@B,@C,@M,@A);
                  SELECT SCOPE_IDENTITY();",
                new { B = described, C = dateTime, M = dateTime, A = accountId });
        }

        public Task UpdateAsync(int postId, string body)
        {
            return Task.Run(() =>
            {
                DBConnection.Execute(
                    @"UPDATE [dbo].[posts] SET body=@B, date_modified=@D WHERE id=@Id",
                    new { B = body, D = DateTime.Now, Id = postId });
            });
        }

        public Task<int> DeleteAsync(int id)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[posts] WHERE id=@Id",
                new { Id = id });
        }
        #endregion

        public Task<string> GetInteractorsAsync(int postId)
        {
            return Task.Run(() =>
            {
                string result = string.Empty;
                List<int> ids = DBConnection.Query<int>(
                    @"SELECT author_account_id FROM [dbo].[likes] WHERE owner_post_id=@PostId",
                    new { PostId = postId }).ToList();
                ids.AddRange(DBConnection.Query<int>(
                    @"SELECT author_account_id FROM [dbo].[comments] WHERE owner_post_id=@PostId",
                    new { PostId = postId }));
                List<string> names = new();
                foreach (int id in ids.Distinct())
                {
                    string name = DBConnection.QuerySingle<string>(
                        @"SELECT name FROM [dbo].[accounts] WHERE id=@Id",
                        new { Id = id });
                    result += $"{name},";
                }
                return string.IsNullOrWhiteSpace(result) ? result : result[..^1];
            });
        }

        #region Report
        public Task<int> GetReportCountAsync(int postId)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(id) FROM [dbo].[reports] WHERE post_id=@Id",
                new { Id = postId });
        }

        public Task<int> AddReportAsync(string subject, string? details, int postId)
        {
            var sql = @"INSERT INTO [dbo].[reports] (subject, detail, post_id) VALUES (@S, @D, @P)";
            return DBConnection.ExecuteAsync(sql, new { S = subject, D = details, P = postId });
        }
        #endregion

        #region Content
        public Task<IEnumerable<PostContent>> GetAllPostContentAsync(int postId)
        {
            return DBConnection.QueryAsync<PostContent>(
                @"SELECT *
                    FROM [dbo].[post_contents]
                    WHERE post_id=@Id
                    ORDER BY order_id ASC",
                new { Id = postId });
        }

        public Task<int> AddPostContentAsync(string fileName, int postId, int orderId)
        {
            return DBConnection.ExecuteAsync(
                @"INSERT INTO [dbo].[post_contents] (order_id,post_id,content_file_name) VALUES (@O,@P,@C)",
                new { O = orderId, P = postId, C = fileName });
        }

        public Task DeletePostContentAsync(int postId, int orderId)
        {
            return Task.Run(() =>
            {
                DBConnection.Execute(
                    @"DELETE FROM [dbo].[post_contents] WHERE order_id=@O AND post_id=@P",
                new { O = orderId, P = postId });
            });
        }

        public Task<int> CountPostContentAsync(int postId, int startId, int endId)
        {
            return DBConnection.QuerySingleAsync<int>(
                $"SELECT COUNT(*) FROM [dbo].[post_contents] WHERE post_id=@P AND order_id >= {startId} AND order_id <= {endId}",
                new { P = postId });
        }
        #endregion

        #region Like
        public Task<int> CountLikeAsync(int postId)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(owner_post_id) FROM [dbo].[likes] WHERE owner_post_id=@OwnerPostId",
                new { OwnerPostId = postId });
        }
        public Task<int> CountLikeAsync(int postId, int accountId)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(*) FROM [dbo].[likes] WHERE owner_post_id=@P AND author_account_id=@A",
                new { P = postId, A = accountId });
        }

        public Task<int> AddLikeAsync(int idAccount, int idPost)
        {
            return DBConnection.ExecuteAsync(
                @"INSERT INTO [dbo].[likes] (author_account_id, owner_post_id) VALUES (@AuthorAccountId,@OwnerPostId)",
                new { AuthorAccountId = idAccount, OwnerPostId = idPost });
        }

        public Task<int> DeleteLikeAsync(int idAccount, int idPost)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[likes] WHERE author_account_id=@AuthorAccountId and owner_post_id=@OwnerPostId",
                new { AuthorAccountId = idAccount, OwnerPostId = idPost });
        }
        #endregion
    }
}
