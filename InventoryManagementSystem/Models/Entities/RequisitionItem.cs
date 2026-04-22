using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class RequisitionItem
        {
            public int Id { get; set; }
            public int RequisitionId { get; set; }
            public Requisition Requisition { get; set; }

            public int ItemId { get; set; }
            public Item Item { get; set; }

            [Range(1, int.MaxValue)]
            public int Quantity { get; set; }

            [StringLength(500)]
            public string Notes { get; set; }
        }
}
