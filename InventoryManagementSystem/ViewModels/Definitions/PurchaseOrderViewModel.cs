using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class PurchaseOrderViewModel
        {
            public int RequisitionId { get; set; }
            public string RequisitionNumber { get; set; }

            [Required]
            [Display(Name = "Supplier")]
            public int SupplierId { get; set; }

            [Required]
            [Display(Name = "Expected Delivery Date")]
            public DateTime ExpectedDeliveryDate { get; set; }

            public List<PurchaseOrderItemViewModel> Items { get; set; } = new List<PurchaseOrderItemViewModel>();
        }
}
