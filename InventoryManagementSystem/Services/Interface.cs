namespace InventoryManagementSystem.Services
{
    public interface IInventoryService
    {
        Task ProcessGoodsReceiptAsync(int grnId);
        Task ProcessIssueAsync(int issueId);
        Task CheckStockLevelsAsync();
        Task CheckExpiryAlertsAsync();
    }
}
