using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Controllers
{
    // Controllers/PurchaseOrderController.cs
    [Authorize(Roles = "Administrator,InventoryManager,Storekeeper")]
    public class PurchaseOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INumberGeneratorService _numberGenerator;

        public PurchaseOrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INumberGeneratorService numberGenerator)
        {
            _context = context;
            _userManager = userManager;
            _numberGenerator = numberGenerator;
        }

        // GET: PurchaseOrder
        public async Task<IActionResult> Index()
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Requisition)
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .OrderByDescending(po => po.CreatedDate)
                .ToListAsync();

            return View(purchaseOrders);
        }

        // GET: PurchaseOrder/Create
        public async Task<IActionResult> Create(int requisitionId)
        {
            var requisition = await _context.Requisitions
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Item)
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.Id == requisitionId && r.Status == RequisitionStatus.Approved);

            if (requisition == null)
            {
                return NotFound();
            }

            var model = new PurchaseOrderViewModel
            {
                RequisitionId = requisitionId,
                RequisitionNumber = requisition.RequisitionNumber,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                Items = requisition.RequisitionItems.Select(ri => new PurchaseOrderItemViewModel
                {
                    ItemId = ri.ItemId,
                    ItemName = ri.Item.Name,
                    QuantityOrdered = ri.Quantity,
                    UnitPrice = ri.Item.UnitPrice ?? 0
                }).ToList()
            };

            ViewBag.Suppliers = new SelectList(_context.Suppliers.Where(s => s.IsActive), "Id", "Name");
            return View(model);
        }

        // POST: PurchaseOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var grnNumber = _numberGenerator.GenerateGRNNumber();
                var purchaseOrder = new PurchaseOrder
                {
                    PONumber = grnNumber,
                    RequisitionId = model.RequisitionId,
                    SupplierId = model.SupplierId,
                    CreatedById = user.Id,
                    CreatedDate = DateTime.UtcNow,
                    ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                    Status = PurchaseOrderStatus.Pending,
                    TotalAmount = model.Items.Sum(i => i.QuantityOrdered * i.UnitPrice),
                    StatusNotes = "" // or "Pending approval", whatever default you want
                };


                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                foreach (var item in model.Items)
                {
                    var poItem = new PurchaseOrderItem
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        ItemId = item.ItemId,
                        QuantityOrdered = item.QuantityOrdered,
                        UnitPrice = item.UnitPrice
                    };
                    _context.Add(poItem);
                }

                // Update requisition status
                var requisition = await _context.Requisitions.FindAsync(model.RequisitionId);
                requisition.Status = RequisitionStatus.PartiallyFulfilled;
                _context.Update(requisition);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
            }

            ViewBag.Suppliers = new SelectList(_context.Suppliers.Where(s => s.IsActive), "Id", "Name");
            return View(model);
        }

        // GET: PurchaseOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Requisition)
                .Include(po => po.Supplier)
                .Include(po => po.CreatedBy)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Item)
                .Include(po => po.GoodsReceivedNotes)
                    .ThenInclude(grn => grn.GRNItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // POST: PurchaseOrder/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            purchaseOrder.Status = PurchaseOrderStatus.Approved;
            _context.Update(purchaseOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        //private string GeneratePONumber()
        //{
        //    var date = DateTime.Now.ToString("yyyyMMdd");
        //    var count = _context.PurchaseOrders.Count(po => po.CreatedDate.Date == DateTime.Today) + 1;
        //    return $"PO-{date}-{count.ToString().PadLeft(4, '0')}";
        //}
    }

  
}
