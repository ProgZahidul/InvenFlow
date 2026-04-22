using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class IssueItem
        {
            public int Id { get; set; }

            public int IssueId { get; set; }
            public Issue Issue { get; set; }

            public int ItemId { get; set; }
            public Item Item { get; set; }

            public int StockId { get; set; }
            public Stock Stock { get; set; }

            [Range(1, int.MaxValue)]
            public int Quantity { get; set; }
        }
    }
