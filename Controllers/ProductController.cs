using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization; // Needed for Authorize
using System.IO;
using System.Threading.Tasks;
using System;
using trial.DAL;
using trial.Models;

namespace trial.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductDAL _dal;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            _dal = new ProductDAL(configuration);
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            ViewBag.Categories = _dal.GetCategories();
            var products = _dal.GetAllProducts();
            return View(products);
        }

        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _dal.GetAllProducts().Find(p => p.ProductId == id);
            return Json(product);
        }

        // NEW: Only Admins can Add/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AddOrEdit(Product product, IFormFile file)
        {
            string message = "";
            
            if (file != null && file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images/products");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                
                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                product.ImagePath = "/images/products/" + fileName;
            }

            bool success = _dal.UpsertProduct(product, out message);
            return Json(new { success = success, message = message });
        }
        
        // NEW: Only Admins can Delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            string message;
            bool success = _dal.DeleteProduct(id, out message);
            
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }
    }
}