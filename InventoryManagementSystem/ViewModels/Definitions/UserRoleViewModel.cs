using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class UserRoleViewModel
        {
            public ApplicationUser User { get; set; }
            public IList<string> Roles { get; set; }
        }

        // ViewModels/AdminViewModels.cs
}
