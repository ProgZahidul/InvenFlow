using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class GRNViewModel
        {
            public int PurchaseOrderId { get; set; }
            public string PONumber { get; set; }
            public string SupplierName { get; set; }
            public string Notes { get; set; }
            public List<GRNItemViewModel> Items { get; set; } = new List<GRNItemViewModel>();
        }
}
