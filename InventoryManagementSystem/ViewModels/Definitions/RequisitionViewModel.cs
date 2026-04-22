using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class RequisitionViewModel
        {
            [Required]
            public string Title { get; set; }

            public string Description { get; set; }

            [Required]
            [MinLength(1, ErrorMessage = "At least one item is required")]
            public List<RequisitionItemViewModel> Items { get; set; } = new List<RequisitionItemViewModel>();
        }
}
