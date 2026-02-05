using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using trial.DAL;
using trial.Models;
using System.Collections.Generic;

namespace trial.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductDAL _dal;

        public ProductController(IConfiguration configuration)
        {
            _dal = new ProductDAL(configuration);
        }

        // List all products
        public IActionResult Index()
        {
            var products = _dal.GetAllProducts();
            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            return View(products);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _dal.GetCategories();
            return View();
        }

        // POST: Create
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _dal.GetCategories();
                return View(product);
            }

            string message;
            bool success = _dal.AddProduct(product, out message);
            TempData[success ? "Success" : "Error"] = message;
            return success ? RedirectToAction("Index") : View(product);
        }

        // GET: Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _dal.GetAllProducts().Find(p => p.ProductId == id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = _dal.GetCategories();
            return View(product);
        }

        // POST: Edit
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _dal.GetCategories();
                return View(product);
            }

            string message;
            bool success = _dal.UpdateProduct(product, out message);
            TempData[success ? "Success" : "Error"] = message;
            return success ? RedirectToAction("Index") : View(product);
        }

        // POST: Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string message;
            bool success = _dal.DeleteProduct(id, out message);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index");
        }
    }
}
