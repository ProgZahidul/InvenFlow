using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Controllers
{
    // Controllers/AdminController.cs
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Admin/Users
        // Controllers/AdminController.cs
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .Include(u => u.Department)
                .ToListAsync();

            var userRoles = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = roles?.ToList() ?? new List<string>() // Handle null roles
                });
            }

            return View(userRoles);
        }

        // GET: Admin/EditUser/{id}
        // Controllers/AdminController.cs
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = InventoryManagementSystem.Constants.ApplicationRoles.Ordered.ToList();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DepartmentId = user.DepartmentId,
                SelectedRoles = userRoles.ToList(),
                AllRoles = allRoles // Ensure this is never null
            };

            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.DepartmentId = model.DepartmentId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    var selectedRole = (model.SelectedRoles ?? new List<string>()).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(selectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, selectedRole);
                    }

                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Repopulate AllRoles if model is invalid
            if (model.AllRoles == null || !model.AllRoles.Any())
            {
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            }

            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name");
            return View(model);
        }

        //// GET: Admin/Items
        //public async Task<IActionResult> Items()
        //{
        //    var items = await _context.Items
        //        .Include(i => i.Category)
        //        .Include(i => i.UnitOfMeasure)
        //        .Where(i => i.IsActive)
        //        .ToListAsync();

        //    return View(items);
        //}

        // GET: Admin/Items?status=active|inactive|all
        public async Task<IActionResult> Items(string status = "active")
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.UnitOfMeasure)
                .AsQueryable();

            if (status == "active")
            {
                query = query.Where(i => i.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(i => !i.IsActive);
            }
            // "all" => no filter

            var items = await query.ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(items);
        }



        // GET: Admin/CreateItem
        public IActionResult CreateItem()
        {
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name");
            ViewBag.UnitOfMeasures = new SelectList(_context.UnitOfMeasures, "UnitId", "UnitName");
            return View();
        }

        // POST: Admin/CreateItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(ItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var item = new Item
                {
                    Name = model.Name,
                    Description = model.Description,
                    Code = model.Code,
                    UnitOfMeasureId = model.UnitOfMeasureId,
                    UnitPrice = model.UnitPrice,
                    ReorderLevel = model.ReorderLevel,
                    CategoryId = model.CategoryId,
                    IsActive = model.IsActive
                };

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Items));
            }

            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", model.CategoryId);
            ViewBag.UnitOfMeasures = new SelectList(_context.UnitOfMeasures, "UnitId", "UnitName", model.UnitOfMeasureId);
            return View(model);
        }

        // GET: Admin/EditItem/{id}
        public async Task<IActionResult> EditItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var model = new ItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Code = item.Code,
                UnitOfMeasureId = item.UnitOfMeasureId,
                UnitPrice = item.UnitPrice,
                ReorderLevel = item.ReorderLevel,
                CategoryId = item.CategoryId,
                IsActive = item.IsActive
            };

            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", model.CategoryId);
            ViewBag.UnitOfMeasures = new SelectList(_context.UnitOfMeasures, "UnitId", "UnitName", model.UnitOfMeasureId);

            return View(model);
        }

        // POST: Admin/EditItem/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(int id, ItemViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var item = await _context.Items.FindAsync(id);
                    if (item == null)
                    {
                        return NotFound();
                    }

                    item.Name = model.Name;
                    item.Description = model.Description;
                    item.Code = model.Code;
                    item.UnitOfMeasureId = model.UnitOfMeasureId;
                    item.UnitPrice = model.UnitPrice;
                    item.ReorderLevel = model.ReorderLevel;
                    item.CategoryId = model.CategoryId;
                    item.IsActive = model.IsActive;

                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Items));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name", model.CategoryId);
            ViewBag.UnitOfMeasures = new SelectList(_context.UnitOfMeasures, "UnitId", "UnitName", model.UnitOfMeasureId);

            return View(model);
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
        // POST: Admin/ReactivateItem/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.IsActive = true;
            _context.Update(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item has been reactivated successfully.";
            return RedirectToAction(nameof(Items));
        }

        // GET: Admin/DeleteItem/{id}
        public async Task<IActionResult> DeleteItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.UnitOfMeasure)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Admin/DeleteItem/{id}
        [HttpPost, ActionName("DeleteItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItemConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                item.IsActive = false; // Soft delete
                _context.Update(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Item has been deactivated successfully.";
            }
            return RedirectToAction(nameof(Items));
        }

        // GET: Admin/InactiveItems
        public async Task<IActionResult> InactiveItems()
        {
            var inactiveItems = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.UnitOfMeasure)
                .Where(i => !i.IsActive)
                .ToListAsync();

            return View(inactiveItems);
        }
    }
}

   

