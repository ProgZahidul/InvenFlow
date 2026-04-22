using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class ItemViewModel
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(500)]
            public string Description { get; set; }

            [Required, StringLength(50)]
            public string Code { get; set; }

            [Required]
            [Display(Name = "Unit of Measure")]
            public int UnitOfMeasureId { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? UnitPrice { get; set; }

            [Range(0, int.MaxValue)]
            public int ReorderLevel { get; set; } = 10;

            [Display(Name = "Category")]
            public int? CategoryId { get; set; }

            public bool IsActive { get; set; } = true;
        }
}
