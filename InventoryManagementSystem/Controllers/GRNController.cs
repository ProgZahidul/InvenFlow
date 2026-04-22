using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Administrator,InventoryManager,Storekeeper")]
    public class GRNController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<GRNController> _logger;
        private readonly INumberGeneratorService _numberGenerator;
        public GRNController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IInventoryService inventoryService, ILogger<GRNController> logger, INumberGeneratorService numberGenerator)
        {
            _context = context;
            _userManager = userManager;
            _inventoryService = inventoryService;
            _logger = logger;
            _numberGenerator = numberGenerator;
        }

        // GET: GRN
        public async Task<IActionResult> Index()
        {
            var grns = await _context.GoodsReceivedNotes
                .Include(g => g.PurchaseOrder)
                .Include(g => g.ReceivedBy)
                .OrderByDescending(g => g.ReceivedDate)
                .ToListAsync();

            return View(grns);
        }

        // GET: GRN/Create
        public async Task<IActionResult> Create(int purchaseOrderId)
        {
            _logger.LogInformation($"Attempting to create GRN for PO ID: {purchaseOrderId}");

            try
            {
                // First check if the PO exists at all
                var poExists = await _context.PurchaseOrders.AnyAsync(p => p.Id == purchaseOrderId);
                if (!poExists)
                {
                    _logger.LogWarning($"PO with ID {purchaseOrderId} does not exist.");
                    TempData["ErrorMessage"] = $"Purchase Order #{purchaseOrderId} not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Now load the PO with all related data
                var po = await _context.PurchaseOrders
                    .AsNoTracking()
                    .Include(p => p.PurchaseOrderItems)
                        .ThenInclude(i => i.Item)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

                if (po == null)
                {
                    _logger.LogWarning($"PO with ID {purchaseOrderId} could not be loaded.");
                    TempData["ErrorMessage"] = $"Purchase Order #{purchaseOrderId} could not be loaded.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Loaded PO: {po.PONumber}, Status: {po.Status}, Supplier: {po.Supplier?.Name}, Items: {po.PurchaseOrderItems?.Count}");

                // Check if PO is in a valid state for receiving goods
                if (po.Status != PurchaseOrderStatus.Approved &&
                    po.Status != PurchaseOrderStatus.PartiallyReceived &&
                    po.Status != PurchaseOrderStatus.Ordered)
                {
                    _logger.LogWarning($"PO {po.PONumber} is in invalid status: {po.Status}");
                    TempData["ErrorMessage"] = $"Purchase Order {po.PONumber} is in '{po.Status}' status and cannot receive goods. Only Approved, Ordered, or PartiallyReceived POs can receive goods.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if PO has items
                if (po.PurchaseOrderItems == null || !po.PurchaseOrderItems.Any())
                {
                    _logger.LogWarning($"PO {po.PONumber} has no items.");
                    TempData["ErrorMessage"] = $"Purchase Order {po.PONumber} has no items to receive.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new GRNViewModel
                {
                    PurchaseOrderId = po.Id,
                    PONumber = po.PONumber,
                    SupplierName = po.Supplier?.Name ?? "Unknown Supplier",
                    Items = po.PurchaseOrderItems.Select(i => new GRNItemViewModel
                    {
                        PurchaseOrderItemId = i.Id,
                        ItemId = i.ItemId,
                        ItemName = i.Item?.Name ?? "Unknown Item",
                        QuantityOrdered = i.QuantityOrdered,
                        QuantityReceived = 0,
                        UnitPrice = i.UnitPrice,
                        BatchNumber = "",
                        ExpiryDate = DateTime.UtcNow.AddYears(1)
                    }).ToList()
                };

                _logger.LogInformation($"Created GRN view model with {model.Items.Count} items");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading PO {purchaseOrderId} for GRN creation");
                TempData["ErrorMessage"] = $"Error loading Purchase Order: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GRNViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.GetUserAsync(User);
                // Replace GenerateGRNNumber() calls with:
                var grnNumber = _numberGenerator.GenerateGRNNumber();

                // 1. Create GRN
                var grn = new GoodsReceivedNote
                {
                    GRNNumber = grnNumber,
                    PurchaseOrderId = model.PurchaseOrderId,
                    ReceivedById = user.Id,
                    ReceivedDate = DateTime.UtcNow,
                    Notes = model.Notes
                };
                _context.GoodsReceivedNotes.Add(grn);
                await _context.SaveChangesAsync(); // Generate GRN.Id

                // 2. Add GRNItems
                foreach (var item in model.Items)
                {
                    if (item.QuantityReceived <= 0) continue;

                    var grnItem = new GRNItem
                    {
                        GRNId = grn.Id,
                        PurchaseOrderItemId = item.PurchaseOrderItemId,
                        QuantityReceived = item.QuantityReceived,
                        UnitCost = item.UnitPrice,
                        BatchNumber = item.BatchNumber,
                        ExpiryDate = item.ExpiryDate
                    };
                    _context.GRNItems.Add(grnItem);
                }
                await _context.SaveChangesAsync();

                // 3. Update stock
                await _inventoryService.ProcessGoodsReceiptAsync(grn.Id);

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Details), new { id = grn.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error saving GRN: {ex.Message}");
                await PopulatePOData(model);
                return View(model);
            }
        }

        // Helper to repopulate PO data in case of error
        private async Task PopulatePOData(GRNViewModel model)
        {
            var po = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Item)
                .FirstOrDefaultAsync(p => p.Id == model.PurchaseOrderId);

            if (po != null)
            {
                model.PONumber = po.PONumber;
                model.SupplierName = po.Supplier?.Name ?? "N/A";
                model.Items = po.PurchaseOrderItems.Select(poi => new GRNItemViewModel
                {
                    PurchaseOrderItemId = poi.Id,
                    ItemId = poi.ItemId,
                    ItemName = poi.Item?.Name ?? "N/A",
                    QuantityOrdered = poi.QuantityOrdered,
                    QuantityReceived = 0,
                    UnitPrice = poi.UnitPrice,
                    BatchNumber = "",
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                }).ToList();
            }
        }

        // GET: GRN/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "GRN ID not specified.";
                return RedirectToAction(nameof(Index));
            }

            var grn = await _context.GoodsReceivedNotes
                .Include(g => g.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .Include(g => g.ReceivedBy)
                .Include(g => g.GRNItems)
                    .ThenInclude(gi => gi.PurchaseOrderItem)
                        .ThenInclude(poi => poi.Item)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null)
            {
                TempData["ErrorMessage"] = "Goods Received Note not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(grn);
        }

        //private string GenerateGRNNumber()
        //{
        //    var date = DateTime.Now.ToString("yyyyMMdd");
        //    var count = _context.GoodsReceivedNotes.Count(g => g.ReceivedDate.Date == DateTime.Today) + 1;
        //    return $"GRN-{date}-{count.ToString().PadLeft(4, '0')}";
        //}
    }
}