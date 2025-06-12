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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> Companys = _unitOfWork.Company.GetAll().ToList();
     
            return View(Companys);
        }

        public IActionResult Upsert(int? id) // Update Insert
        {
            if(id == null || id == 0)
            {
                // create
                return View(new Company());
            } else
            {
                // update
                Company companyObj = _unitOfWork.Company.Get(a => a.Id == id);
                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj) 
        { 
            if(ModelState.IsValid)
            {
                if(companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);
                } else
                {
                    _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company Created Successfully";

                return RedirectToAction("Index");
            } else
            {
                return View(companyObj);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() 
        { 
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = objCompanyList });
        }

        public IActionResult Delete(int? id)
        {
            Company CompanyToBeDeleted = _unitOfWork.Company.Get(a => a.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting"});
            }
            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successfull"});
        }
        #endregion
    }
}
