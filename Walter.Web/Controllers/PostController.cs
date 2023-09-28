using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Walter.Core.DTO_s.Category;
using Walter.Core.DTO_s.Post;
using Walter.Core.Interfaces;
using Walter.Core.Validation.Post;
using X.PagedList;

namespace Walter.Web.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        private IWebHostEnvironment _webHostEnvironment;

        public PostController(ICategoryService categoryService, IPostService postService, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _postService = postService; 
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexAsync(int? page)
        {
            List<PostDto> posts = await _postService.GetAll();
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(posts.ToPagedList(pageNumber, pageSize));
        }


        


        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostDto model)
        {
            var validator = new CreateValidation();
            var validatinResult = await validator.ValidateAsync(model);
            if (validatinResult.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                model.File = files;
                await _postService.Create(model);
                return RedirectToAction("Index", "Post");
            }
            ViewBag.AuthError = validatinResult.Errors[0];
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var posts = await _postService.Get(id);

            if (posts == null) return NotFound();

            await LoadCategories();
            return View(posts);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostDto model)
        {
            var validator = new CreateValidation();
            var validationResult = await validator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                model.File = files;
                await _postService.Update(model);
                return RedirectToAction("Index", "Post");
            }
            ViewBag.CreatePostError = validationResult.Errors[0];
            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var postDto = await _postService.Get(id);

            if (postDto == null)
            {
                ViewBag.AuthError = "Post not found.";
                return View();
            }
            return View(postDto);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteById(int id)
        {
            await _postService.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PostsByCategory(int id)
        {
            List<PostDto> posts = await _postService.GetByCategory(id);
            int pageSize = 20;
            int pageNumber = 1;
            return View("Index", posts.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromForm] string searchString)
        {
            List<PostDto> posts = await _postService.Search(searchString);
            int pageSize = 20;
            int pageNumber = 1;
            return View("Index", posts.ToPagedList(pageNumber, pageSize));
        }


        private async Task LoadCategories()
        {
            ViewBag.CategoryList = new SelectList(
                await _categoryService.GettAll(),
                nameof(CategoryDto.Id),
                nameof(CategoryDto.Name)
                );
            ;
        }
        public async Task<IActionResult> FullNews(int id)
        {
            var posts = await _postService.Get(id);


            return View(posts);
        }



        [HttpPost]
        public IActionResult Save()
        {
            string base64 = Request.Form["crop-image"];
            byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);

            string filePath = Path.Combine(this._webHostEnvironment.WebRootPath, "Images", "Cropped.png");
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            return RedirectToAction("Index");
        }
    }
    
}
