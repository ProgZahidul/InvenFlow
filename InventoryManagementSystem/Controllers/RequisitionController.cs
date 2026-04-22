using InventoryManagementSystem.Constants;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class RequisitionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequisitionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = ApplicationRoles.DepartmentUser + "," + ApplicationRoles.Administrator + "," + ApplicationRoles.InventoryManager + "," + ApplicationRoles.Storekeeper + "," + ApplicationRoles.Approver)]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            IQueryable<Requisition> requisitions = _context.Requisitions
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy);

            if (User.IsInRole(ApplicationRoles.DepartmentUser))
            {
                requisitions = requisitions.Where(r => r.RequestedById == user!.Id);
            }
            else if (User.IsInRole(ApplicationRoles.Approver))
            {
                requisitions = requisitions.Where(r => r.Status == RequisitionStatus.Pending);
            }

            var result = await requisitions
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();

            return View(result);
        }

        [Authorize(Roles = ApplicationRoles.DepartmentUser + "," + ApplicationRoles.Administrator + "," + ApplicationRoles.InventoryManager + "," + ApplicationRoles.Storekeeper + "," + ApplicationRoles.Approver)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requisition = await _context.Requisitions
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Item)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole(ApplicationRoles.DepartmentUser) && requisition.RequestedById != user!.Id)
            {
                return Forbid();
            }

            return View(requisition);
        }

        [Authorize(Roles = ApplicationRoles.DepartmentUser)]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!user.DepartmentId.HasValue)
            {
                TempData["ErrorMessage"] = "You must be assigned to a department to create requisitions. Please contact your administrator.";
                return RedirectToAction(nameof(Index));
            }

            var model = new RequisitionViewModel
            {
                Items = new List<RequisitionItemViewModel> { new RequisitionItemViewModel() }
            };

            await LoadItemsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.DepartmentUser)]
        public async Task<IActionResult> Create(RequisitionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!user.DepartmentId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "You must be assigned to a department to create requisitions.");
            }

            if (model.Items == null || !model.Items.Any(i => i.ItemId > 0 && i.Quantity > 0))
            {
                ModelState.AddModelError(string.Empty, "Please add at least one valid requisition item.");
            }

            if (!ModelState.IsValid)
            {
                await LoadItemsAsync();
                return View(model);
            }

            var requisition = new Requisition
            {
                RequisitionNumber = GenerateRequisitionNumber(),
                Title = model.Title,
                Description = model.Description,
                DepartmentId = user.DepartmentId!.Value,
                RequestedById = user.Id,
                Status = RequisitionStatus.Pending,
                RequestedDate = DateTime.UtcNow
            };

            _context.Requisitions.Add(requisition);
            await _context.SaveChangesAsync();

            var requisitionItems = model.Items
                .Where(i => i.ItemId > 0 && i.Quantity > 0)
                .Select(i => new RequisitionItem
                {
                    RequisitionId = requisition.Id,
                    ItemId = i.ItemId,
                    Quantity = i.Quantity,
                    Notes = i.Notes
                })
                .ToList();

            _context.RequisitionItems.AddRange(requisitionItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Requisition created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = ApplicationRoles.Administrator + "," + ApplicationRoles.Approver)]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requisition = await _context.Requisitions
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Item)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requisition == null)
            {
                return NotFound();
            }

            if (requisition.Status != RequisitionStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requisitions can be approved or rejected.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(requisition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Administrator + "," + ApplicationRoles.Approver)]
        public async Task<IActionResult> Approve(int id, string? statusNotes)
        {
            var requisition = await _context.Requisitions.FindAsync(id);
            if (requisition == null)
            {
                return NotFound();
            }

            if (requisition.Status != RequisitionStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requisitions can be approved.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            requisition.Status = RequisitionStatus.Approved;
            requisition.ApprovedById = user?.Id;
            requisition.ApprovedDate = DateTime.UtcNow;
            requisition.StatusNotes = statusNotes;

            _context.Update(requisition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Requisition approved successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRoles.Administrator + "," + ApplicationRoles.Approver)]
        public async Task<IActionResult> Reject(int id, string? statusNotes)
        {
            var requisition = await _context.Requisitions.FindAsync(id);
            if (requisition == null)
            {
                return NotFound();
            }

            if (requisition.Status != RequisitionStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending requisitions can be rejected.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            requisition.Status = RequisitionStatus.Rejected;
            requisition.StatusNotes = statusNotes;
            requisition.ApprovedDate = DateTime.UtcNow;
            requisition.ApprovedById = user?.Id;

            _context.Update(requisition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Requisition rejected successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadItemsAsync()
        {
            ViewBag.Items = new SelectList(
                await _context.Items.Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync(),
                "Id",
                "Name");
        }

        private string GenerateRequisitionNumber()
        {
            var today = DateTime.Today;
            var datePart = today.ToString("yyyyMMdd");
            var count = _context.Requisitions.Count(r => r.RequestedDate.Date == today) + 1;
            return $"REQ-{datePart}-{count.ToString().PadLeft(4, '0')}";
        }
    }
}
