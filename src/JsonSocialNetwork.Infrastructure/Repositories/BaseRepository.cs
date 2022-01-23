using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace JsonSocialNetwork.Infrastructure.Repositories
{
    public abstract class BaseRepository
    {
        static BaseRepository()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            DBConnection = new SqlConnection(File.ReadAllText(ResourcePath + @"\ConnectionString.txt"));
        }

        protected static readonly string ResourcePath = @"..\..\Data";
        protected static readonly IDbConnection DBConnection;
    }
}
