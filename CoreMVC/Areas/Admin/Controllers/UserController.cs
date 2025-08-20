using CoreMVC.DataAccess.Repository.IRepository;
using CoreMVC.Models.ViewModels;
using CoreMVC.Models;
using CoreMVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RoleManagment(string userId)
        {
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company");
            if (user == null) return NotFound();

            RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
                ApplicationUser = user,
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            var roles = await _userManager.GetRolesAsync(user);
            RoleVM.ApplicationUser.Role = roles.FirstOrDefault()??string.Empty;

            return View(RoleVM);
        }


        [HttpPost]
        public async Task<IActionResult> RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            if (roleManagmentVM.ApplicationUser == null || string.IsNullOrEmpty(roleManagmentVM.ApplicationUser.Id))
            {
                return BadRequest("Invalid user data.");
            }

            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            // get current role  
            var roles = await _userManager.GetRolesAsync(applicationUser);
            string oldRole = roles.FirstOrDefault() ?? string.Empty;

            if (roleManagmentVM.ApplicationUser?.Role != oldRole)
            {
                // role updated  
                if (roleManagmentVM.ApplicationUser?.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }

                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                if (!string.IsNullOrEmpty(oldRole))
                {
                    await _userManager.RemoveFromRoleAsync(applicationUser, oldRole);
                }
                await _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser?.Role ?? string.Empty);
            }
            else
            {
                // role is same, but company might have changed  
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }



        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var objUserList = _unitOfWork.ApplicationUser
                .GetAll(includeProperties: "Company")
                .ToList();

            foreach (var user in objUserList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault() ?? string.Empty;

                // Ensure company is not null
                user.Company ??= new Company { Name = "" };
            }

            return Json(new { data = objUserList });
        }



        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {

            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
