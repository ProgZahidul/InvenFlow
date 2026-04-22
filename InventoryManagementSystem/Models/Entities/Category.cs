using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Category
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(500)]
            public string Description { get; set; }

            public bool IsActive { get; set; } = true;

            public ICollection<Item> Items { get; set; }
        }
}
