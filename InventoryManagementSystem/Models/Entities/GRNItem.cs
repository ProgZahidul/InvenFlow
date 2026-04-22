using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class GRNItem
        {
            public int Id { get; set; }

            public int GRNId { get; set; }
            public GoodsReceivedNote GoodsReceivedNote { get; set; }

            public int PurchaseOrderItemId { get; set; }
            public PurchaseOrderItem PurchaseOrderItem { get; set; }

            [Required, StringLength(50)]
            public string BatchNumber { get; set; }

            [Range(1, int.MaxValue)]
            public int QuantityReceived { get; set; }

            public DateTime? ExpiryDate { get; set; }

            [Range(0.01, double.MaxValue)]
            public decimal UnitCost { get; set; }

            public ICollection<Stock> Stocks { get; set; }
        }
}
