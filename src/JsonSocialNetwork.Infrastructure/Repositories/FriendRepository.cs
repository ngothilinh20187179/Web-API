using Dapper;
using JsonSocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public class FriendRepository : BaseRepository
    {
        #region Friend
        public Task<Friend> GetAsync(int friendIdA, int friendIdB)
        {
            string sql = @"SELECT * FROM [dbo].[friends] WHERE smaller_account_id=@SmallerAccountId AND bigger_account_id=@BiggerAccountId";
            if (friendIdA > friendIdB)
            {
                return DBConnection.QuerySingleAsync<Friend>(sql, new { SmallerAccountId = friendIdB, BiggerAccountId = friendIdA });
            }
            else
            {
                return DBConnection.QuerySingleAsync<Friend>(sql, new { SmallerAccountId = friendIdA, BiggerAccountId = friendIdB });
            }
        }

        public Task<IEnumerable<Friend>> GetAllAsync(int accountId)
        {
            return DBConnection.QueryAsync<Friend>(
                @"SELECT * FROM [dbo].[friends] WHERE smaller_account_id=@SmallerAccountId OR bigger_account_id=@BiggerAccountId",
                new { SmallerAccountId = accountId, BiggerAccountId = accountId });
        }


        public Task<int> AddAsync(int friendIdA, int friendIdB)
        {
            string sql = @"INSERT INTO [dbo].[friends] (smaller_account_id, bigger_account_id) VALUES (@SmallerAccountId, @BiggerAccountId)";
            if (friendIdA > friendIdB)
            {
                return DBConnection.ExecuteAsync(sql, new { SmallerAccountId = friendIdB, BiggerAccountId = friendIdA });
            }
            else
            {
                return DBConnection.ExecuteAsync(sql, new { SmallerAccountId = friendIdA, BiggerAccountId = friendIdB });
            }
        }

        public Task<int> CountFriendsAsync(int accountId)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(*) FROM [dbo].[friends] WHERE smaller_account_id=@SmallerAccountId OR bigger_account_id=@BiggerAccountId",
                new { SmallerAccountId = accountId, BiggerAccountId = accountId });
        }
        #endregion

        #region FriendRequest
        public Task<FriendRequest> GetRequestFriendAsync(int senderId, int receiverId)
        {
            return DBConnection.QuerySingleAsync<FriendRequest>(
                @"SELECT * FROM [dbo].[friend_requests] WHERE sender_account_id=@SenderAccountId AND receiver_account_id=@ReceiverAccountId",
                new
                {
                    SenderAccountId = senderId,
                    ReceiverAccountId = receiverId
                });
        }

        public Task<IEnumerable<FriendRequest>> GetAllFriendRequestAsync(int receiverId)
        {
            return DBConnection.QueryAsync<FriendRequest>(
                @"SELECT * FROM [dbo].[friend_requests] WHERE receiver_account_id=@ReceiverAccountId",
                new { ReceiverAccountId = receiverId });
        }

        public Task<int> AddRequestFriendAsync(int senderId, int receiverId)
        {
            return DBConnection.ExecuteAsync(
                @"INSERT INTO [dbo].[friend_requests] (sender_account_id, receiver_account_id) VALUES (@SenderAccountId, @ReceiverAccountId)",
                new
                {
                    SenderAccountId = senderId,
                    ReceiverAccountId = receiverId
                });
        }

        public Task<int> CountRequestedFriendsAsync(int accountId)
        {
            return DBConnection.QuerySingleAsync<int>(
                @"SELECT COUNT(sender_account_id) FROM [dbo].[friend_requests] WHERE sender_account_id=@SenderAccountId",
                new { SenderAccountId = accountId });
        }

        public Task<int> DeleteRequestFriendAsync(int senderId, int receiverId)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[friend_requests] WHERE sender_account_id=@SenderAccountId AND receiver_account_id=@ReceiverAccountId",
                new
                {
                    SenderAccountId = senderId,
                    ReceiverAccountId = receiverId
                });
        }
        #endregion
    }
}
