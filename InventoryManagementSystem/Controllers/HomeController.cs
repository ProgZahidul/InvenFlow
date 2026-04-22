namespace InventoryManagementSystem.Controllers;

using InventoryManagementSystem.Constants;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Dashboard));
        }
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var dashboard = new DashboardViewModel();
        var userId = _userManager.GetUserId(User);
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.InventoryManager) || User.IsInRole(ApplicationRoles.Approver))
        {
            dashboard.PendingRequisitions = await _context.Requisitions
                .Where(r => r.Status == RequisitionStatus.Pending)
                .CountAsync();
        }

        if (User.IsInRole(ApplicationRoles.DepartmentUser))
        {
            dashboard.MyPendingRequisitions = await _context.Requisitions
                .Where(r => r.RequestedById == userId && r.Status == RequisitionStatus.Pending)
                .CountAsync();
        }

        if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.InventoryManager))
        {
            dashboard.PendingPurchaseOrders = await _context.PurchaseOrders
                .Where(po => po.Status == PurchaseOrderStatus.Pending)
                .CountAsync();

            var lowStockItems = await _context.Items
                .Where(i => i.IsActive)
                .Select(i => new
                {
                    i.ReorderLevel,
                    TotalQuantity = i.Stocks.Sum(s => s.Quantity),
                    TotalReserved = i.Stocks.Sum(s => s.ReservedQuantity)
                })
                .ToListAsync();

            dashboard.LowStockItems = lowStockItems.Count(x => (x.TotalQuantity - x.TotalReserved) <= x.ReorderLevel);
        }

        if (User.IsInRole(ApplicationRoles.Administrator) || User.IsInRole(ApplicationRoles.InventoryManager) || User.IsInRole(ApplicationRoles.Storekeeper))
        {
            dashboard.RecentReceipts = await _context.GoodsReceivedNotes
                .Where(g => g.ReceivedDate >= thirtyDaysAgo)
                .CountAsync();

            dashboard.RecentIssues = await _context.Issues
                .Where(i => i.IssueDate >= thirtyDaysAgo)
                .CountAsync();
        }

        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
