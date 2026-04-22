using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index(string status = "active")
        {
            ViewBag.CurrentStatus = status;

            IQueryable<Supplier> query = _context.Suppliers;

            switch (status.ToLower())
            {
                case "inactive":
                    query = query.Where(s => !s.IsActive);
                    break;
                case "all":
                    // No filter - show all
                    break;
                default:
                    query = query.Where(s => s.IsActive);
                    break;
            }

            var suppliers = await query
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(suppliers);
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }
        // POST: Suppliers/Reactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,InventoryManager")]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                {
                    return NotFound();
                }

                if (supplier.IsActive)
                {
                    TempData["InfoMessage"] = "Supplier is already active.";
                }
                else
                {
                    supplier.IsActive = true;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Supplier reactivated successfully.";
                }
            }
            catch (DbUpdateException ex)
            {
                // Optionally log the exception (if ILogger is injected)
                TempData["ErrorMessage"] = "An error occurred while reactivating the supplier.";
            }

            // After reactivation, go back to the inactive list
            return RedirectToAction(nameof(Index), new { status = "inactive" });
        }



        // GET: Suppliers/Create
        [Authorize(Roles = "Administrator,InventoryManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,InventoryManager")]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (ModelState.IsValid)
            {
                var supplier = new Supplier
                {
                    Name = model.Name,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    ContactPerson = model.ContactPerson,
                    IsActive = true
                };

                _context.Add(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Suppliers/Edit/5
        [Authorize(Roles = "Administrator,InventoryManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null )
            {
                return NotFound();
            }

            var model = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Address = supplier.Address,
                Phone = supplier.Phone,
                Email = supplier.Email,
                ContactPerson = supplier.ContactPerson
            };

            return View(model);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,InventoryManager")]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var supplier = await _context.Suppliers.FindAsync(id);
                    if (supplier == null)
                    {
                        return NotFound();
                    }

                    supplier.Name = model.Name;
                    supplier.Address = model.Address;
                    supplier.Phone = model.Phone;
                    supplier.Email = model.Email;
                    supplier.ContactPerson = model.ContactPerson;

                    _context.Update(supplier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Suppliers/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,InventoryManager")]
        public async Task<IActionResult> Deactivate(int id, string returnStatus = "active")
        {
            try
            {
                var supplier = await _context.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (supplier == null)
                {
                    return NotFound();
                }

                var supplierToUpdate = new Supplier { Id = id };
                _context.Suppliers.Attach(supplierToUpdate);
                supplierToUpdate.IsActive = false;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Supplier deactivated successfully.";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Error deactivating supplier.";
                // Log the exception here if needed
            }

            // Redirect back to the same filter view
            return RedirectToAction(nameof(Index), new { status = returnStatus });
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnStatus = "active")
        {
            try
            {
                var supplier = await _context.Suppliers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (supplier == null)
                {
                    return NotFound();
                }

                // Check again for purchase orders before deletion
                var hasPurchaseOrders = await _context.PurchaseOrders
                    .AnyAsync(po => po.SupplierId == id && po.Status != PurchaseOrderStatus.Cancelled);

                if (hasPurchaseOrders)
                {
                    TempData["ErrorMessage"] = "Cannot delete supplier with associated purchase orders.";
                    return RedirectToAction(nameof(Index), new { status = returnStatus });
                }

                var supplierToDelete = new Supplier { Id = id };
                _context.Suppliers.Attach(supplierToDelete);
                supplierToDelete.IsActive = false;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Supplier deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Error deleting supplier. It may have associated records.";
            }

            return RedirectToAction(nameof(Index), new { status = returnStatus });
        }

        // GET: Suppliers/Deactivate/5
        [Authorize(Roles = "Administrator,InventoryManager")]
        public async Task<IActionResult> Deactivate(int? id, string returnStatus = "active")
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            ViewBag.ReturnStatus = returnStatus;
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id, string returnStatus = "active")
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            // Check if supplier has associated purchase orders
            var hasPurchaseOrders = await _context.PurchaseOrders
                .AnyAsync(po => po.SupplierId == id && po.Status != PurchaseOrderStatus.Cancelled);

            if (hasPurchaseOrders)
            {
                TempData["ErrorMessage"] = "Cannot delete supplier with associated purchase orders. Please deactivate instead.";
                return RedirectToAction(nameof(Index), new { status = returnStatus });
            }

            ViewBag.ReturnStatus = returnStatus;
            return View(supplier);
        }


        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id);
        }
    }
}