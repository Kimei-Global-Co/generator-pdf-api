// Models/Common/SharedModels.cs
namespace PdfGeneratorApi.Models.Common
{
    public class WorkshopInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Chain { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public string AccountHolder { get; set; }
    }

    public class CarInfo
    {
        public string LicensePlate { get; set; }
        public string VIN { get; set; }
        public string Model { get; set; }
        public string Type { get; set; } = "Ô tô con 4 chỗ";
        public string CurrentMileage { get; set; }
        public string EngineNumber { get; set; }
    }

    public class CustomerInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public bool IsOwner { get; set; }
    }
}