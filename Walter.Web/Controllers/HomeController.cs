using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Walter.Core.DTO_s.Post;
using Walter.Core.Interfaces;
using Walter.Web.Models;
using X.PagedList;

namespace Walter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;

        public HomeController(ILogger<HomeController> logger, IPostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        public async Task<IActionResult> Index(int? page)
        {
            List<PostDto> posts = await _postService.GetAll();
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View( posts.ToPagedList(pageNumber, pageSize));
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            switch (statusCode)
            {
                case 404: return View("NotFound");
                    break;
                default:
                    return View("Error");
            }
        }
        public async Task<IActionResult> PostsByCategory(int id)
        {
            List<PostDto> posts = await _postService.GetByCategory(id);
            int pageSize = 20;
            int pageNumber = 1;
            return View("Index", posts.ToPagedList(pageNumber, pageSize));
        }

    }
}