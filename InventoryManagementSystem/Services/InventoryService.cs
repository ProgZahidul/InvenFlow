using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessGoodsReceiptAsync(int grnId)
        {
            var grn = await _context.GoodsReceivedNotes
                .Include(g => g.GRNItems)
                .ThenInclude(gi => gi.PurchaseOrderItem)
                .FirstOrDefaultAsync(g => g.Id == grnId);

            if (grn == null)
                throw new ArgumentException("GRN not found");

            foreach (var grnItem in grn.GRNItems)
            {
                // Update or create stock
                var existingStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ItemId == grnItem.PurchaseOrderItem.ItemId &&
                                             s.BatchNumber == grnItem.BatchNumber);

                if (existingStock != null)
                {
                    existingStock.Quantity += grnItem.QuantityReceived;
                    existingStock.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    var newStock = new Stock
                    {
                        ItemId = grnItem.PurchaseOrderItem.ItemId,
                        GRNItemId = grnItem.Id,
                        BatchNumber = grnItem.BatchNumber,
                        ExpiryDate = grnItem.ExpiryDate,
                        UnitCost = grnItem.UnitCost,
                        Quantity = grnItem.QuantityReceived,
                        ReservedQuantity = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.Stocks.Add(newStock);
                }
            }

            // Update purchase order status
            var po = await _context.PurchaseOrders
                .Include(p => p.PurchaseOrderItems)
                .ThenInclude(pi => pi.GRNItems)
                .FirstOrDefaultAsync(p => p.Id == grn.PurchaseOrderId);

            if (po != null)
            {
                var allItemsReceived = po.PurchaseOrderItems.All(pi =>
                    pi.QuantityOrdered <= pi.GRNItems.Sum(gi => gi.QuantityReceived));

                var someItemsReceived = po.PurchaseOrderItems.Any(pi =>
                    pi.GRNItems.Sum(gi => gi.QuantityReceived) > 0);

                if (allItemsReceived)
                {
                    po.Status = PurchaseOrderStatus.Completed;
                }
                else if (someItemsReceived)
                {
                    po.Status = PurchaseOrderStatus.PartiallyReceived;
                }

                // Update requisition status if all items are fulfilled
                var requisition = await _context.Requisitions
                    .Include(r => r.RequisitionItems)
                    .Include(r => r.PurchaseOrders)
                    .ThenInclude(po => po.PurchaseOrderItems)
                    .ThenInclude(pi => pi.GRNItems)
                    .FirstOrDefaultAsync(r => r.Id == po.RequisitionId);

                if (requisition != null)
                {
                    var totalRequisitionQuantity = requisition.RequisitionItems.Sum(ri => ri.Quantity);
                    var totalReceivedQuantity = requisition.PurchaseOrders
                        .SelectMany(po => po.PurchaseOrderItems)
                        .Sum(pi => pi.GRNItems.Sum(gi => gi.QuantityReceived));

                    if (totalReceivedQuantity >= totalRequisitionQuantity)
                    {
                        requisition.Status = RequisitionStatus.Completed;
                    }
                    else if (totalReceivedQuantity > 0)
                    {
                        requisition.Status = RequisitionStatus.PartiallyFulfilled;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task ProcessIssueAsync(int issueId)
        {
            var issue = await _context.Issues
                .Include(i => i.IssueItems)
                .ThenInclude(ii => ii.Stock)
                .FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null)
                throw new ArgumentException("Issue not found");

            foreach (var issueItem in issue.IssueItems)
            {
                var stock = await _context.Stocks.FindAsync(issueItem.StockId);
                if (stock == null)
                    throw new ArgumentException($"Stock not found for ID {issueItem.StockId}");

                if (stock.Quantity < issueItem.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for item {stock.ItemId}");

                stock.Quantity -= issueItem.Quantity;
                stock.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckStockLevelsAsync()
        {
            // Get items with their stock information
            var itemsWithStock = await _context.Items
                .Where(i => i.IsActive)
                .Select(i => new
                {
                    Item = i,
                    TotalQuantity = i.Stocks.Sum(s => s.Quantity),
                    TotalReserved = i.Stocks.Sum(s => s.ReservedQuantity)
                })
                .ToListAsync();

            var lowStockItems = itemsWithStock
                .Where(x => (x.TotalQuantity - x.TotalReserved) <= x.Item.ReorderLevel)
                .ToList();

            foreach (var item in lowStockItems)
            {
                var availableQuantity = item.TotalQuantity - item.TotalReserved;
                _logger.LogWarning($"Low stock alert for item: {item.Item.Name} " +
                                  $"(Current: {availableQuantity}, Reorder: {item.Item.ReorderLevel})");
            }
        }

        public async Task CheckExpiryAlertsAsync()
        {
            var soon = DateTime.UtcNow.AddDays(30);
            var expiringItems = await _context.Stocks
                .Include(s => s.Item)
                .Where(s => s.ExpiryDate.HasValue && s.ExpiryDate <= soon && s.Quantity > 0)
                .ToListAsync();

            foreach (var stock in expiringItems)
            {
                var daysUntilExpiry = (stock.ExpiryDate.Value - DateTime.UtcNow).Days;
                _logger.LogWarning($"Expiry alert for item: {stock.Item.Name}, " +
                                  $"Batch: {stock.BatchNumber}, " +
                                  $"Expiry: {stock.ExpiryDate:yyyy-MM-dd}, " +
                                  $"Days left: {daysUntilExpiry}");
            }
        }
    }
}