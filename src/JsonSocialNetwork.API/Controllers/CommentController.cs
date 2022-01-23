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
    public class CommentController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly CommentRepository _commentRepository;
        private readonly PostRepository _postRepository;
        private readonly AccountRepository _accountRepository;
        private readonly ContentRepository _contentRepository;

        public CommentController(
            AuthenticationService authenticationService,
            CommentRepository commentRepository,
            PostRepository postRepository,
            AccountRepository accountRepository,
            ContentRepository contentRepository
            )
        {
            _authenticationService = authenticationService;
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _accountRepository = accountRepository;
            _contentRepository = contentRepository;
        }


        // W3 (Tuan - Thau / C)
        [Route("set_comment")]
        [HttpPost]
        public async Task<object> SetComment(
            [FromQuery] string token,
            [FromQuery] string id,     // id post
            [FromQuery] string? comment = null,
            [FromQuery] string index = "0",
            [FromQuery] string count = "20")
        {
            if (Validation.IsEmpty(token, id, index, count)) return new JsonResponse(1002);
            if (string.IsNullOrWhiteSpace(comment)) return new JsonResponse(1002);
            if (!Validation.IsID(id)) return new JsonResponse(1004);

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

            Post post;
            int postId = int.Parse(id);
            try
            {
                post = await _postRepository.GetAsync(postId);
            }
            catch (Exception)
            {
                return new JsonResponse(9992);
            }

            string avatarFileName = await _contentRepository.GetFileNameAsync(account.Id);

            bool isBlocked = true;
            try
            {
                _ = await _accountRepository.GetBlockAsync(post.AuthorAccountId, accountId);
            }
            catch (Exception)
            {
                isBlocked = false;
            }
           
                _ = await _commentRepository.AddAsync(comment, accountId, postId);

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    id = post.Id.ToString(),
                    comment = comment,
                    created = DateTime.Now.ToString(),
                    poster = new
                    {
                        id = account.Id.ToString(),
                        name = account.Name,
                        avatar = $"/content/{avatarFileName}"
                    }
                },
                is_blocked = isBlocked ? "1" : "0"
            };
        }


        // W3 (Linh / C)
        [Route("get_comment")]
        [HttpPost]
        public async Task<object> GetComment(
            [FromQuery] string id,
            [FromQuery] string index,
            [FromQuery] string count,
            [FromQuery] string token
            )
        {
            if (Validation.IsEmpty(id, index, count, token)) return new JsonResponse(1002);
            if (!Validation.IsID(id, count)) return new JsonResponse(1004);
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

            int postId = int.Parse(id);
            Post post;
            try
            {
                post = await _postRepository.GetAsync(postId);
            }
            catch (Exception)
            {
                return new JsonResponse(9992);
            }

            List<Comment> commentList = new();
            commentList.AddRange(await _commentRepository.GetAllAsync(postId));

            int countComment = commentList.Count;
            if (countComment == 0)
            {
                return new JsonResponse(9994);
            }

            int start = int.Parse(index);
            if (start >= countComment)
            {
                return new JsonResponse(1004);
            }

            int number = int.Parse(count);
            if (number >= countComment)
            {
                number = countComment - start;
            }

            List<Comment> comments = commentList.GetRange(start, number);
            List<object> data = new();

            foreach (Comment comment in comments)
            {
                string contentFileName = await _contentRepository.GetFileNameAsync(comment.AuthorAccountId);
                Account account = await _accountRepository.GetAsync(comment.AuthorAccountId);
                data.Add(new
                {
                    id = comment.Id.ToString(),
                    comment = comment.Body,
                    created = comment.DateCreated,
                    poster = new
                    {
                        id = comment.AuthorAccountId.ToString(),
                        name = account.Name,
                        avatar = $"/content/{contentFileName}"
                    }
                }); ;
            }

            bool isBlocked = true;
            try
            {
                await _accountRepository.GetBlockAsync(post.AuthorAccountId, accountId);
            }
            catch (Exception)
            {
                isBlocked = false;
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data,
                is_blocked = isBlocked ? "1" : "0"
            };
        }


        // W3 (Linh / C)
        [Route("edit_comment")]
        [HttpPost]
        public async Task<JsonResponse> EditComment(
            [FromQuery] string token,
            [FromQuery] string id,
            [FromQuery] string id_com,
            [FromQuery] string comment)
        {
            if (Validation.IsEmpty(token, id, id_com, comment)) return new JsonResponse(1002);
            if (!Validation.IsID(id, id_com)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            int postId = int.Parse(id);
            try
            {
                _ = await _postRepository.GetAsync(postId);
            }
            catch (Exception)
            {
                return new JsonResponse(9992);
            }

            Comment com;
            int comId = int.Parse(id_com);
            try
            {
                com = await _commentRepository.GetAsync(comId);
            }
            catch (Exception)
            {
                return new JsonResponse(1004);
            }

            if (com.AuthorAccountId != accountId || com.OwnerPostId != postId) return new JsonResponse(1009);

            _ = await _commentRepository.UpdateAsync(comId, comment);

            return new JsonResponse(1000);
        }


        // W3 (Linh / C)
        [Route("del_comment")]
        [HttpPost]
        public async Task<JsonResponse> DelComment(
            [FromQuery] string token,
            [FromQuery] string id,
            [FromQuery] string id_com)
        {
            if (Validation.IsEmpty(token, id, id_com)) return new JsonResponse(1002);
            if (!Validation.IsID(id, id_com)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            }
            catch (Exception)
            {
                return new JsonResponse(9998);
            }

            int postId = int.Parse(id);
            Post post;
            try
            {
                post = await _postRepository.GetAsync(postId);
            }
            catch (Exception)
            {
                return new JsonResponse(9992);
            }

            int comId = int.Parse(id_com);
            Comment comment;
            try
            {
                comment = await _commentRepository.GetAsync(comId);
            }
            catch (Exception)
            {
                return new JsonResponse(1004);
            }

            if (comment.OwnerPostId != postId) return new JsonResponse(1009);

            if (comment.AuthorAccountId != accountId && post.AuthorAccountId != accountId) return new JsonResponse(1009);

            _ = await _commentRepository.DeleteAsync(comId);

            return new JsonResponse(1000);
        }

    }
}
