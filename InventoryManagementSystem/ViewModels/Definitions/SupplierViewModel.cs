using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class SupplierViewModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; }

            [StringLength(200)]
            public string Address { get; set; }

            [StringLength(20)]
            public string Phone { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            [StringLength(100)]
            public string ContactPerson { get; set; }
           // public bool IsActive { get; set; }

        }
        // ViewModels/ReportViewModels.cs
}
