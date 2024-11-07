namespace PdfGeneratorApi.Models
{
    public class PdfRequest
    {
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerCountry { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string PaymentDue { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceFrom { get; set; }
        public string ServiceUntil { get; set; }
        public string Amount { get; set; }
    }
}
