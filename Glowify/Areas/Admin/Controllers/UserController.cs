using Glowify.Data;
using Glowify.Data.Repository.IRepository;
using Glowify.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Glowify.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RoleManagement(string userId)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId)?.RoleId;
            var roleName = _db.Roles.FirstOrDefault(u => u.Id == roleId)?.Name;

            user.Role = roleName;

            ViewBag.RoleList = _db.Roles.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name
            });

            return View(user);
        }

        [HttpPost]
        public IActionResult RoleManagement(Glowify.Models.ApplicationUser user)
        {
            var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == user.Id)?.RoleId;
            var oldRole = _db.Roles.FirstOrDefault(u => u.Id == roleId)?.Name;

            if (user.Role != oldRole)
            {
                var oldUserRole = _db.UserRoles.FirstOrDefault(u => u.UserId == user.Id);
                if (oldUserRole != null)
                {
                    _db.UserRoles.Remove(oldUserRole);
                    _db.SaveChanges();
                }

                var newRoleId = _db.Roles.FirstOrDefault(u => u.Name == user.Role)?.Id;
                if (newRoleId != null)
                {
                    _db.UserRoles.Add(new IdentityUserRole<string>
                    {
                        RoleId = newRoleId,
                        UserId = user.Id
                    });
                    _db.SaveChanges();
                }
            }

            TempData["success"] = "User role updated successfully!";
            return RedirectToAction("Index");
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _unitOfWork.ApplicationUser.GetAll().ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id)?.RoleId;

                if (roleId != null)
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == roleId)?.Name;
                }
                else
                {
                    user.Role = "No Role";
                }
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == user.Id)?.RoleId;
            var role = _db.Roles.FirstOrDefault(u => u.Id == roleId)?.Name;

            if (role == SD.Role_Admin)
            {
                return Json(new { success = false, message = "You can't lock admin account" });
            }

            if (user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Lock/Unlock successfull" });
        }

        #endregion
    }
}
