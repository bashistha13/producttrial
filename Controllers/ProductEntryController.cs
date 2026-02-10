using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using trial.DAL;

namespace trial.Controllers
{
    public class ProductEntryController : Controller
    {
        private readonly PurchaseEntryDAL _dal;

        public ProductEntryController(IConfiguration configuration)
        {
            _dal = new PurchaseEntryDAL(configuration);
        }

        public IActionResult Index()
        {
            var entries = _dal.GetAllEntries();
            return View(entries);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            string message;
            bool success = _dal.DeleteEntry(id, out message);
            
            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("Index");
        }
    }
}