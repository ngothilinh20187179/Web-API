using JsonSocialNetwork.API.Classes;
using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Repositories;
using JsonSocialNetwork.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JsonSocialNetwork.API.Controllers
{
    [Route("")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly AccountRepository _accountRepository;
        private readonly ContentRepository _contentRepository;
        private readonly PostRepository _postRepository;

        public PostController(
            AuthenticationService authenticationService,
            AccountRepository accountRepository,
            ContentRepository contentRepository,
            PostRepository postRepository)
        {
            _authenticationService = authenticationService;
            _accountRepository = accountRepository;
            _contentRepository = contentRepository;
            _postRepository = postRepository;
        }

        // W2 (Tuan - Thai / C)
        [Route("get_post")]
        [HttpPost]
        public async Task<object> GetPost(
            [FromQuery] string token,
            [FromQuery] string id)
        {
            if (Validation.IsEmpty(token, id)) return new JsonResponse(1002);
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

            string likes = (await _postRepository.CountLikeAsync(postId)).ToString();
            string isLiked = (await _postRepository.CountLikeAsync(postId, accountId)).ToString();

            List<object> media = new();
            try
            {
                PostContent[] postContents = (await _postRepository.GetAllPostContentAsync(postId)).ToArray();
                Content content = await _contentRepository.GetAsync(postContents[0].ContentFileName);
                switch (content.ContentType.Split('/')[0])
                {
                    case "image":
                        {
                            foreach (var item in postContents)
                            {
                                media.Add(new
                                {
                                    image = new
                                    {
                                        id = item.OrderId.ToString(),
                                        url = $"/content/{item.ContentFileName}"
                                    }
                                });
                            }
                            break;
                        }
                    case "video":
                        {
                            media.Add(new
                            {
                                video = new
                                {
                                    url = $"/content/{postContents[0].ContentFileName}"
                                }
                            });
                            break;
                        }
                }
            } catch (Exception)
            {
            }

            string avatarFileName = await _contentRepository.GetFileNameAsync(account.Id);

            bool isBlocked = true;
            try
            {
                await _accountRepository.GetBlockAsync(post.AuthorAccountId, accountId);
            } catch (Exception)
            {
                isBlocked = false;
            }

            int reports = await _postRepository.GetReportCountAsync(post.Id);

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    id = post.Id.ToString(),
                    described = post.Body,
                    created = Formatter.PostedTime(post.DateCreated),
                    modified = Formatter.PostedTime(post.DateModified),
                    like = likes,
                    is_liked = isLiked,
                    media,
                    author = new
                    {
                        id = post.AuthorAccountId.ToString(),
                        name = account.Name,
                        avatar = $"/content/{avatarFileName}",
                    },
                    is_blocked = isBlocked ? "1" : "0",
                    can_edit = (post.AuthorAccountId == accountId) ? "1" : "0",
                    banned = (reports > 0) ? "1" : "0",
                    can_comment = isBlocked ? "0" : "1"
                }
            };
        }


        // W2 (Tuan - Thai / C)
        [Route("get_list_posts")]
        [HttpPost]
        public async Task<object> GetListPosts(
            [FromQuery] string token,
            [FromQuery] string index,
            [FromQuery] string count)
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Formatter.TryParseId(index, out int indexInt) != Formatter.Result.OK)
                return new JsonResponse(1004);
            if (Formatter.TryParseId(count, out int countInt) != Formatter.Result.OK)
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

            Account account;
            try
            {
                account = await _accountRepository.GetAsync(accountId);
            }
            catch (Exception)
            {
                return new JsonResponse(9995);
            }

            List<object> posts = new List<object>();
            foreach(Post post in (await _postRepository.GetAllAsync(indexInt, countInt)).ToList())
            {
                string likes = (await _postRepository.CountLikeAsync(post.Id)).ToString();
                string isLiked = (await _postRepository.CountLikeAsync(post.Id, accountId)).ToString();

                List<object> media = new();
                try
                {
                    PostContent[] postContents = (await _postRepository.GetAllPostContentAsync(post.Id)).ToArray();
                    Content content = await _contentRepository.GetAsync(postContents[0].ContentFileName);
                    switch (content.ContentType.Split('/')[0])
                    {
                        case "image":
                            {
                                foreach (var item in postContents)
                                {
                                    media.Add(new
                                    {
                                        image = new
                                        {
                                            id = item.OrderId.ToString(),
                                            url = $"/content/{item.ContentFileName}"
                                        }
                                    });
                                }
                                break;
                            }
                        case "video":
                            {
                                media.Add(new
                                {
                                    video = new
                                    {
                                        url = $"/content/{postContents[0].ContentFileName}"
                                    }
                                });
                                break;
                            }
                    }
                }
                catch (Exception)
                {
                }

                string avatarFileName = await _contentRepository.GetFileNameAsync(account.Id);

                bool isBlocked = true;
                try
                {
                    await _accountRepository.GetBlockAsync(post.AuthorAccountId, accountId);
                }
                catch (Exception)
                {
                    isBlocked = false;
                }

                int reports = await _postRepository.GetReportCountAsync(post.Id);

                posts.Add(new
                {
                    id = post.Id.ToString(),
                    name = await _postRepository.GetInteractorsAsync(post.Id),
                    media,
                    described = post.Body,
                    created = Formatter.PostedTime(post.DateCreated),
                    like = likes,
                    is_liked = isLiked,
                    is_blocked = isBlocked ? "1" : "0",
                    can_comment = isBlocked ? "0" : "1",
                    can_edit = (post.AuthorAccountId == accountId) ? "1" : "0",
                    banned = (reports > 0) ? "1" : "0",
                    author = new
                    {
                        id = post.AuthorAccountId.ToString(),
                        name = account.Name,
                        avatar = $"/content/{avatarFileName}",
                    }
                });
            }


            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    posts,
                    new_items = await _postRepository.CountNewAsync(indexInt, countInt),
                    last_id = (await _postRepository.GetLastAsync()).Id.ToString(),
                }
            };
        }


        // W2 (Tuan - Thai / C)
        [Route("add_post")]
        [HttpPost]
        public async Task<object> AddPost(
            [FromQuery] string token,
            [FromForm] List<IFormFile> image,
            [FromQuery] string? described = null,
            [FromForm] IFormFile? video = null)
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (string.IsNullOrWhiteSpace(described)) return new JsonResponse(1002);

            IFormFile[] media = Array.Empty<IFormFile>();
            if (video != null)
            {
                if (image.Count > 0 || !Validation.IsVideo(video)) return new JsonResponse(1003);
                if (Validation.IsVideoOversize(video)) return new JsonResponse(1006);
                media = new IFormFile[1] {video};
            }
            else if (image.Count > 0)
            {
                if (image.Count > 4) return new JsonResponse(1008);
                if (!Validation.IsImage(image.ToArray())) return new JsonResponse(1003);
                if (Validation.IsImageOversize(image.ToArray())) return new JsonResponse(1006);
                media = image.ToArray();
            }
            

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

            int postId = await _postRepository.AddAsync(described, accountId);

            try
            {
                int order = 0;
                foreach (IFormFile file in media)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    string fileName = await _contentRepository.AddAsync(ms.ToArray(), file.ContentType);
                    _ = await _postRepository.AddPostContentAsync(fileName, postId, order++);
                }
            }
            catch (Exception)
            {
                return new JsonResponse(1007);
            }


            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    id = postId,
                    url = $"/get_post?id={postId}&token={token}"
                }
            };
        }


        // W2 (Tuan - Thai / RC)
        [Route("edit_post")]
        [HttpPost]
        public async Task<JsonResponse> EditPost(
            [FromQuery] string token,
            [FromQuery] string id,
            [FromForm] List<IFormFile> image,
            [FromQuery] string? described = null,
            [FromQuery] string? image_del = null,
            [FromQuery] string? image_sort = null,
            [FromForm] IFormFile? video = null)
        {
            if (string.IsNullOrWhiteSpace(token)) return new JsonResponse(1002);
            if (Formatter.TryParseId(id, out int postId) != Formatter.Result.OK) return new JsonResponse(1004);
            bool hasVideo = video != null;
            bool hasImage = image.Count > 0;
            if (hasVideo && hasImage) return new JsonResponse(1004);

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
            try
            {
                post = await _postRepository.GetAsync(postId);
                if (post.AuthorAccountId != accountId) return new JsonResponse(1009);
            }
            catch (Exception)
            {
                return new JsonResponse(9992);
            }

            Formatter.Result delIdFormat = Formatter.TryParseIds(image_del, out var delIds, true);
            if (delIdFormat == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);
            if (delIdFormat == Formatter.Result.OK)
            {
                foreach (int delId in delIds)
                {
                    await _postRepository.DeletePostContentAsync(postId, delId);
                }
            }

            List<PostContent> postContents;
            try
            {
                postContents = (await _postRepository.GetAllPostContentAsync(postId)).ToList();
                string contentType = (await _contentRepository.GetAsync(postContents[0].ContentFileName)).ContentType;
                if (contentType.Split('/')[0] == "image" && hasVideo)
                    return new JsonResponse(1003);
                else if (contentType.Split('/')[0] == "video" && (hasImage || hasVideo))
                    return new JsonResponse(1003);
            }
            catch (Exception)
            {
            }

            Formatter.Result sortIdFormat = Formatter.TryParseId(image_sort, out var sortId, true);
            if (sortIdFormat == Formatter.Result.WRONG_FORMAT) return new JsonResponse(1004);

            IFormFile[] media = Array.Empty<IFormFile>();
            if (sortIdFormat == Formatter.Result.OK)
            {
                if (!hasImage) return new JsonResponse(1004);
                if (hasVideo) return new JsonResponse(1004);

                if (image.Count > 4) return new JsonResponse(1008);
                IFormFile[] formFiles = image.ToArray();
                if (!Validation.IsImage(formFiles)) return new JsonResponse(1003);
                if (Validation.IsImageOversize(formFiles)) return new JsonResponse(1006);
                media = formFiles;
                if (sortId + image.Count > 4) return new JsonResponse(1004);

                int count = await _postRepository.CountPostContentAsync(postId, sortId, sortId + image.Count - 1);
                if (count > 0) return new JsonResponse(1004);
            }
            else
            {
                if (hasImage) return new JsonResponse(1004);
                if (video != null)
                {
                    if (!Validation.IsVideo(video)) return new JsonResponse(1003);
                    if (Validation.IsVideoOversize(video)) return new JsonResponse(1006);
                    media = new IFormFile[1] { video };
                }
            }


            try
            {
                foreach (IFormFile file in media)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    string fileName = await _contentRepository.AddAsync(ms.ToArray(), file.ContentType);
                    _ = await _postRepository.AddPostContentAsync(fileName, postId, sortId++);
                }
            }
            catch (Exception)
            {
                return new JsonResponse(1007);
            }

            if (!string.IsNullOrWhiteSpace(described)) await _postRepository.UpdateAsync(postId, described);

            return new JsonResponse(1000);
        }


        // W2 (Linh / C)
        [Route("delete_post")]
        [HttpPost]
        public async Task<JsonResponse> DeletePost(
            [FromQuery] string token,
            [FromQuery] string id)
        {
            if (Validation.IsEmpty(token, id)) return new JsonResponse(1002);
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

            if (post.AuthorAccountId != accountId) return new JsonResponse(1004);

            _ = await _postRepository.DeleteAsync(postId);

            return new JsonResponse(1000);
        }


        // W3 (Linh / C)
        [Route("like")]
        [HttpPost]
        public async Task<object> Like(
            [FromQuery] string token,
            [FromQuery] string id)
        {
            if (Validation.IsEmpty(token, id)) return new JsonResponse(1002);
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

            int postId = int.Parse(id);
            try
            {
                _ = await _postRepository.GetAsync(postId);
            }
            catch (Exception)
            {
                return new JsonResponse(9992);
            }

            int currentLike = await _postRepository.CountLikeAsync(postId);

            int isLiked = await _postRepository.CountLikeAsync(postId, accountId);

            if (isLiked == 1)
            {
                if (currentLike < 0 || currentLike >= 1000000000)
                {
                    return new
                    {
                        code = JsonResponse.GetCode(1000),
                        message = JsonResponse.GetMessage(1000),
                        data = new
                        {
                            like = "Không có ai like"
                        }
                    };
                }
                else
                {
                    _ = await _postRepository.DeleteLikeAsync(accountId, postId);
                    currentLike--;
                }
            }
            else
            {
                if (currentLike < 0 || currentLike >= 1000000000)
                {
                    return new
                    {
                        code = JsonResponse.GetCode(1000),
                        message = JsonResponse.GetMessage(1000),
                        data = new
                        {
                            like = "Bạn thích bài viết"
                        }
                    };
                }
                else
                {
                    _ = await _postRepository.AddLikeAsync(accountId, postId);
                    currentLike++;
                }
            }

            return new
            {
                code = JsonResponse.GetCode(1000),
                message = JsonResponse.GetMessage(1000),
                data = new
                {
                    like = currentLike.ToString()
                }
            };
        }


        // W3 (Tuan - Thai / C)
        [Route("report")]
        [HttpPost]
        public async Task<object> Report(
            [FromQuery] string token,
            [FromQuery] string id,
            [FromQuery] string subject,
            [FromQuery] string? details = null)
        {
            if (Validation.IsEmpty(token, id, subject)) return new JsonResponse(1002);
            if (!Validation.IsID(id, subject)) return new JsonResponse(1004);

            int accountId;
            try
            {
                accountId = _authenticationService.GetIdByToken(token);
            } catch (Exception)
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

            _ = await _postRepository.AddReportAsync(subject, details, postId);
            return new JsonResponse(1000);
        }
    }
}
