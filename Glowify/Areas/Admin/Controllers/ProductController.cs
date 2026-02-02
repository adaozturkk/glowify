using Glowify.Data;
using Glowify.Models;
using Glowify.Models.ViewModels;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Glowify.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new Product(),

                CategoryList = Enum.GetValues(typeof(CategoryType))
                    .Cast<CategoryType>()
                    .Select(v => new SelectListItem
                    {
                        Text = v.ToString(),
                        Value = ((int)v).ToString()
                    }),

                SkinTypeList = Enum.GetValues(typeof(SkinType))
                    .Cast<SkinType>()
                    .Select(v => new SelectListItem
                    {
                        Text = v.ToString(),
                        Value = ((int)v).ToString()
                    })
            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Products.FirstOrDefault(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (obj.Product.Id == 0)
                {
                    _db.Products.Add(obj.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _db.Products.Update(obj.Product);
                    TempData["success"] = "Product updated successfully";
                }

                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            obj.CategoryList = Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() });
            obj.SkinTypeList = Enum.GetValues(typeof(SkinType)).Cast<SkinType>().Select(v => new SelectListItem { Text = v.ToString(), Value = ((int)v).ToString() });

            return View(obj);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _db.Products.Select(u => new {
                id = u.Id,
                name = u.Name,
                listPrice = u.ListPrice,
                price = u.Price,
                stock = u.Stock,
                imageUrl = u.ImageUrl,
                category = u.Category.ToString(),
                skinType = u.TargetSkinType.ToString()
            }).ToList();

            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Products.FirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            if (obj.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _db.Products.Remove(obj);
            _db.SaveChanges();
            return Json(new { success = false, message = "Delete Successfull" });
        }

        #endregion
    }
}
