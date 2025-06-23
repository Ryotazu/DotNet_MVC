using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.DataAccess.Repository.IRepository;
using Web.Models;
using Web.Models.ViewModels;
using Web.Utility;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
     
            return View(products);
        }

        public IActionResult Upsert(int? id) // Update Insert
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;

            ProductViewModel ProductVM = new()
            {
                CategoryList = CategoryList,
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                // create
                return View(ProductVM);
            } else
            {
                // update
                ProductVM.Product = _unitOfWork.Product.Get(a => a.Id == id);
                return View(ProductVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel productVM, IFormFile? file) 
        { 
            if(ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if(productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                } else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";

                return RedirectToAction("Index");
            } else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            }

                return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() 
        { 
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();

            return Json(new { data = objProductList });
        }

        public IActionResult Delete(int? id)
        {
            Product productToBeDeleted = _unitOfWork.Product.Get(a => a.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting"});
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successfull"});
        }
        #endregion
    }
}
