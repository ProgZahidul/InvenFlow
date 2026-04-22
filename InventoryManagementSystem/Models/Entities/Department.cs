using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Department
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(20)]
            public string Code { get; set; }

            [StringLength(500)]
            public string Description { get; set; }

            public bool IsActive { get; set; } = true;

            // Navigation properties
            public ICollection<ApplicationUser> Users { get; set; }
            public ICollection<Requisition> Requisitions { get; set; }
            public ICollection<Issue> Issues { get; set; }
        }
}
