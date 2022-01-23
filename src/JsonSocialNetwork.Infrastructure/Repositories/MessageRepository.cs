using Dapper;
using JsonSocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public class MessageRepository : BaseRepository
    {
        #region Message
        public Task<Message> GetAsync(int messageId)
        {
            return DBConnection.QuerySingleAsync<Message>(
                @"SELECT * FROM [dbo].[messages] WHERE id=@Id",
                new { Id = messageId });
        }

        public Task<IEnumerable<Message>> GetAllAsync(int conversationId)
        {
            return DBConnection.QueryAsync<Message>(
                @"SELECT *
                    FROM [dbo].[messages]
                    WHERE conversation_id=@Id
                    ORDER BY id ASC",
                new { Id = conversationId });
        }

        public Task<IEnumerable<Message>> GetMessagesAsync(int conversationId, int count)
        {
            return DBConnection.QueryAsync<Message>(
                @"UPDATE [dbo].[messages]
                    SET is_read=1
                    FROM (SELECT TOP (@Count) *
                            FROM [dbo].[messages]
                            WHERE conversation_id=@Id
                            ORDER BY id
                            DESC) AS q
                    WHERE q.id=[dbo].[messages].id
                  SELECT *
                    FROM (SELECT TOP (@Count) *
                            FROM [dbo].[messages]
                            WHERE conversation_id=@Id
                            ORDER BY id
                            DESC) AS q
                    ORDER BY q.id
                    ASC",
                new { Id = conversationId, Count = count });
        }

        public Task<IEnumerable<Message>> GetMessagesFromAsync(int conversationId, int lastMessageId)
        {
            return DBConnection.QueryAsync<Message>(
                @"SELECT * FROM [dbo].[messages] WHERE conversation_id=@C AND id>@M",
                new { C = conversationId, M = lastMessageId });
        }

        public Task<Message> GetLastAsync(int conversationId)
        {
            return DBConnection.QuerySingleAsync<Message>(
                @"SELECT TOP 1 * FROM [dbo].[messages] WHERE conversation_id=@Id ORDER BY id DESC",
                new { Id = conversationId });
        }

        public Task<int> CountNewAsync(int conversationId)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(id) FROM [dbo].[messages] WHERE conversation_id=@Id AND is_read=0",
                new { Id = conversationId });
        }


        public Task AddAsync(string body, int authorId, int conversationId)
        {
            return Task.Run(() =>
            {
                DBConnection.Execute(
                    @"INSERT INTO [dbo].[messages] (body,date_created,is_read,author_account_id,conversation_id) 
                          VALUES (@B,@D,@I,@A,@C)",
                new
                {
                    B = body,
                    D = DateTime.Now,
                    I = false,
                    A = authorId,
                    C = conversationId
                });
            });
        }

        public Task<int> DeleteAsync(int id)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[messages] WHERE id=@Id",
                new { Id = id });
        }
        #endregion

        #region Conversation
        public Task<Conversation> GetConversationAsync(int conversationId)
        {
            return DBConnection.QuerySingleAsync<Conversation>(
                @"SELECT * FROM [dbo].[conversations] WHERE id=@Id",
                new { Id = conversationId });
        }

        public Task<Conversation> GetConversationAsync(int ownerAccountId, int partnerAccountId)
        {
            return DBConnection.QuerySingleAsync<Conversation>(
                @"SELECT TOP 1 * FROM [dbo].[conversations] WHERE owner_account_id=@O AND partner_account_id=@P",
                new { O = ownerAccountId, P = partnerAccountId });
        }

        public Task<IEnumerable<Conversation>> GetAllConversationAsync(int ownerAccountId)
        {
            return DBConnection.QueryAsync<Conversation>(
                @"SELECT *
                    FROM [dbo].[conversations]
                    WHERE owner_account_id=@Id
                    ORDER BY partner_account_id ASC",
                new { Id = ownerAccountId });
        }

        public Task<Conversation> AddConversationAsync(int ownerAccountId, int partnerAccountId)
        {
            return DBConnection.QuerySingleAsync<Conversation>(
                @"INSERT INTO [dbo].[conversations] (owner_account_id,partner_account_id) VALUES (@O,@P);
                  SELECT * FROM [dbo].[conversations] WHERE id=SCOPE_IDENTITY();",
                new { O = ownerAccountId, P = partnerAccountId });
        }
        public Task<int> DeleteConversationAsync(int conversationId)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[conversations] WHERE id=@Id",
                new {Id = conversationId});
        }

        public Task<int> DeleteConversationAsync(int ownerAccountId, int partnerAccountId)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[conversations] WHERE owner_account_id=@Id1 AND partner_account_id=@Id2",
                new { Id1 = ownerAccountId, Id2 = partnerAccountId });
        } 
        #endregion
    }
}
