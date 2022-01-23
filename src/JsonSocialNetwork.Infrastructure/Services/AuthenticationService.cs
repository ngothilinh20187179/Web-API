using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSocialNetwork.Infrastructure.Services
{
    public class AuthenticationService
    {
        // ID - token
        private readonly Dictionary<int, string> _tokenConnections = new();

        public string InitiateTokenConnection(int accountId)
        {
            string token = Guid.NewGuid().ToString();
            _tokenConnections[accountId] = token;
            Console.WriteLine($"Initiate Token Connection: {accountId} - {token}");
            return token;
        }

        public bool CloseTokenConnection(string token)
        {
            int key;
            try
            {
                key = GetIdByToken(token);
            }
            catch (Exception)
            {
                return false;
            }

            Console.WriteLine($"Close Token Connection: {key}");
            return _tokenConnections.Remove(key);
        }

        public int GetIdByToken(string token)
        {
            foreach (var connection in _tokenConnections)
            {
                if (connection.Value == token)
                    return connection.Key;
            }
            throw new Exception("Invalid token!");
        }

        public bool IsOnline(int accountId)
            => _tokenConnections.ContainsKey(accountId);


        // SignalR: ID - connectionId
        private readonly Dictionary<int, string> _messengerConnections = new();
        public string? GetConnectionId(int accountId)
            => _messengerConnections.GetValueOrDefault(accountId);

        public void InitiateMessengerConnection(int accountId, string connectionId)
        {
            _messengerConnections[accountId] = connectionId;
            Console.WriteLine($"Initiate Messenger Connection: {accountId} - {connectionId}");
        }

        public void CloseMessengerConnection(int accountId)
        {
            _messengerConnections.Remove(accountId);
            Console.WriteLine($"Close Messenger Connection: {accountId}");
        }
    }
}
