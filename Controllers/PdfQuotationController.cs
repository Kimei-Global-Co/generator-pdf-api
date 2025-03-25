// Controllers/PdfQuotationController.cs
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font;
using PdfGeneratorApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using iText.Layout.Borders;

namespace PdfGeneratorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfQuotationController : ControllerBase
    {
        private readonly string _fontPath;

        public PdfQuotationController()
        {
            _fontPath = Path.Combine(AppContext.BaseDirectory, "assets", "NotoSans-Regular.ttf");
            if (!System.IO.File.Exists(_fontPath))
                throw new FileNotFoundException($"Font file not found at {_fontPath}");
        }

        [HttpPost("generate")]
        public IActionResult GenerateQuotationPdf([FromBody] QuotationRequest request)
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = new PdfWriter(stream);
            writer.SetCloseStream(false);

            try
            {
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {
                    // Setup fonts and colors
                    PdfFont font = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
                    PdfFont boldFont = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
                    Color primaryColor = new DeviceRgb(59, 89, 152);
                    Color lightGray = new DeviceRgb(240, 240, 240);

                    // 1. Workshop Header
                    document.Add(new Paragraph(request.QuotationWorkshopInfo.Name.ToUpper())
                        .SetFont(boldFont)
                        .SetFontSize(16)
                        .SetFontColor(primaryColor)
                        .SetMarginBottom(5));

                    var workshopInfo = new List<string>
                    {
                        $"Thuộc chuỗi: {request.QuotationWorkshopInfo.Chain}",
                        $"Địa chỉ: {request.QuotationWorkshopInfo.Address}",
                        $"Điện thoại: {request.QuotationWorkshopInfo.Phone}",
                        $"Website: {request.QuotationWorkshopInfo.Website}",
                        $"Số tài khoản: {request.QuotationWorkshopInfo.BankInfo}"
                    };

                    foreach (var info in workshopInfo)
                    {
                        document.Add(new Paragraph(info)
                            .SetFont(font)
                            .SetFontSize(10)
                            .SetMarginBottom(3));
                    }

                    //document.Add(new LineSeparator(new SolidBorder(1f))
                    //    .SetMarginTop(10)
                    //    .SetMarginBottom(15));

                    // 2. Quotation Title
                    document.Add(new Paragraph(request.QuotationInfo.Name)
                        .SetFont(boldFont)
                        .SetFontSize(18)
                        .SetFontColor(primaryColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20));

                    // 3. Basic Info Table
                    var infoTable = new Table(new float[] { 1, 1, 1, 1 })
                        .UseAllAvailableWidth()
                        .SetMarginBottom(20)
                        .SetBorder(Border.NO_BORDER);

                    infoTable.AddCell(CreateCell($"Mã phiếu: {request.QuotationInfo.ReceiptCode}", font));
                    infoTable.AddCell(CreateCell($"Ngày nhận xe: {request.QuotationInfo.CarPickUpDate}", font));
                    infoTable.AddCell(CreateCell($"Ngày báo giá: {request.QuotationInfo.QuotationDate}", font));

                    document.Add(infoTable);

                    // 4. Vehicle & Customer Info
                    var twoColTable = new Table(new float[] { 1, 1 })
                        .UseAllAvailableWidth()
                        .SetMarginBottom(20)
                        .SetBorder(Border.NO_BORDER);

                    // Vehicle Info
                    var vehicleCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPaddingRight(15)
                        .Add(new Paragraph("THÔNG TIN XE")
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetFontColor(primaryColor)
                            .SetMarginBottom(10))
                        .Add(CreateInfoRow("Biển số:", request.QuotationCarInfo.LicensePlateNumber, font, boldFont))
                        .Add(CreateInfoRow("Số máy:", request.QuotationCarInfo.MachineNumber, font, boldFont))
                        .Add(CreateInfoRow("Số khung:", request.QuotationCarInfo.FrameNumber, font, boldFont))
                        .Add(CreateInfoRow("Model:", request.QuotationCarInfo.CarModel, font, boldFont))
                        .Add(CreateInfoRow("Số km:", request.QuotationCarInfo.NumberOfKilometersTo, font, boldFont));

                    // Customer Info
                    var customerCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPaddingLeft(15)
                        .Add(new Paragraph("THÔNG TIN KHÁCH HÀNG")
                            .SetFont(boldFont)
                            .SetFontSize(14)
                            .SetFontColor(primaryColor)
                            .SetMarginBottom(10))
                        .Add(CreateInfoRow("Khách hàng:", request.QuotationCustomerInfo.Name, font, boldFont))
                        .Add(CreateInfoRow("Điện thoại:", request.QuotationCustomerInfo.Phone, font, boldFont))
                        .Add(CreateInfoRow("Email:", request.QuotationCustomerInfo.Email, font, boldFont))
                        .Add(CreateInfoRow("Địa chỉ:", request.QuotationCustomerInfo.Address, font, boldFont))
                        .Add(CreateInfoRow("Người liên hệ:", request.QuotationCustomerInfo.ContactPerson, font, boldFont))
                        .Add(CreateInfoRow("Là chủ xe:", request.QuotationCustomerInfo.CarOwner ? "Có" : "Không", font, boldFont));

                    twoColTable.AddCell(vehicleCell);
                    twoColTable.AddCell(customerCell);
                    document.Add(twoColTable);

                    // 5. Quotation Items
                    document.Add(new Paragraph("BÁO GIÁ")
                        .SetFont(boldFont)
                        .SetFontSize(14)
                        .SetFontColor(primaryColor)
                        .SetMarginBottom(15));

                    var quoteTable = new Table(new float[] { 3, 1, 1, 1, 1, 1, 1 })
                        .UseAllAvailableWidth()
                        .SetMarginBottom(20);

                    // Table Header
                    string[] headers = { "Sản phẩm/Dịch vụ", "Đơn vị", "Số lượng", "Đơn giá", "Thành tiền", "Chiết khấu", "VAT" };
                    foreach (var header in headers)
                    {
                        quoteTable.AddHeaderCell(
                            new Cell()
                                .Add(new Paragraph(header).SetFont(boldFont))
                                .SetBackgroundColor(lightGray)
                                .SetTextAlignment(TextAlignment.CENTER));
                    }

                    // Table Rows
                    foreach (var item in request.Quote)
                    {
                        quoteTable.AddCell(CreateCell(item.Product, font).SetPadding(5));
                        quoteTable.AddCell(CreateCell(item.Unit, font).SetTextAlignment(TextAlignment.CENTER));
                        quoteTable.AddCell(CreateCell(item.Quantity.ToString(), font).SetTextAlignment(TextAlignment.CENTER));
                        quoteTable.AddCell(CreateCell(item.UnitPrice.ToString("N0") + " đ", font).SetTextAlignment(TextAlignment.RIGHT));
                        quoteTable.AddCell(CreateCell(item.Money.ToString("N0") + " đ", font).SetTextAlignment(TextAlignment.RIGHT));
                        quoteTable.AddCell(CreateCell(item.Discount.ToString("N0") + " đ", font).SetTextAlignment(TextAlignment.RIGHT));
                        quoteTable.AddCell(CreateCell(item.Tax + "%", font).SetTextAlignment(TextAlignment.CENTER));
                    }

                    document.Add(quoteTable);

                    // 6. Summary
                    decimal totalAmount = request.Quote.Sum(x => x.Money);
                    decimal totalDiscount = request.Quote.Sum(x => x.Discount);
                    decimal totalTax = request.Quote.Sum(x => (x.Money * x.Tax / 100));
                    decimal finalAmount = totalAmount - totalDiscount + totalTax;

                    var summaryTable = new Table(new float[] { 1, 1 })
                        .UseAllAvailableWidth()
                        .SetMarginBottom(20)
                        .SetBorder(Border.NO_BORDER);

                    summaryTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                    var summaryCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .Add(new Paragraph($"Số lượng: {request.Quote.Count}").SetFont(font).SetMarginBottom(5))
                        .Add(new Paragraph($"Tổng cộng: {totalAmount:N0} VND").SetFont(font).SetMarginBottom(5))
                        .Add(new Paragraph($"Chiết khấu: {totalDiscount:N0} VND").SetFont(font).SetMarginBottom(5))
                        .Add(new Paragraph($"Thuế VAT: {totalTax:N0} VND").SetFont(font).SetMarginBottom(5))
                        .Add(new Paragraph($"Tổng thanh toán: {finalAmount:N0} VND").SetFont(boldFont));

                    summaryTable.AddCell(summaryCell);
                    document.Add(summaryTable);

                    // 7. Terms and Conditions
                    document.Add(new Paragraph("ĐIỀU KHOẢN")
                        .SetFont(boldFont)
                        .SetMarginTop(20)
                        .SetMarginBottom(10));

                    var terms = new List<string>
                    {
                        "1. Báo giá có hiệu lực trong vòng 7 ngày kể từ ngày lập",
                        "2. Thanh toán ít nhất 30% để xác nhận đơn hàng",
                        "3. Chi phí phát sinh sẽ được báo cáo riêng",
                        "4. Bảo hành 12 tháng với phụ tùng chính hãng",
                        "5. Thời gian hoàn thành phụ thuộc vào tình trạng xe thực tế"
                    };

                    foreach (var term in terms)
                    {
                        document.Add(new Paragraph(term).SetFont(font).SetMarginBottom(5));
                    }

                    // 8. Signature
                    var signatureTable = new Table(new float[] { 1, 1 })
                        .UseAllAvailableWidth()
                        .SetMarginTop(30);

                    signatureTable.AddCell(new Cell()
                        .Add(new Paragraph("ĐẠI DIỆN XƯỞNG").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER))
                        .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                        .SetBorder(Border.NO_BORDER));

                    signatureTable.AddCell(new Cell()
                        .Add(new Paragraph("KHÁCH HÀNG").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER))
                        .Add(new Paragraph("(Ký và ghi rõ họ tên)").SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                        .SetBorder(Border.NO_BORDER));

                    document.Add(signatureTable);
                }

                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", $"BaoGia_{request.QuotationInfo.ReceiptCode}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo phiếu báo giá",
                    error = ex.Message
                });
            }
        }

        private Cell CreateCell(string text, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font)).SetBorder(Border.NO_BORDER);
        }

        private Paragraph CreateInfoRow(string label, string value, PdfFont font, PdfFont boldFont)
        {
            return new Paragraph()
                .Add(new Text(label).SetFont(boldFont))
                .Add(new Text(" " + value).SetFont(font))
                .SetMarginBottom(3);
        }
    }
}