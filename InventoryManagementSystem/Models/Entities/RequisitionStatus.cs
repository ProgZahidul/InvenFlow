using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public enum RequisitionStatus { Pending, Approved, Rejected, PartiallyFulfilled, Completed }
}
