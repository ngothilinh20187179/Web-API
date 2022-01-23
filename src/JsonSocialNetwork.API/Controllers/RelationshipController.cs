using JsonSocialNetwork.API.Classes;
using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Repositories;
using JsonSocialNetwork.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonSocialNetwork.API.Controllers
{
    [Route("")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly AccountRepository _accountRepository;
        private readonly FriendRepository _friendRepository;
        private readonly ContentRepository _contentRepository;

        public RelationshipController(
            AuthenticationService authenticationService,
            AccountRepository accountRepository,
            FriendRepository friendRepository,
            ContentRepository contentRepository)
        {
            _authenticationService = authenticationService;
            _accountRepository = accountRepository;
            _friendRepository = friendRepository;
            _contentRepository = contentRepository;
        }

        // W6 (Linh / C)
        [Route("set_request_friend")]
        [HttpPost]
        public async Task<object> SetRequestFriend(
            [FromQuery] string token,
            [FromQuery] string user_id)
        {
            if (Validation.IsEmpty(token, user_id)) return new JsonResponse(1002);
            if (!Validation.IsID(user_id)) return new JsonResponse(1004);

            int senderId;
            try
            {
                senderId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            int receiverId = int.Parse(user_id);
            if (senderId == receiverId) return new JsonResponse(1004);

            try
            {
                _ = await _accountRepository.GetAsync(receiverId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            try
            {
                _ = await _friendRepository.GetRequestFriendAsync(senderId, receiverId);
                return new JsonResponse(1004);
            }
            catch (Exception)
            {

            }

            try
            {
                _ = await _friendRepository.GetAsync(senderId, receiverId);
                return new JsonResponse(1004);
            }
            catch (Exception)
            {

            }

            try
            {
                _ = await _friendRepository.GetRequestFriendAsync(receiverId, senderId);
                _ = await _friendRepository.AddAsync(senderId, receiverId);
                _ = await _friendRepository.DeleteRequestFriendAsync(receiverId, senderId);
            }
            catch (Exception)
            {
                _ = await _friendRepository.AddRequestFriendAsync(senderId, receiverId);
            }

            int countRequestedFriends = await _friendRepository.CountRequestedFriendsAsync(senderId);

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    requested_friends = countRequestedFriends.ToString()
                }
            };
        }


        // W6 (Linh / RR)
        // (bỏ trống user_id tức lấy danh sách bạn của chính người đang đăng nhập)
        // Chỉ chấp nhận tham số user_id nếu request là từ phía trang quản trị, ứng dụng nếu truyền user_id là của người khác thì sẽ coi là không truyền tham số này.
        [Route("get_requested_friend")]
        [HttpPost]
        public async Task<object> GetRequestedFriend(
            [FromQuery] string token,
            [FromQuery] string index,
            [FromQuery] string count,
            [FromQuery] string? user_id = null)
        {
            if (Validation.IsEmpty(token, index, count)) return new JsonResponse(1002);
            if (!Validation.IsID(count)) return new JsonResponse(1004);
            if (!Regex.IsMatch(index, @"^[0-9]+$")) return new JsonResponse(1004);
            if (user_id != null && !Validation.IsID(user_id)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            List<FriendRequest> friendRequestList = new();
            friendRequestList.AddRange(await _friendRepository.GetAllFriendRequestAsync(accountId));

            int countFriendRequest = friendRequestList.Count;
            if (countFriendRequest == 0)
            {
                return new JsonResponse(9994);
            }

            int start = int.Parse(index);
            if (start >= countFriendRequest)
            {
                return new JsonResponse(1004);
            }

            int number = int.Parse(count);
            if (number >= countFriendRequest)
            {
                number = countFriendRequest - start;
            }

            List<FriendRequest> friendRequests = friendRequestList.GetRange(start, number);
            List<object> friends = new();

            foreach (FriendRequest friendRequest in friendRequests)
            {
                Account sender = await _accountRepository.GetAsync(friendRequest.SenderAccountId);
                string contentFileName = await _contentRepository.GetFileNameAsync(friendRequest.SenderAccountId);
                friends.Add(new
                {
                    id = sender.Id.ToString(),
                    username = sender.Name,
                    avatar = $"/content/{contentFileName}"
                });
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    friends
                }
            };
        }


        // W6 (Linh / C)
        [Route("set_accept_friend")]
        [HttpPost]
        public async Task<JsonResponse> SetAcceptFriend(
            [FromQuery] string token,
            [FromQuery] string user_id,
            [FromQuery] string is_accept)
        {
            if (Validation.IsEmpty(token, user_id, is_accept)) return new JsonResponse(1002);
            if (!Validation.IsID(user_id)) return new JsonResponse(1004);
            if (is_accept != "0" && is_accept != "1") return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            int userId = int.Parse(user_id);
            if (accountId == userId) return new JsonResponse(1004);

            try
            {
                _ = await _accountRepository.GetAsync(userId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            try
            {
                _ = await _friendRepository.GetRequestFriendAsync(userId, accountId);
            }
            catch (Exception)
            {
                return new JsonResponse(1004);
            }

            _ = await _friendRepository.DeleteRequestFriendAsync(userId, accountId);
            if (is_accept == "1")
            {
                _ = await _friendRepository.AddAsync(accountId, userId);
            }

            return new JsonResponse(1000);
        }


        // W5 (Linh / C)
        [Route("get_user_friends")]
        [HttpPost]
        public async Task<object> GetUserFriends(
            [FromQuery] string token,
            [FromQuery] string index,
            [FromQuery] string count,
            [FromQuery] string? user_id = null)
        {
            if (Validation.IsEmpty(token, index, count)) return new JsonResponse(1002);
            if (!Validation.IsID(count)) return new JsonResponse(1004);
            if (!Regex.IsMatch(index, @"^[0-9]+$")) return new JsonResponse(1004);
            if (user_id != null && !Validation.IsID(user_id)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            List<Friend> friendList = new();
            friendList.AddRange(await _friendRepository.GetAllAsync(accountId));

            int countFriend = friendList.Count;
            if (countFriend == 0)
            {
                return new JsonResponse(9994);
            }

            int start = int.Parse(index);
            if (start >= countFriend)
            {
                return new JsonResponse(1004);
            }

            int number = int.Parse(count);
            if (number >= countFriend)
            {
                number = countFriend - start;
            }

            List<Friend> partialFriendsList = friendList.GetRange(start, number);
            List<object> friends = new();

            foreach (Friend friend in partialFriendsList)
            {
                int friendId = (accountId == friend.SmallerAccountId) ? friend.BiggerAccountId : friend.SmallerAccountId;
                Account user = await _accountRepository.GetAsync(friendId);
                string avatarFileName = await _contentRepository.GetFileNameAsync(user.Id);
                friends.Add(new
                {
                    user_id = user.Id.ToString(),
                    user_name = user.Name,
                    avatar = $"/content/{avatarFileName}"
                });
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    friends
                }
            };
        }


        // W7 (Tuan - Thai / C)
        [Route("set_block_user")]
        [HttpPost]
        public async Task<JsonResponse> SetBlock(
            [FromQuery] string token,
            [FromQuery] string user_id,
            [FromQuery] string type)
        {
            if (Validation.IsEmpty(token, user_id)) return new JsonResponse(1002);
            if (!Validation.IsID(user_id)) return new JsonResponse(1004);
            if (!Validation.IsBool(type)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            int blockedUserId = int.Parse(user_id);
            try
            {
                _ = await _accountRepository.GetAsync(blockedUserId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            try
            {
                if (type == "1")
                    await _accountRepository.AddBlockAsync(accountId, blockedUserId);
                else
                    await _accountRepository.DeleteBlockAsync(accountId, blockedUserId);
            }
            catch (Exception)
            {
                return new JsonResponse(1010);
            }

            return new JsonResponse(1000);
        }
    }
}
