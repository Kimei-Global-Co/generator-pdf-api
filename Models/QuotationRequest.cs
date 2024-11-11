namespace PdfGeneratorApi.Models
{
    public class QuotationRequest
    {
        public string CompanyEmail { get; set; }

        public string BelongChain { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string Website { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string AccountHolder { get; set; }
        public string InvoiceNumber { get; set; }
        public string ReceiveDate { get; set; }
        public string IssueDate { get; set; }
        public string CarLicensePlate { get; set; }
        public string CarVIN { get; set; }
        public string CarBrand { get; set; }
        public string CarType { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public List<LaborItem> LaborItems { get; set; }
        public List<ProductItem> ProductItems { get; set; }
    }
}
