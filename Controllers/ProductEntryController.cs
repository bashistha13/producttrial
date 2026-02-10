using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using trial.DAL;
using trial.Models;
using System.Collections.Generic;
using System.Linq; // Needed for Select()

namespace trial.Controllers
{
    public class ProductEntryController : Controller
    {
        private readonly PurchaseEntryDAL _dal;
        private readonly ProductDAL _productDAL;

        public ProductEntryController(IConfiguration configuration)
        {
            _dal = new PurchaseEntryDAL(configuration);
            _productDAL = new ProductDAL(configuration);
        }

        public IActionResult Index()
        {
            var entries = _dal.GetAllEntries();
            return View(entries);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var products = _productDAL.GetAllProducts();
            
            // Pass the full product list to ViewBag so we can access Prices in the View
            ViewBag.FullProductList = products;

            // Simple list for the dropdown
            ViewBag.ProductList = new SelectList(products, "ProductId", "ProductName");
            
            return View(new PurchaseEntry());
        }

        [HttpPost]
        public IActionResult Create(PurchaseEntry entry)
        {
            string message;
            bool success = _dal.InsertEntry(entry, out message);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = message;
                var products = _productDAL.GetAllProducts();
                ViewBag.FullProductList = products;
                ViewBag.ProductList = new SelectList(products, "ProductId", "ProductName");
                return View(entry);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            string message;
            bool success = _dal.DeleteEntry(id, out message);
            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetDetails(int id)
        {
            var entry = _dal.GetEntryById(id);
            if (entry == null) return NotFound();
            return Json(entry);
        }
    }
}