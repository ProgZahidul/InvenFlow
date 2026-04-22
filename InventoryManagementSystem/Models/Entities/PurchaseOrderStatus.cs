using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public enum PurchaseOrderStatus { Pending, Approved, Ordered, PartiallyReceived, Completed, Cancelled }
}
