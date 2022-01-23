using Dapper;
using JsonSocialNetwork.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public class ContentRepository : BaseRepository
    {
        private string _contentDirectory = ResourcePath + @"\Content";


        public Task<Content> GetAsync(string fileName)
        {
            return DBConnection.QuerySingleAsync<Content>(
                @"SELECT * FROM [dbo].[contents] WHERE file_name=@FileName",
                new { FileName = fileName });
        }

        public Task<string> GetFileNameAsync(int accountId)
        {
            return Task.Run(
                () =>
                {
                    string res;
                    try
                    {
                        res = DBConnection.QuerySingle<string>(
                            @"SELECT [content_file_name] FROM [dbo].[avatars] WHERE account_id=@Id",
                            new { Id = accountId }
                        );
                    }
                    catch (Exception)
                    {
                        res = "default_avatar.png";
                    }
                    return res;
                });
        }

        public Task<(byte[], Content)> GetFileDataAsync(string fileName)
        {
            return Task.Run(
                () =>
                {
                    Content content;
                    try
                    {
                        content = DBConnection.QuerySingle<Content>(
                            @"SELECT * FROM [dbo].[contents] WHERE file_name=@FileName",
                            new { FileName = fileName });
                    }
                    catch (Exception)
                    {
                        content = DBConnection.QuerySingle<Content>(
                            @"SELECT * FROM [dbo].[contents] WHERE file_name=@FileName",
                            new { FileName = "default_avatar.png" });
                    }
                    var bytes = File.ReadAllBytes($"{_contentDirectory}\\{content.FileName}");
                    return (bytes, content);
                });
        }

        public Task<string> AddAsync(byte[] data, string contentType)
        {
            return Task.Run(
                () =>
                {
                    string fileName = DateTime.Now.Ticks.ToString();
                    File.WriteAllBytes($"{_contentDirectory}\\{fileName}", data);
                    DBConnection.Execute(
                        @"INSERT INTO [dbo].[contents] (file_name,content_type) VALUES (@F,@C);",
                        new { F = fileName, C = contentType });
                    return fileName;
                });
        }
    }
}
