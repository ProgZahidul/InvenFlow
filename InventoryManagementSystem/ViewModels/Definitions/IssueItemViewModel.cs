using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class IssueItemViewModel
        {
            public int ItemId { get; set; }

            [Required]
            public int StockId { get; set; }

            [Range(1, int.MaxValue)]
            public int Quantity { get; set; }
        }
        // Add this ViewModel class to your ViewModels namespace or create it here
}
