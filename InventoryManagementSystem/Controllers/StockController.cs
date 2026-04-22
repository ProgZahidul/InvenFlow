using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Controllers
{
    // Controllers/StockController.cs
    [Authorize(Roles = "Administrator,InventoryManager")]
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public StockController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Stock
        public async Task<IActionResult> Index()
        {
            var stocks = await _context.Stocks
                .Include(s => s.Item)
                .Include(s => s.GRNItem)
                    .ThenInclude(gi => gi.GoodsReceivedNote)
                .Where(s => s.Quantity > 0)
                .OrderBy(s => s.Item.Name)
                .ThenBy(s => s.BatchNumber)
                .ToListAsync();

            return View(stocks);
        }

        // GET: Stock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .Include(s => s.Item)
                .Include(s => s.GRNItem)
                    .ThenInclude(gi => gi.GoodsReceivedNote)
                        .ThenInclude(grn => grn.PurchaseOrder)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        // GET: Stock/Adjust/5
        [Authorize(Roles = "Administrator,InventoryManager,Storekeeper")]
        public async Task<IActionResult> Adjust(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .Include(s => s.Item)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (stock == null)
            {
                return NotFound();
            }

            var model = new StockAdjustmentViewModel
            {
                StockId = stock.Id,
                ItemName = stock.Item.Name,
                BatchNumber = stock.BatchNumber,
                CurrentQuantity = stock.Quantity,
                AdjustmentQuantity = 0,
                Reason = ""
            };

            return View(model);
        }

        // POST: Stock/Adjust/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,InventoryManager,Storekeeper")]
        public async Task<IActionResult> Adjust(int id, StockAdjustmentViewModel model)
        {
            if (id != model.StockId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var stock = await _context.Stocks.FindAsync(model.StockId);
                if (stock == null)
                {
                    return NotFound();
                }

                // Create adjustment record
                var adjustment = new StockAdjustment
                {
                    StockId = stock.Id,
                    PreviousQuantity = stock.Quantity,
                    AdjustmentQuantity = model.AdjustmentQuantity,
                    NewQuantity = stock.Quantity + model.AdjustmentQuantity,
                    Reason = model.Reason,
                    AdjustedById = _userManager.GetUserId(User),
                    AdjustedDate = DateTime.UtcNow
                };

                // Update stock
                stock.Quantity += model.AdjustmentQuantity;
                stock.LastUpdated = DateTime.UtcNow;

                _context.Add(adjustment);
                _context.Update(stock);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }

   
}
