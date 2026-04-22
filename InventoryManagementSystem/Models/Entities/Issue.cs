using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Issue
        {
            public int Id { get; set; }

            [Required, StringLength(50)]
            public string IssueNumber { get; set; }

            public int DepartmentId { get; set; }
            public Department Department { get; set; }

            [Required]
            public string RequestedById { get; set; }
            public ApplicationUser RequestedBy { get; set; }

            [Required]
            public string IssuedById { get; set; }
            public ApplicationUser IssuedBy { get; set; }

            public DateTime IssueDate { get; set; } = DateTime.UtcNow;

            [StringLength(1000)]
            public string Notes { get; set; }

            public ICollection<IssueItem> IssueItems { get; set; }
        }
}
