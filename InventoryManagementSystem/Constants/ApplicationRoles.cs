namespace InventoryManagementSystem.Constants
{
    public static class ApplicationRoles
    {
        public const string Administrator = "Administrator";
        public const string DepartmentUser = "DepartmentUser";
        public const string InventoryManager = "InventoryManager";
        public const string Storekeeper = "Storekeeper";
        public const string Approver = "Approver";
        public const string Supplier = "Supplier";

        public static readonly string[] Ordered =
        {
            Administrator,
            DepartmentUser,
            InventoryManager,
            Storekeeper,
            Approver,
            Supplier
        };

        public static readonly string[] All = Ordered;
    }
}
