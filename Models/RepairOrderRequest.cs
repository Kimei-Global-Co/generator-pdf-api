// Models/RepairOrderRequest.cs
namespace PdfGeneratorApi.Models
{
    public class RepairOrderRequest
    {
        public RepairOrderWorkshopInfo RepairOrderWorkshopInfo { get; set; }
        public RepairOrderInfo RepairOrderInfo { get; set; }
        public RepairOrderCarInfo RepairOrderCarInfo { get; set; }
        public RepairOrderCustomerInfo RepairOrderCustomerInfo { get; set; }
        public List<RepairOrderLabor> RepairOrderLabors { get; set; }
    }

    public class RepairOrderWorkshopInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Chain { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string BankInfo { get; set; }
    }

    public class RepairOrderInfo
    {
        public string Name { get; set; }
        public string QuotationCode { get; set; }
        public string OrderDate { get; set; }
        public string DateOfVehicleReceipt { get; set; }
        public string EstCompletionTime { get; set; }
        public string ServiceAdvisor { get; set; }
        public string Manager { get; set; }
    }

    public class RepairOrderCarInfo
    {
        public string LicensePlateNumber { get; set; }
        public string CarModel { get; set; }
        public string MachineNumber { get; set; }
        public string FrameNumber { get; set; }
        public string NumberOfKilometersTo { get; set; }
    }

    public class RepairOrderCustomerInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public bool CarOwner { get; set; }
    }

    public class RepairOrderLabor
    {
        public string Note { get; set; }
        public string CategoryName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string Technician { get; set; }
        public List<ChildWorker> ChildWorker { get; set; }
    }

    public class ChildWorker
    {
        public string Product { get; set; }
        public int QuantityChild { get; set; }
        public string UnitChild { get; set; }
    }
}