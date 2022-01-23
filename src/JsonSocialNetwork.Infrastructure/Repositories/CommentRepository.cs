using Dapper;
using JsonSocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public class CommentRepository : BaseRepository
    {
        public Task<Comment> GetAsync(int id)
        {
            return DBConnection.QuerySingleAsync<Comment>(
               @"SELECT * FROM [dbo].[comments] WHERE id=@Id",
               new { Id = id });
        }

        public Task<IEnumerable<Comment>> GetAllAsync(int postId)
        {
            return DBConnection.QueryAsync<Comment>(
                @"SELECT * FROM [dbo].[comments] WHERE owner_post_id=@O",
                new { O = postId });
        }

        public Task<int> AddAsync(string body, int authorId, int postId)
        {
            DateTime dateTime = DateTime.Now;
            return DBConnection.ExecuteAsync(
                @"INSERT INTO [dbo].[comments] (body,date_created,author_account_id,owner_post_id)
                    VALUES (@B,@D,@A,@O)",
                new { B = body, D = dateTime, A = authorId, O = postId });
        }

        public Task<int> UpdateAsync(int id, string newComment)
        {
            return DBConnection.ExecuteAsync(
                @"UPDATE [dbo].[comments] SET body=@Body WHERE id=@Id",
                new { Id = id, Body = newComment });
        }

        public Task<int> DeleteAsync(int id)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[comments] WHERE id=@Id",
                new { Id = id });
        }
    }
}
