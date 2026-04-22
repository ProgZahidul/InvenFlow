using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class UnitOfMeasure
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int UnitId { get; set; }

            [Display(Name = "Unit Name")]
            [Required(ErrorMessage = "Plz input Unit Name")]
            [StringLength(30)]
            [DataType("nvarchar(30)")]
            public string UnitName { get; set; }

            [Display(Name = "Short Name")]
            [StringLength(30)]
            [DataType("nvarchar(30)")]
            public string UnitShortName { get; set; }

        }
}
