using InventoryManagementSystem.Data;

namespace InventoryManagementSystem.Services
{
    public interface INumberGeneratorService
    {
        string GenerateRequisitionNumber();
        string GeneratePONumber();
        string GenerateGRNNumber();
        string GenerateIssueNumber();
    }

    public class NumberGeneratorService : INumberGeneratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NumberGeneratorService> _logger;

        public NumberGeneratorService(ApplicationDbContext context, ILogger<NumberGeneratorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GenerateRequisitionNumber() => GenerateNumber("REQ");
        public string GeneratePONumber() => GenerateNumber("PO");
        public string GenerateGRNNumber() => GenerateNumber("GRN");
        public string GenerateIssueNumber() => GenerateNumber("ISS");

        private static string GenerateNumber(string prefix)
        {
            var timestamp = DateTime.Now;
            return $"{prefix}-{timestamp:yyyyMMdd-HHmmssfff}";
        }
    }
}
