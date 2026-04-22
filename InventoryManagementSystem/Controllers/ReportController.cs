using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    // Controllers/ReportController.cs
    [Authorize(Roles = "Administrator,InventoryManager")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        // Controllers/ReportController.cs
        public async Task<IActionResult> StockReport()
        {
            var stockData = await _context.Stocks
                .Include(s => s.Item)
                    .ThenInclude(i => i.UnitOfMeasure) // include UnitOfMeasure
                .GroupBy(s => new
                {
                    s.Item.Id,
                    s.Item.Name,
                    s.Item.Code,
                    UnitName = s.Item.UnitOfMeasure.UnitName,
                    s.Item.ReorderLevel
                })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.Code,
                    g.Key.UnitName,
                    g.Key.ReorderLevel,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    ReservedQuantity = g.Sum(s => s.ReservedQuantity)
                })
                .ToListAsync();

            var result = stockData.Select(x => new StockReportViewModel
            {
                ItemName = x.Name,
                ItemCode = x.Code,
                UnitOfMeasure = x.UnitName,
                TotalQuantity = x.TotalQuantity,
                ReservedQuantity = x.ReservedQuantity,

                AvailableQuantity = x.TotalQuantity - x.ReservedQuantity,
                ReorderLevel = x.ReorderLevel
            })
            .OrderBy(s => s.ItemName)
            .ToList();

            return View(result);
        }



        public async Task<IActionResult> ExpiryReport()
        {
            var expiryData = await _context.Stocks
                .Include(s => s.Item)
                .Where(s => s.ExpiryDate.HasValue && s.ExpiryDate > DateTime.UtcNow && s.Quantity > 0)
                .OrderBy(s => s.ExpiryDate)
                .Select(s => new ExpiryReportViewModel
                {
                    ItemName = s.Item.Name,
                    BatchNumber = s.BatchNumber,
                    ExpiryDate = s.ExpiryDate.Value,
                    Quantity = s.Quantity,
                    DaysUntilExpiry = EF.Functions.DateDiffDay(DateTime.UtcNow, s.ExpiryDate.Value)
                })
                .ToListAsync();

            return View(expiryData);
        }

        //public async Task<IActionResult> TransactionReport(DateTime? fromDate, DateTime? toDate)
        //{
        //    fromDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
        //    toDate = toDate ?? DateTime.UtcNow;

        //    var receipts = await _context.GRNItems
        //        .Where(gi => gi.GoodsReceivedNote.ReceivedDate >= fromDate && gi.GoodsReceivedNote.ReceivedDate <= toDate)
        //        .Select(gi => new TransactionReportViewModel
        //        {
        //            Date = gi.GoodsReceivedNote.ReceivedDate,
        //            Type = "Receipt",
        //            ItemName = gi.PurchaseOrderItem.Item.Name,
        //            Quantity = gi.QuantityReceived,
        //            Reference = gi.GoodsReceivedNote.GRNNumber,
        //            UnitCost = gi.UnitCost,
        //            TotalCost = gi.QuantityReceived * gi.UnitCost
        //        })
        //        .ToListAsync();

        //    var issues = await _context.IssueItems
        //        .Where(ii => ii.Issue.IssueDate >= fromDate && ii.Issue.IssueDate <= toDate)
        //        .Select(ii => new TransactionReportViewModel
        //        {
        //            Date = ii.Issue.IssueDate,
        //            Type = "Issue",
        //            ItemName = ii.Item.Name,
        //            Quantity = ii.Quantity,
        //            Reference = ii.Issue.IssueNumber,
        //            UnitCost = ii.Stock.UnitCost,
        //            TotalCost = ii.Quantity * ii.Stock.UnitCost
        //        })
        //        .ToListAsync();

        //    var transactions = receipts.Concat(issues)
        //        .OrderByDescending(t => t.Date)
        //        .ToList();

        //    ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
        //    ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

        //    return View(transactions);
        //}
        public async Task<IActionResult> TransactionReport(DateTime? fromDate, DateTime? toDate)
        {
            fromDate = fromDate ?? DateTime.Today.AddDays(-30);
            toDate = toDate ?? DateTime.Today;

            var receipts = await _context.GRNItems
                .Where(gi => gi.GoodsReceivedNote.ReceivedDate >= fromDate &&
                             gi.GoodsReceivedNote.ReceivedDate < toDate.Value.AddDays(1))
                .Select(gi => new TransactionReportViewModel
                {
                    Date = gi.GoodsReceivedNote.ReceivedDate,
                    Type = "Receipt",
                    ItemName = gi.PurchaseOrderItem.Item.Name,
                    Quantity = gi.QuantityReceived,
                    Reference = gi.GoodsReceivedNote.GRNNumber,
                    UnitCost = gi.UnitCost,
                    TotalCost = gi.QuantityReceived * gi.UnitCost
                })
                .ToListAsync();

            var issues = await _context.IssueItems
                .Where(ii => ii.Issue.IssueDate >= fromDate &&
                             ii.Issue.IssueDate < toDate.Value.AddDays(1))
                .Select(ii => new TransactionReportViewModel
                {
                    Date = ii.Issue.IssueDate,
                    Type = "Issue",
                    ItemName = ii.Item.Name,
                    Quantity = ii.Quantity,
                    Reference = ii.Issue.IssueNumber,
                    UnitCost = ii.Stock.UnitCost,
                    TotalCost = ii.Quantity * ii.Stock.UnitCost
                })
                .ToListAsync();

            var transactions = receipts.Concat(issues)
                .OrderByDescending(t => t.Date)
                .ToList();

            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(transactions);
        }
        //public async Task<IActionResult> RequisitionReport(RequisitionStatus? status, DateTime? fromDate, DateTime? toDate)
        //{
        //    fromDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
        //    toDate = toDate ?? DateTime.UtcNow;

        //    var query = _context.Requisitions
        //        .Include(r => r.Department)
        //        .Include(r => r.RequestedBy)
        //        .Include(r => r.ApprovedBy)
        //        .Include(r => r.RequisitionItems)
        //        .Where(r => r.RequestedDate >= fromDate && r.RequestedDate <= toDate);

        //    if (status.HasValue)
        //    {
        //        query = query.Where(r => r.Status == status.Value);
        //    }

        //    var requisitions = await query
        //        .OrderByDescending(r => r.RequestedDate)
        //        .ToListAsync();

        //    ViewBag.Status = status;
        //    ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
        //    ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

        //    return View(requisitions);
        //}
        public async Task<IActionResult> RequisitionReport(RequisitionStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            fromDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
            toDate = toDate ?? DateTime.UtcNow;

            var query = _context.Requisitions
                .Include(r => r.Department)
                .Include(r => r.RequestedBy)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RequisitionItems)
                .Where(r => r.RequestedDate >= fromDate && r.RequestedDate < toDate.Value.AddDays(1));

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            var requisitions = await query
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            return View(requisitions);
        }
    }

    
}
