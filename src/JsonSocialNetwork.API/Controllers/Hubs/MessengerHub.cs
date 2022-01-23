using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Repositories;
using JsonSocialNetwork.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace JsonSocialNetwork.API.Controllers.Hubs
{
    public class MessengerHub : Hub
    {
        private readonly AuthenticationService _authenticationService;
        public MessengerHub(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }


        public async Task Sent(string senderId, string receiverId)
        {
            string? connectionIdS = _authenticationService.GetConnectionId(int.Parse(senderId));
            string? connectionIdR = _authenticationService.GetConnectionId(int.Parse(receiverId));

            if (connectionIdS != null)
                await Clients.Client(connectionIdS).SendAsync("Update", senderId);
            if (connectionIdR != null)
                await Clients.Client(connectionIdR).SendAsync("Update", senderId);
        }

        public void Initiate(string id)
            => _authenticationService.InitiateMessengerConnection(int.Parse(id), Context.ConnectionId);

        public void Close(string id)
            => _authenticationService.CloseMessengerConnection(int.Parse(id));
    }
}
