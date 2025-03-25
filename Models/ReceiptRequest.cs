// Models/ReceiptRequest.cs
namespace PdfGeneratorApi.Models
{
    public class WorkshopInfo
    {
        public string Name { get; set; }
        public string Chain { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string BankInfo { get; set; }
    }

    public class ReceiptInfo
    {
        public string Name { get; set; }
        public string DateOfReceipt { get; set; }
        public string ServiceAdvisor { get; set; }
        public string ExpectedReleaseDate { get; set; }
    }

    public class CarInfo
    {
        public string LicensePlateNumber { get; set; }
        public string CarModel { get; set; }
        public string MachineNumber { get; set; }
        public string FrameNumber { get; set; }
        public string NumberOfKilometersTo { get; set; }
    }

    public class CustomerInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public bool CarOwner { get; set; }
    }

    public class RepairOption
    {
        public string VehicleCondition { get; set; }
        public List<string> RepairPlan { get; set; }
        public List<string> ReplacementParts { get; set; }
        public string Note { get; set; }
    }

    public class ReceiptRequest
    {
        public WorkshopInfo WorkshopInfo { get; set; }
        public ReceiptInfo ReceiptInfo { get; set; }
        public CarInfo CarInfo { get; set; }
        public CustomerInfo CustomerInfo { get; set; }
        public List<RepairOption> VehicleConditionAndRepairOptions { get; set; }
        public string Note { get; set; }
    }
}