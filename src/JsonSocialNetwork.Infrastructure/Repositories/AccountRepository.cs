using Dapper;
using JsonSocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public class AccountRepository : BaseRepository
    {
        #region Account
        public Task<Account> GetAsync(int accountId)
        {
            return DBConnection.QuerySingleAsync<Account>(
                @"SELECT * FROM [dbo].[accounts] WHERE id=@Id",
                new { Id = accountId });
        }

        public Task<Account> GetAsync(string phoneNumber)
        {
            return DBConnection.QuerySingleAsync<Account>(
                @"SELECT * FROM [dbo].[accounts] WHERE phone=@Phone",
                new { Phone = phoneNumber });
        }

        public Task<string> GetAddressAsync(int accountId)
        {
            return DBConnection.QuerySingleAsync<string>(
                @"SELECT address FROM [dbo].[accounts] WHERE id=@Id",
                new { Id = accountId });
        }

        public Task<int> AddAsync(string phoneNumber, string password)
        {
            return DBConnection.ExecuteAsync(
                @"INSERT INTO [dbo].[accounts] (name,password,phone,date_created,is_admin) VALUES (@Phone,@Password,@Phone,@Date,@A);",
                new { Phone = phoneNumber, Password = password, Date = DateTime.Now, A = false });
        }

        public Task<int> DeleteAsync(int id)
        {
            return DBConnection.ExecuteAsync(
                @"DELETE FROM [dbo].[accounts] WHERE id=@Id",
                new { Id = id }); 
        }

        public Task<int> UpdateAsync(int id, string newPassword)
        {
            return DBConnection.ExecuteAsync(
                @"UPDATE [dbo].[accounts] SET password=@Password WHERE id=@Id",
                new { Id = id, Password = newPassword });
        }

        public Task<int> UpdateAsync(int id, string username, string description, string address)
        {
            return DBConnection.ExecuteAsync(
                @"UPDATE [dbo].[accounts] SET phone=@U,description=@D,address=@A WHERE id=@Id",
                new { U = username, D = description, A = address, Id = id });
        }
        #endregion

        #region Block
        public Task<Block> GetBlockAsync(int blockerId, int blockedId)
        {
            return DBConnection.QuerySingleAsync<Block>(
                @"SELECT * FROM [dbo].[blocks] WHERE blocker_account_id=@Blocker AND blocked_account_id=@Blocked",
                new { Blocker = blockerId, Blocked = blockedId });
        }

        public Task AddBlockAsync(int blockerId, int blockedId)
        {
            return Task.Run(() =>
            {
                DBConnection.Execute(
                    @"INSERT INTO [dbo].[blocks] (blocker_account_id,blocked_account_id) VALUES (@A,@B)",
                    new { A = blockerId, B = blockedId });
            });
        }

        public Task DeleteBlockAsync(int blockerId, int blockedId)
        {
            return Task.Run(() =>
            {
                DBConnection.Execute(
                    @"DELETE FROM [dbo].[blocks] WHERE blocker_account_id=@A AND blocked_account_id=@B",
                    new { A = blockerId, B = blockedId });
            });
        }
        #endregion

        #region Admin
        public Task<int> UpdateRoleAsync(int id, string is_admin)
        {
            return DBConnection.ExecuteAsync(
                @"UPDATE [dbo].[accounts] SET is_admin=@IsAdmin WHERE id=@Id",
                new { Id = id, IsAdmin = is_admin });
        }


        public Task<IEnumerable<Account>> GetAllAsync()
        {
            return DBConnection.QueryAsync<Account>(
                @"SELECT * FROM [dbo].[accounts] WHERE is_admin=@IsAdmin",
                new { IsAdmin = false });
        }
        #endregion
    }
}