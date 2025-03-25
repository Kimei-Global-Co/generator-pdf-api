using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace PdfGeneratorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfQuotationController : ControllerBase
    {
        [HttpGet("generate")]
        public IActionResult GenerateQuotation()
        {
            var pdfStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Quotation Document").FontSize(20).Bold().AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Date: {DateTime.Now.ToShortDateString()}");
                        col.Item().Text("Customer: John Doe");
                        col.Item().Text("Product: Premium Car Service");
                        col.Item().Text("Total: $300");
                    });
                    page.Footer().Text("This is a sample quotation.").FontSize(10).AlignCenter();
                });
            }).GeneratePdf(pdfStream);

            pdfStream.Position = 0;
            return File(pdfStream.ToArray(), "application/pdf", "quotation.pdf");
        }
    }
}
