using InventoryManagementSystem.Constants;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
            }

            CreateRolesAsync().Wait();
            CreateAdministratorAsync().Wait();
            SeedInitialDataAsync().Wait();
            CreateSampleUsersAsync().Wait();
        }

        private async Task CreateRolesAsync()
        {
            foreach (var roleName in ApplicationRoles.All)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task CreateAdministratorAsync()
        {
            var adminEmail = "admin@inventory.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null)
            {
                if (!await _userManager.IsInRoleAsync(adminUser, ApplicationRoles.Administrator))
                {
                    await _userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
                }
                return;
            }

            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
            }
        }


        private async Task CreateSampleUsersAsync()
        {
            await EnsureUserWithRoleAsync("departmentuser@gmail.com", "Department", "User", "1220022@Sk", ApplicationRoles.DepartmentUser, "PROC");
            await EnsureUserWithRoleAsync("inventorymanager@gmail.com", "Inventory", "Manager", "1220022@Sk", ApplicationRoles.InventoryManager, "STORE");
            await EnsureUserWithRoleAsync("storekeeper@gmail.com", "Store", "Keeper", "1220022@Sk", ApplicationRoles.Storekeeper, "STORE");
            await EnsureUserWithRoleAsync("approve@gmail.com", "Main", "Approver", "1220022@Sk", ApplicationRoles.Approver, "ADMIN");
            await EnsureUserWithRoleAsync("supplier@gmail.com", "Default", "Supplier", "1220022@Sk", ApplicationRoles.Supplier, null);
        }

        private async Task EnsureUserWithRoleAsync(string email, string firstName, string lastName, string password, string roleName, string? departmentCode)
        {
            var user = await _userManager.FindByEmailAsync(email);
            Department? department = null;
            if (!string.IsNullOrWhiteSpace(departmentCode))
            {
                department = await _context.Departments.FirstOrDefaultAsync(d => d.Code == departmentCode);
            }

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    DepartmentId = department?.Id
                };

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    return;
                }
            }
            else
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.DepartmentId = department?.Id;
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, roleName);
        }

        private async Task SeedInitialDataAsync()
        {
            if (!_context.Departments.Any())
            {
                await _context.Departments.AddRangeAsync(
                    new Department { Name = "Administration", Code = "ADMIN", Description = "Administrative department" },
                    new Department { Name = "IT", Code = "IT", Description = "Information technology department" },
                    new Department { Name = "Accounts", Code = "ACC", Description = "Accounts and finance department" },
                    new Department { Name = "Procurement", Code = "PROC", Description = "Procurement and purchasing" },
                    new Department { Name = "Store", Code = "STORE", Description = "Inventory and store operations" }
                );
            }

            if (!_context.Categories.Any())
            {
                await _context.Categories.AddRangeAsync(
                    new Category { Name = "Office Equipment", Description = "Computers, printers, projectors and related equipment" },
                    new Category { Name = "Furniture", Description = "Desks, chairs, cabinets and tables" },
                    new Category { Name = "Stationery", Description = "Paper, pens, files and general office supplies" },
                    new Category { Name = "IT & Networking", Description = "Servers, switches, routers and accessories" },
                    new Category { Name = "Facilities", Description = "Maintenance and facility support items" }
                );
            }

            if (!_context.UnitOfMeasures.Any())
            {
                await _context.UnitOfMeasures.AddRangeAsync(
                    new UnitOfMeasure { UnitName = "Piece", UnitShortName = "Pc" },
                    new UnitOfMeasure { UnitName = "Set", UnitShortName = "Set" },
                    new UnitOfMeasure { UnitName = "Box", UnitShortName = "Box" }
                );
            }

            await _context.SaveChangesAsync();

            if (!_context.Items.Any())
            {
                var officeCategory = _context.Categories.First(c => c.Name == "Office Equipment");
                var furnitureCategory = _context.Categories.First(c => c.Name == "Furniture");
                var stationeryCategory = _context.Categories.First(c => c.Name == "Stationery");
                var pieceUnit = _context.UnitOfMeasures.First(u => u.UnitName == "Piece");
                var setUnit = _context.UnitOfMeasures.First(u => u.UnitName == "Set");
                var boxUnit = _context.UnitOfMeasures.First(u => u.UnitName == "Box");

                await _context.Items.AddRangeAsync(
                    new Item { Name = "Desktop Computer", Code = "OFF001", Description = "Standard office desktop computer", UnitOfMeasureId = pieceUnit.UnitId, UnitPrice = 600.00m, ReorderLevel = 10, CategoryId = officeCategory.Id },
                    new Item { Name = "Laser Printer", Code = "OFF002", Description = "High-speed office laser printer", UnitOfMeasureId = pieceUnit.UnitId, UnitPrice = 250.00m, ReorderLevel = 5, CategoryId = officeCategory.Id },
                    new Item { Name = "Office Chair", Code = "FUR001", Description = "Ergonomic office chair", UnitOfMeasureId = pieceUnit.UnitId, UnitPrice = 80.00m, ReorderLevel = 20, CategoryId = furnitureCategory.Id },
                    new Item { Name = "Whiteboard Markers", Code = "STA001", Description = "Pack of whiteboard markers", UnitOfMeasureId = boxUnit.UnitId, UnitPrice = 12.00m, ReorderLevel = 50, CategoryId = stationeryCategory.Id },
                    new Item { Name = "Conference Table Set", Code = "FUR002", Description = "Large conference table with chairs", UnitOfMeasureId = setUnit.UnitId, UnitPrice = 1200.00m, ReorderLevel = 2, CategoryId = furnitureCategory.Id }
                );
            }

            await _context.SaveChangesAsync();
        }
    }
}
