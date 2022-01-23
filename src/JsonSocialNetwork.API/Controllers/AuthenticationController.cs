using JsonSocialNetwork.API.Classes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Services;
using JsonSocialNetwork.Infrastructure.Repositories;


namespace JsonSocialNetwork.API.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly AccountRepository _accountRepository;
        private readonly ContentRepository _contentRepository;
        private readonly FriendRepository _friendRepository;

        public AuthenticationController(
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


        // W1 (Thai / C)
        [Route("signup")]
        [HttpPost]
        public async Task<JsonResponse> SignUp(
            [FromQuery] string phonenumber,
            [FromQuery] string password)
        {
            if (Validation.IsEmpty(phonenumber, password)) return new JsonResponse(1002);
            if (!Validation.IsPhone(phonenumber)) return new JsonResponse(1004);
            if (!Validation.IsPassword(password)) return new JsonResponse(1004);
            if (phonenumber == password) return new JsonResponse(1004);

            try
            {
                _ = await _accountRepository.AddAsync(phonenumber, password);
            }
            catch (Exception)
            {
                return new JsonResponse(9996);
            }

            return new JsonResponse(1000);
        }


        // W1 (Thai / C)
        [Route("login")]
        [HttpPost]
        public async Task<object> Login(
            [FromQuery] string phonenumber,
            [FromQuery] string password)
        {
            if (Validation.IsEmpty(phonenumber, password)) return new JsonResponse(1002);
            if (!Validation.IsPhone(phonenumber)) return new JsonResponse(1004);
            if (!Validation.IsPassword(password)) return new JsonResponse(1004);
            if (phonenumber == password) return new JsonResponse(1004);

            Account account;
            try
            {
                account = await _accountRepository.GetAsync(phonenumber);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            if (account.Password != password) return new JsonResponse(1009);

            string contentFileName = await _contentRepository.GetFileNameAsync(account.Id);

            return new {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    id = account.Id.ToString(),
                    username = account.Name,
                    token = _authenticationService.InitiateTokenConnection(account.Id),
                    avatar = $"/content/{contentFileName}",
                    active = contentFileName == @"default_avatar.png" ? "-1" : "1"
                }
            };
        }


        // W1 (Thai / C)
        [Route("logout")]
        [HttpPost]
        public JsonResponse Logout([FromQuery] string token)
        {
            if (Validation.IsEmpty(token)) return new JsonResponse(1002);

            if (_authenticationService.CloseTokenConnection(token))
                return new JsonResponse(1000);
            else
                return new JsonResponse(1009);
        }


        // W8 (Linh / C)
        [Route("change_password")]
        [HttpPost]
        public async Task<JsonResponse> ChangePassword(
            [FromQuery] string token,
            [FromQuery] string password,
            [FromQuery] string new_password)
        {
            if (Validation.IsEmpty(token, password, new_password)) return new JsonResponse(1002);
            if (!Validation.IsPassword(password, new_password)) return new JsonResponse(1004);
            if (!Validation.IsNewPasswordValid(password, new_password)) return new JsonResponse(1004);

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

            if (account.Password != password) return new JsonResponse(1009);

            _ = await _accountRepository.UpdateAsync(accountId, new_password);

            return new JsonResponse(1000);
        }


        // W8 (Thai / C)
        [Route("set_user_info")]
        [HttpPost]
        public async Task<object> SetUserInfo(
            [FromQuery] string token,
            [FromQuery] string username,
            [FromQuery] string description,
            [FromQuery] string address,
            [FromQuery] string city,
            [FromQuery] string country)
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Validation.IsEmpty(token, description, address, city, country)) return new JsonResponse(1002);
            if (!Validation.IsPhone(username)) return new JsonResponse(1004);

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

            try
            {
                _ = await _accountRepository.GetAsync(username);
                return new JsonResponse(9996);
            }
            catch (Exception)
            {
            }

            string contentFileName = await _contentRepository.GetFileNameAsync(account.Id);

            string unifiedAddress = $"{country},{city},{address}";
            int res = await _accountRepository.UpdateAsync(accountId, username, description, unifiedAddress);
            if (res != 1) return new JsonResponse(9998);


            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    avatar = $"/content/{contentFileName}",
                    city = city,
                    country = country,
                    address = address,
                    description = description
                }
            };
        }


        // W6 (Linh / C)
        [Route("get_user_info")]
        [HttpPost]
        public async Task<object> GetUserInfo(
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
                bool isFriend = true;
                try
                {
                    _ = await _friendRepository.GetAsync(userId, accountId);
                }
                catch (Exception)
                {
                    isFriend = false;
                }
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
                        is_friend = isFriend ? "1" : "0",
                        online = isOnline ? "1" : "0"
                    }
                };
            }
        }
    }
}
