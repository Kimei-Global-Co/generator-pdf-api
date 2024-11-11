namespace PdfGeneratorApi.Models
{
    public class LaborItem
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Discount { get; set; }
        public string Tax { get; set; }
        public string Notes { get; set; }
    }
}