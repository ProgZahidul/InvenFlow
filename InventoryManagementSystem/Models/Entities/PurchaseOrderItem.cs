using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class PurchaseOrderItem
        {
            public int Id { get; set; }

            public int PurchaseOrderId { get; set; }
            public PurchaseOrder PurchaseOrder { get; set; }

            public int ItemId { get; set; }
            public Item Item { get; set; }

            [Range(1, int.MaxValue)]
            public int QuantityOrdered { get; set; }

            [Range(0.01, double.MaxValue)]
            public decimal UnitPrice { get; set; }

            public ICollection<GRNItem> GRNItems { get; set; }
        }
}
