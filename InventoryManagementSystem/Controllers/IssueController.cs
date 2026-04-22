using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Administrator,InventoryManager,Storekeeper")]
    public class IssueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<IssueController> _logger;
        private readonly INumberGeneratorService _numberGenerator;

        public IssueController(ApplicationDbContext context,
                             UserManager<ApplicationUser> userManager,
                             IInventoryService inventoryService,
                             ILogger<IssueController> logger,
                             INumberGeneratorService numberGenerator)
        {
            _context = context;
            _userManager = userManager;
            _inventoryService = inventoryService;
            _logger = logger;
            _numberGenerator = numberGenerator;
        }

        // GET: Issue
        public async Task<IActionResult> Index()
        {
            try
            {
                var issues = await _context.Issues
                    .Include(i => i.Department)
                    .Include(i => i.RequestedBy)
                    .Include(i => i.IssuedBy)
                    .Include(i => i.IssueItems)
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync();

                _logger.LogInformation($"Loaded {issues.Count} issues");
                return View(issues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading issues");
                TempData["ErrorMessage"] = "Error loading issues. Please try again.";
                return View(new List<Issue>());
            }
        }

        // GET: Issue/Create
        public async Task<IActionResult> Create()
        {
            var model = new IssueViewModel
            {
                IssueDate = DateTime.UtcNow,
                Items = new List<IssueItemViewModel> { new IssueItemViewModel() }
            };

            ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name");
            ViewBag.Items = new SelectList(_context.Items.Where(i => i.IsActive), "Id", "Name");
            ViewBag.Stocks = new SelectList(new List<Stock>(), "Id", "BatchInfo");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueViewModel model)
        {
            _logger.LogInformation("Issue creation started");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Current user: {user?.Id}, {user?.UserName}");

                if (user == null)
                {
                    _logger.LogWarning("Current user not found");
                    ModelState.AddModelError("", "User not authenticated.");
                    await RepopulateViewBags();
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var issueNumber = _numberGenerator.GenerateIssueNumber();
                    _logger.LogInformation($"Generated issue number: {issueNumber}");

                    var issue = new Issue
                    {
                        IssueNumber = issueNumber,
                        DepartmentId = model.DepartmentId,
                        RequestedById = user.Id,
                        IssuedById = user.Id,
                        IssueDate = model.IssueDate,
                        Notes = model.Notes
                    };

                    _context.Add(issue);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Issue created with ID: {issue.Id}");

                    // Validate and add issue items
                    foreach (var item in model.Items.Where(i => i.StockId > 0 && i.Quantity > 0))
                    {
                        var stock = await _context.Stocks.FindAsync(item.StockId);
                        if (stock == null)
                        {
                            throw new Exception($"Stock with ID {item.StockId} not found");
                        }

                        var availableQuantity = stock.Quantity - stock.ReservedQuantity;
                        if (item.Quantity > availableQuantity)
                        {
                            throw new Exception($"Insufficient stock for item {item.ItemId}. Available: {availableQuantity}, Requested: {item.Quantity}");
                        }

                        var issueItem = new IssueItem
                        {
                            IssueId = issue.Id,
                            ItemId = item.ItemId,
                            StockId = item.StockId,
                            Quantity = item.Quantity
                        };
                        _context.Add(issueItem);
                        _logger.LogInformation($"Added issue item: ItemId={item.ItemId}, StockId={item.StockId}, Qty={item.Quantity}");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Process the issue to update stock - THIS CALLS THE SERVICE NOW
                    await _inventoryService.ProcessIssueAsync(issue.Id);

                    _logger.LogInformation("Issue processed successfully");
                    TempData["SuccessMessage"] = "Items issued successfully!";
                    return RedirectToAction(nameof(Details), new { id = issue.Id });
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "Database error creating issue");

                    if (dbEx.InnerException is SqlException sqlEx)
                    {
                        _logger.LogError($"SQL Error Number: {sqlEx.Number}, Message: {sqlEx.Message}");

                        switch (sqlEx.Number)
                        {
                            case 547:
                                ModelState.AddModelError("", "Invalid data reference. Please check your selections.");
                                break;
                            case 2601:
                            case 2627:
                                ModelState.AddModelError("", "Duplicate issue number detected. Please try again.");
                                break;
                            default:
                                ModelState.AddModelError("", "Database error. Please try again.");
                                break;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error saving to database. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unexpected error creating issue");
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Model error: {error.ErrorMessage}");
                }
            }

            await RepopulateViewBags();
            return View(model);
        }

        // GET: Issue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.Issues
                .Include(i => i.Department)
                .Include(i => i.RequestedBy)
                .Include(i => i.IssuedBy)
                .Include(i => i.IssueItems)
                    .ThenInclude(ii => ii.Item)
                .Include(i => i.IssueItems)
                    .ThenInclude(ii => ii.Stock)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (issue == null)
            {
                return NotFound();
            }

            return View(issue);
        }

        // AJAX: Get available stocks for item
        [HttpGet]
        public async Task<JsonResult> GetStocksForItem(int itemId)
        {
            try
            {
                var stocks = await _context.Stocks
                    .Where(s => s.ItemId == itemId && (s.Quantity - s.ReservedQuantity) > 0)
                    .OrderBy(s => s.ExpiryDate)
                    .Select(s => new
                    {
                        Id = s.Id,
                        BatchInfo = $"{s.BatchNumber} (Exp: {s.ExpiryDate:yyyy-MM-dd}, Available: {s.Quantity - s.ReservedQuantity})",
                        AvailableQuantity = s.Quantity - s.ReservedQuantity
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {stocks.Count} stocks for item {itemId}");
                return Json(stocks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting stocks for item {itemId}");
                return Json(new { error = "Error loading stocks" });
            }
        }

        // REMOVED: ProcessIssueAsync method from here - it should be in InventoryService

        private async Task RepopulateViewBags()
        {
            try
            {
                ViewBag.Departments = new SelectList(_context.Departments.Where(d => d.IsActive), "Id", "Name");
                ViewBag.Items = new SelectList(_context.Items.Where(i => i.IsActive), "Id", "Name");
                ViewBag.Stocks = new SelectList(new List<Stock>(), "Id", "BatchInfo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repopulating view bags");
            }
        }

        //private string GenerateIssueNumber()
        //{
        //    var date = DateTime.Now.ToString("yyyyMMdd");

        //    // Use a database query to get the next sequence number
        //    var lastIssueNumber = _context.Issues
        //        .Where(i => i.IssueNumber.StartsWith($"ISS-{date}-"))
        //        .OrderByDescending(i => i.IssueNumber)
        //        .Select(i => i.IssueNumber)
        //        .FirstOrDefault();

        //    int nextSequence = 1;

        //    if (!string.IsNullOrEmpty(lastIssueNumber))
        //    {
        //        var sequencePart = lastIssueNumber.Substring($"ISS-{date}-".Length);
        //        if (int.TryParse(sequencePart, out int lastSequence))
        //        {
        //            nextSequence = lastSequence + 1;
        //        }
        //    }

        //    return $"ISS-{date}-{nextSequence.ToString().PadLeft(4, '0')}";
        //}
    }
}