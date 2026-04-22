using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class IssueViewModel
        {
            [Required]
            [Display(Name = "Department")]
            public int DepartmentId { get; set; }

            // REMOVED: RequestedById since we're using current user
            // [Required]
            // [Display(Name = "Requested By")]
            // public string RequestedById { get; set; }

            [Required]
            [Display(Name = "Issue Date")]
            public DateTime IssueDate { get; set; }

            public string Notes { get; set; }

            [Required]
            [MinLength(1, ErrorMessage = "At least one item is required")]
            public List<IssueItemViewModel> Items { get; set; } = new List<IssueItemViewModel>();
        }
}
