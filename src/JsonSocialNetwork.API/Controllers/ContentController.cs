using JsonSocialNetwork.API.Classes;
using JsonSocialNetwork.Domain.Entities;
using JsonSocialNetwork.Infrastructure.Repositories;
using JsonSocialNetwork.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JsonSocialNetwork.API.Controllers
{
    [Route("content")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly ContentRepository _contentRepository;
        public ContentController(ContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> Get(string fileName)
        {
            (byte[], Content) data;
            try
            {
                data = await _contentRepository.GetFileDataAsync(fileName);
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = System.IO.File.ReadAllText(@"..\..\Data\404.html")
                };
            }
            return File(data.Item1, data.Item2.ContentType);
        }
    }
}
