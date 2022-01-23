using JsonSocialNetwork.API.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Services;
using JsonSocialNetwork.Infrastructure.Repositories;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace JsonSocialNetwork.API.Controllers
{
    [Route("")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly AccountRepository _accountRepository;
        private readonly ContentRepository _contentRepository;
        private readonly FriendRepository _friendRepository;

        public AdminController(
            AuthenticationService authenticationService,
            AccountRepository accountRepository,
            FriendRepository friendRepository,
            ContentRepository contentRepository)
        {
            _authenticationService = authenticationService;
            _accountRepository = accountRepository;
            _contentRepository = contentRepository;
            _friendRepository = friendRepository;
        }

        // W5 (Linh / RR)
        [Route("get_user_basic_info")]
        [HttpPost]
        public async Task<object> GetUserBasicInfo(
            [FromQuery] string token,
            [FromQuery] string user_id)
        {
            if (Validation.IsEmpty(token, user_id)) return new JsonResponse(1002);
            if (!Validation.IsID(user_id)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            Account account;
            try
            {
                account = await _accountRepository.GetAsync(accountId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            if (!account.IsAdmin) return new JsonResponse(1009);

            int userId = int.Parse(user_id);
            Account user;
            try
            {
                user = await _accountRepository.GetAsync(userId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            string contentFileName = await _contentRepository.GetFileNameAsync(user.Id);
            int countFriend = await _friendRepository.CountFriendsAsync(userId);
            string address = await _accountRepository.GetAddressAsync(userId);
            string[] arrListStr = address.Split(new char[] { ',' });

            if (userId == accountId)
            {
                return new
                {
                    code = JsonResponse.GetCode(1000),
                    message = JsonResponse.GetMessage(1000),
                    data = new
                    {
                        id = user.Id.ToString(),
                        username = user.Name,
                        created = user.DateCreated,
                        description = user.Description,
                        avatar = $"/content/{contentFileName}",
                        address = arrListStr[2],
                        city = arrListStr[1],
                        country = arrListStr[0],
                        listing = countFriend.ToString()
                    }
                };
            }
            else
            {
                bool isOnline = _authenticationService.IsOnline(userId);
                bool isAdmin = true;
                if (!user.IsAdmin) isAdmin = false;
                return new
                {
                    code = JsonResponse.GetCode(1000),
                    message = JsonResponse.GetMessage(1000),
                    data = new
                    {
                        id = user.Id.ToString(),
                        username = user.Name,
                        created = user.DateCreated,
                        description = user.Description,
                        avatar = $"/content/{contentFileName}",
                        address = arrListStr[2],
                        city = arrListStr[1],
                        country = arrListStr[0],
                        listing = countFriend.ToString(),
                        online = isOnline ? "1" : "0",
                        is_admin = isAdmin ? "1" : "0"
                    }
                };
            }
        }

        // W4 (Linh / C)
        [Route("get_user_list")]
        [HttpPost]
        public async Task<object> GetUserList(
            [FromQuery] string token,
            [FromQuery] string index = "0",
            [FromQuery] string count = "20")
        {
            if (Validation.IsEmpty(token, index, count)) return new JsonResponse(1002);
            if (!Validation.IsID(count)) return new JsonResponse(1004);
            if (!Regex.IsMatch(index, @"^[0-9]+$")) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            Account account;
            try
            {
                account = await _accountRepository.GetAsync(accountId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            if (!account.IsAdmin) return new JsonResponse(1009);

            List<Account> userList = new();
            userList.AddRange(await _accountRepository.GetAllAsync());

            int countUser = userList.Count;
            if (countUser == 0)
            {
                return new JsonResponse(9994);
            }

            int start = int.Parse(index);
            if (start >= countUser)
            {
                return new JsonResponse(1004);
            }

            int number = int.Parse(count);
            if (number >= countUser)
            {
                number = countUser - start;
            }

            List<Account> partiaUserList = userList.GetRange(start, number);
            List<object> data = new();

            foreach (Account user in partiaUserList)
            {
                string contentFileName = await _contentRepository.GetFileNameAsync(user.Id);
                data.Add(new
                {
                    user_id = user.Id.ToString(),
                    user_name = user.Name,
                    avatar = $"/content/{contentFileName}",
                    is_active = contentFileName == @"default_avatar.png" ? "-1" : "1"
                });
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data
            };
        }


        // W5 (Thau / C)
        [Route("delete_user")]
        [HttpPost]
        public async Task<JsonResponse> DeleteUser(
            [FromQuery] string token,
            [FromQuery] string user_id)
        {
            if (Validation.IsEmpty(token, user_id)) return new JsonResponse(1002);
            if (!Validation.IsID(user_id)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            Account account;
            try
            {
                account = await _accountRepository.GetAsync(accountId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            if (account.IsAdmin != true) return new JsonResponse(1009);

            int idUser = int.Parse(user_id);
            var userId = await _accountRepository.GetAsync(idUser);
            if (account.IsAdmin == userId.IsAdmin) return new JsonResponse(9997);
            _ = await _accountRepository.DeleteAsync(idUser);

            return new JsonResponse(1000);
        }


        // W4 (Linh / C)
        [Route("set_role")]
        [HttpPost]
        public async Task<JsonResponse> SetRole(
            [FromQuery] string token,
            [FromQuery] string user_id,
            [FromQuery] string is_admin)
        {
            if (Validation.IsEmpty(token, user_id, is_admin)) return new JsonResponse(1002);
            if (!Validation.IsID(user_id)) return new JsonResponse(1004);
            if (!Validation.IsBool(is_admin)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            Account account;
            try
            {
                account = await _accountRepository.GetAsync(accountId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            if (!account.IsAdmin) return new JsonResponse(1009);

            int userId = int.Parse(user_id);

            if (userId == accountId && is_admin == "1")
            {
                return new JsonResponse(1010);
            }
            else if (userId == accountId && is_admin == "0")
            {
                _ = await _accountRepository.UpdateRoleAsync(userId, is_admin);
                return new JsonResponse(1000);
            }

            Account user;
            try
            {
                user = await _accountRepository.GetAsync(userId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            if (user.IsAdmin && is_admin == "1" || !user.IsAdmin && is_admin == "0")
            {
                return new JsonResponse(1010);
            }
            else if (user.IsAdmin && is_admin == "0" || !user.IsAdmin && is_admin == "1")
            {
                _ = await _accountRepository.UpdateRoleAsync(userId, is_admin);
            }

            return new JsonResponse(1000);
        }
    }
}
