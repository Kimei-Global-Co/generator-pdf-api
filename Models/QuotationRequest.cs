// Models/QuotationModels.cs
using System.Collections.Generic;

namespace PdfGeneratorApi.Models
{
    public class QuotationRequest
    {
        public QuotationWorkshopInfo QuotationWorkshopInfo { get; set; }
        public QuotationInfo QuotationInfo { get; set; }
        public QuotationCarInfo QuotationCarInfo { get; set; }
        public QuotationCustomerInfo QuotationCustomerInfo { get; set; }
        public List<QuoteItem> Quote { get; set; }
    }

    public class QuotationWorkshopInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Chain { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string BankInfo { get; set; }
    }

    public class QuotationInfo
    {
        public string Name { get; set; }
        public string ReceiptCode { get; set; }
        public string CarPickUpDate { get; set; }
        public string QuotationDate { get; set; }
    }

    public class QuotationCarInfo
    {
        public string LicensePlateNumber { get; set; }
        public string CarModel { get; set; }
        public string MachineNumber { get; set; }
        public string FrameNumber { get; set; }
        public string NumberOfKilometersTo { get; set; }
    }

    public class QuotationCustomerInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public bool CarOwner { get; set; }
    }

    public class QuoteItem
    {
        public string Product { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Money { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
    }
}