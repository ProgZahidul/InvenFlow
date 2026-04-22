using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class EditUserViewModel
        {
            public string Id { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            public int? DepartmentId { get; set; }

            public List<string> SelectedRoles { get; set; } = new List<string>();
            public List<string> AllRoles { get; set; } = new List<string>(); // Initialize with empty list
        }

        // ViewModels/RequisitionViewModels.cs
}
