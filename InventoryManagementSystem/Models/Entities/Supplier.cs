using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Supplier
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(200)]
            public string Address { get; set; }

            [StringLength(20)]
            public string Phone { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            [StringLength(100)]
            public string ContactPerson { get; set; }

            public bool IsActive { get; set; } = true;

            public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        }
}
