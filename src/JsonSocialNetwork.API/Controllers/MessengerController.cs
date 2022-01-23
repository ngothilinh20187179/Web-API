using JsonSocialNetwork.API.Classes;
using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Repositories;
using JsonSocialNetwork.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonSocialNetwork.API.Controllers
{
    [Route("")]
    [ApiController]
    public class MessengerController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly AccountRepository _accountRepository;
        private readonly ContentRepository _contentRepository;
        private readonly MessageRepository _messageRepository;

        public MessengerController(
            AuthenticationService authenticationService,
            AccountRepository accountRepository,
            ContentRepository contentRepository,
            MessageRepository messsageRepository)
        {
            _authenticationService = authenticationService;
            _accountRepository = accountRepository;
            _contentRepository = contentRepository;
            _messageRepository = messsageRepository;
        }


        // W4 (Thai / C)
        [Route("get_conversation")]
        [HttpPost]
        public async Task<object> GetConversation(
            [FromQuery] string token,
            [FromQuery] string index,
            [FromQuery] string count,
            [FromQuery] string? partner_Id = null,
            [FromQuery] string? conversation_Id = null
            )
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (string.IsNullOrWhiteSpace(partner_Id) == string.IsNullOrWhiteSpace(conversation_Id)) return new JsonResponse(1004);
            if (Formatter.TryParseId(partner_Id, out int partnerId) == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);
            if (Formatter.TryParseId(conversation_Id, out int conversationId) == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);
            if (Formatter.TryParseId(index, out int indexInt, true) != Formatter.Result.OK) return new JsonResponse(1004);
            if (Formatter.TryParseId(count, out int countInt) != Formatter.Result.OK) return new JsonResponse(1004);

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

            Conversation conv;
            if (partnerId != 0)
            {
                try
                {
                    conv = await _messageRepository.GetConversationAsync(accountId, partnerId);
                }
                catch (Exception)
                {
                    return new JsonResponse(9994);
                }
            }
            else
            {
                try
                {
                    conv = await _messageRepository.GetConversationAsync(conversationId);
                }
                catch (Exception)
                {
                    return new JsonResponse(9994);
                }
            }

            Account partner;
            try
            {
                partner = await _accountRepository.GetAsync(conv.PartnerAccountId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            List<object> conversation = new();
            List<Message> messages;
            try
            {
                messages = (await _messageRepository.GetAllAsync(conv.Id)).GetRange(indexInt, countInt);
            }
            catch (Exception)
            {
                return new JsonResponse(9994);
            }

            foreach(Message message in messages)
            {
                string contentFileName = await _contentRepository.GetFileNameAsync(message.AuthorAccountId);
                conversation.Add(new
                {
                    message = message.Body,
                    message_id = message.Id.ToString(),
                    unread = message.IsRead ? "0" : "1",
                    created = Formatter.PostedTime(message.DateCreated),
                    sender = new
                    {
                        id = message.AuthorAccountId.ToString(),
                        username = message.AuthorAccountId == accountId ? account.Name : partner.Name,
                        avatar = $"/content/{contentFileName}"
                    },
                });
            }

            bool isBlocked = true;
            try
            {
                await _accountRepository.GetBlockAsync(partnerId, accountId);
            }
            catch (Exception)
            {
                isBlocked = false;
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    conversation,
                    is_blocked = isBlocked ? "1" : "0"
                }
            };
        }


        // W4 (Thai / C)
        [Route("get_list_conversation")]
        [HttpPost]
        public async Task<object> GetListConversation(
            [FromQuery] string token,
            [FromQuery] string index,
            [FromQuery] string count
            )
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Formatter.TryParseId(index, out int indexInt, true) != Formatter.Result.OK) return new JsonResponse(1004);
            if (Formatter.TryParseId(count, out int countInt) != Formatter.Result.OK) return new JsonResponse(1004);

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

            List<Conversation> cons;
            try
            {
                cons = (await _messageRepository.GetAllConversationAsync(accountId)).GetRange(indexInt, countInt);
            }
            catch (Exception)
            {
                return new JsonResponse(9994);
            }
            

            int countNew = 0;
            List<object> data = new();
            foreach (Conversation con in cons)
            {
                Account partner;
                try
                {
                    partner = await _accountRepository.GetAsync(con.PartnerAccountId);
                }
                catch (Exception)
                {
                    return new JsonResponse(9995);
                }
                string contentFileName = await _contentRepository.GetFileNameAsync(partner.Id);
                countNew += await _messageRepository.CountNewAsync(countInt);

                Message message = await _messageRepository.GetLastAsync(con.Id);

                data.Add(new
                {
                    id = con.Id.ToString(),
                    partner = new
                    {
                        id = partner.Id.ToString(),
                        username = partner.Phone,
                        avatar = $"/content/{contentFileName}"
                    },
                    lastmessage = new
                    {
                        message = message.Body,
                        created = Formatter.PostedTime(message.DateCreated),
                        unread = message.IsRead ? "1" : "0",
                    }
                });
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data,
                numNewMessage = countNew.ToString(),
            };
        }


        // WebChat (Thai / C)
        [Route("get_messages")]
        [HttpPost]
        public async Task<object> GetMessages(
            [FromQuery] string token,
            [FromQuery] string partnerAccountId
            )
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Formatter.TryParseId(partnerAccountId, out int partnerId) != Formatter.Result.OK)
                return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            Conversation conversation;
            try
            {
                conversation = await _messageRepository.GetConversationAsync(accountId, partnerId);
            }
            catch (Exception)
            {
                return new JsonResponse(9994);
            }

            var messages = await _messageRepository.GetMessagesAsync(conversation.Id, 20);
            List<object> data = new();
            foreach (var message in messages)
            {
                data.Add(new
                {
                    id = message.Id.ToString(),
                    body = message.Body,
                    authorAccountId = message.AuthorAccountId.ToString()
                });
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data
            };
        }


        // WebChat (Thai / C)
        [Route("get_messages_from_last")]
        [HttpPost]
        public async Task<object> GetMessagesFromLast(
            [FromQuery] string token,
            [FromQuery] string partner_id,
            [FromQuery] string last_message_id
            )
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Formatter.TryParseId(partner_id, out int partnerId) != Formatter.Result.OK)
                return new JsonResponse(1004);
            if (Formatter.TryParseId(last_message_id, out int lastMessageId, true)  != Formatter.Result.OK)
                return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            Conversation? conversation = null;
            while (conversation == null)
            {
                try
                {
                    conversation = await _messageRepository.GetConversationAsync(accountId, partnerId);
                }
                catch (Exception)
                {
                }
            }

            var messages = await _messageRepository.GetMessagesFromAsync(conversation.Id, lastMessageId);
            List<object> data = new();
            foreach (var message in messages)
            {
                data.Add(new
                {
                    id = message.Id.ToString(),
                    body = message.Body,
                    authorAccountId = message.AuthorAccountId.ToString()
                });
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data
            };
        }


        // WebChat (Thai / C)
        [Route("set_message")]
        [HttpPost]
        public async Task<JsonResponse> SetMessage(
            [FromQuery] string token,
            [FromQuery] string partner_id,
            [FromQuery] string body)
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (string.IsNullOrWhiteSpace(body)) return new JsonResponse(1002);
            if (Formatter.TryParseId(partner_id, out int partnerId) != Formatter.Result.OK) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }
            try
            {
                _ = await _accountRepository.GetAsync(partnerId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            Conversation senderCon;
            try
            {
                senderCon = await _messageRepository.GetConversationAsync(accountId, partnerId);
            }
            catch (Exception)
            {
                senderCon = await _messageRepository.AddConversationAsync(accountId, partnerId);
            }

            Conversation receiverCon;
            try
            {
                receiverCon = await _messageRepository.GetConversationAsync(partnerId, accountId);
            }
            catch (Exception)
            {
                receiverCon = await _messageRepository.AddConversationAsync(partnerId, accountId);
            }

            await _messageRepository.AddAsync(body, accountId, senderCon.Id);
            await _messageRepository.AddAsync(body, accountId, receiverCon.Id);


            return new JsonResponse(1000);
        }


        // W4 (Thau / C)
        [Route("delete_message")]
        [HttpPost]
        public async Task<JsonResponse> DeleteMessage(
            [FromQuery] string token,
            [FromQuery] string message_id,
            [FromQuery] string conversation_id)
        {

            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Formatter.TryParseId(message_id, out int messageId) == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);
            if (Formatter.TryParseId(conversation_id, out int conversationId) == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);

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

            Conversation conversation;
            try
            {
                conversation = await _messageRepository.GetConversationAsync(conversationId);
                if (conversation.OwnerAccountId != accountId) return new JsonResponse(1009);
            }
            catch (Exception)
            {
                return new JsonResponse(1004);
            }

            Message message;
            try
            {
                message = await _messageRepository.GetAsync(messageId);
            }
            catch (Exception)
            {
                return new JsonResponse(1004);
            }

            _ = await _messageRepository.DeleteAsync(messageId);

            return new JsonResponse(1000);
        }


        // W4 (Thau - Thai / C)
        [Route("delete_conversation")]
        [HttpPost]
        public async Task<JsonResponse> DeleteConversation(
            [FromQuery] string token,
            [FromQuery] string? partner_Id = null,
            [FromQuery] string? conversation_Id = null
            )
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (string.IsNullOrWhiteSpace(partner_Id) == string.IsNullOrWhiteSpace(conversation_Id)) return new JsonResponse(1004);
            if (Formatter.TryParseId(partner_Id, out int partnerId) == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);
            if (Formatter.TryParseId(conversation_Id, out int conversationId) == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);

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

            Conversation conv;
            if (partnerId != 0)
            {
                try
                {
                    conv = await _messageRepository.GetConversationAsync(accountId, partnerId);
                }
                catch (Exception)
                {
                    return new JsonResponse(9994);
                }
            }
            else
            {
                try
                {
                    conv = await _messageRepository.GetConversationAsync(conversationId);
                }
                catch (Exception)
                {
                    return new JsonResponse(9995);
                }
            }

            if (conv.OwnerAccountId == accountId)
                _ = await _messageRepository.DeleteConversationAsync(conversationId);
            else
                return new JsonResponse(1009);

            return new JsonResponse(1000);
        }
    }
}
