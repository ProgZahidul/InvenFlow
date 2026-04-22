using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class DashboardViewModel
        {
            public int PendingRequisitions { get; set; }
            public int PendingPurchaseOrders { get; set; }
            public int LowStockItems { get; set; }
            public int MyPendingRequisitions { get; set; }
            public int RecentReceipts { get; set; }
            public int RecentIssues { get; set; }
        }

        // ViewModels/AdminViewModels.cs
}
