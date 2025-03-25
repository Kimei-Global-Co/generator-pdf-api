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
//using PdfGeneratorApi.Models.Common;


namespace PdfGeneratorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfReceiptController : ControllerBase
    {
        private readonly string _fontPath;

        public PdfReceiptController()
        {
            _fontPath = Path.Combine(AppContext.BaseDirectory, "assets", "NotoSans-Regular.ttf");

            if (!System.IO.File.Exists(_fontPath))
                throw new FileNotFoundException($"Không tìm thấy file font tại {_fontPath}");
        }

        [HttpPost("generate")]
        public IActionResult GenerateReceiptPdf([FromBody] ReceiptRequest request)
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = null;
            PdfDocument pdf = null;
            Document document = null;

            try
            {
                // 1. Khởi tạo PDF
                writer = new PdfWriter(stream);
                writer.SetCloseStream(false); // Tách thành dòng riêng

                pdf = new PdfDocument(writer);
                document = new Document(pdf);

                // 2. Tải font
                PdfFont font = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
                //boldFont.SetBold();

                // Màu sắc
                Color primaryColor = new DeviceRgb(59, 89, 152);
                Color lightGray = new DeviceRgb(240, 240, 240);

                /* ===== PHẦN 1: HEADER CÔNG TY ===== */
                CreateCompanyHeader(document, request.WorkshopInfo, boldFont, font, primaryColor);

                /* ===== PHẦN 2: TIÊU ĐỀ PHIẾU ===== */
                document.Add(new Paragraph(request.ReceiptInfo.Name)
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetFontColor(primaryColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(15)
                    .SetMarginBottom(20));

                /* ===== PHẦN 3: THÔNG TIN CƠ BẢN - CÙNG HÀNG ===== */
                CreateBasicInfoSection(document, request.ReceiptInfo, font, boldFont);

                /* ===== PHẦN 4: THÔNG TIN XE & KHÁCH HÀNG ===== */
                CreateVehicleAndCustomerInfo(document, request.CarInfo, request.CustomerInfo, font, boldFont, primaryColor);

                /* ===== PHẦN 5: TÌNH TRẠNG XE & PHƯƠNG ÁN SỬA CHỮA ===== */
                CreateConditionAndRepairSection(document, request.VehicleConditionAndRepairOptions, font, boldFont, primaryColor, lightGray);

                /* ===== PHẦN 6: CHÂN TRANG ===== */
                CreateFooterSection(document, request.Note, font, boldFont);

                /* ===== HOÀN TẤT ===== */
                document.Close();
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"Phieu_tiep_nhan_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                document?.Close();
                pdf?.Close();
                writer?.Close();
                stream?.Dispose();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo phiếu tiếp nhận",
                    error = ex.Message
                });
            }
        }

        #region Các phương thức helper

        private void CreateCompanyHeader(Document document, WorkshopInfo workshop, PdfFont boldFont, PdfFont font, Color primaryColor)
        {
            document.Add(new Paragraph(workshop.Name)
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetFontColor(primaryColor)
                .SetMarginBottom(5));

            var companyInfo = new List<string>
            {
                $"Thuộc chuỗi: {workshop.Chain}",
                $"Địa chỉ: {workshop.Address}",
                $"Số điện thoại: {workshop.Phone}",
                $"Website: {workshop.Website}",
                $"Số tài khoản: {workshop.BankInfo}"
            };

            foreach (var line in companyInfo)
            {
                document.Add(new Paragraph(line)
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetMarginBottom(3));
            }

            //document.Add(new LineSeparator(new SolidBorder(1f))
            //    .SetMarginTop(10)
            //    .SetMarginBottom(15));
        }

        private void CreateBasicInfoSection(Document document, ReceiptInfo receipt, PdfFont font, PdfFont boldFont)
        {
            var infoTable = new Table(new float[] { 1, 1, 1 })
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetBorder(Border.NO_BORDER);

            infoTable.AddCell(CreateInfoCell($"Ngày tiếp nhận: {receipt.DateOfReceipt}", font));
            infoTable.AddCell(CreateInfoCell($"Cố vấn dịch vụ: {receipt.ServiceAdvisor}", font));
            infoTable.AddCell(CreateInfoCell($"Ngày ra dự kiến: {receipt.ExpectedReleaseDate}", font));

            document.Add(infoTable);
        }

        private Cell CreateInfoCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetBorder(Border.NO_BORDER);
        }

        private void CreateVehicleAndCustomerInfo(Document document, CarInfo car, CustomerInfo customer, PdfFont font, PdfFont boldFont, Color primaryColor)
        {
            var twoColTable = new Table(new float[] { 1, 1 })
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetBorder(Border.NO_BORDER);

            // Cột thông tin xe
            var vehicleCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingRight(15);

            vehicleCell.Add(new Paragraph("THÔNG TIN XE")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            var vehicleInfo = new List<(string, string)>
            {
                ("Biển số xe:", car.LicensePlateNumber),
                ("Mẫu xe:", car.CarModel),
                ("Số máy:", car.MachineNumber),
                ("Số khung:", car.FrameNumber),
                ("Số km vào:", car.NumberOfKilometersTo)
            };

            foreach (var (label, value) in vehicleInfo)
            {
                vehicleCell.Add(new Paragraph()
                    .Add(new Text(label).SetFont(boldFont))
                    .Add(new Text(" " + value).SetFont(font))
                    .SetMarginBottom(5));
            }

            // Cột thông tin khách hàng
            var customerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(15);

            customerCell.Add(new Paragraph("THÔNG TIN KHÁCH HÀNG")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            var customerInfo = new List<(string, string)>
            {
                ("Khách hàng:", customer.Name),
                ("Số điện thoại:", customer.Phone),
                ("Email:", customer.Email),
                ("Địa chỉ:", customer.Address),
                ("Người liên hệ:", customer.ContactPerson),
                ("Là chủ xe:", customer.CarOwner ? "Có" : "Không")
            };

            foreach (var (label, value) in customerInfo)
            {
                customerCell.Add(new Paragraph()
                    .Add(new Text(label).SetFont(boldFont))
                    .Add(new Text(" " + value).SetFont(font))
                    .SetMarginBottom(5));
            }

            twoColTable.AddCell(vehicleCell);
            twoColTable.AddCell(customerCell);
            document.Add(twoColTable);
        }

        private void CreateConditionAndRepairSection(Document document, List<RepairOption> repairOptions, PdfFont font, PdfFont boldFont, Color primaryColor, Color lightGray)
        {
            document.Add(new Paragraph("TÌNH TRẠNG XE & PHƯƠNG ÁN SỬA CHỮA")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(15));

            var table = new Table(new float[] { 25, 25, 25, 25 })
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            // Header với cách viết rõ ràng
            table.AddHeaderCell(
                new Cell()
                    .Add(new Paragraph("Tình trạng xe"))
                    .SetFont(boldFont)
                    .SetBackgroundColor(lightGray)
                    .SetTextAlignment(TextAlignment.CENTER)
            );

            table.AddHeaderCell(
                new Cell()
                    .Add(new Paragraph("Phương án sửa chữa"))
                    .SetFont(boldFont)
                    .SetBackgroundColor(lightGray)
                    .SetTextAlignment(TextAlignment.CENTER)
            );

            table.AddHeaderCell(
                new Cell()
                    .Add(new Paragraph("Bộ phận thay thế"))
                    .SetFont(boldFont)
                    .SetBackgroundColor(lightGray)
                    .SetTextAlignment(TextAlignment.CENTER)
            );

            table.AddHeaderCell(
                new Cell()
                    .Add(new Paragraph("Ghi chú"))
                    .SetFont(boldFont)
                    .SetBackgroundColor(lightGray)
                    .SetTextAlignment(TextAlignment.CENTER)
            );

            foreach (var item in repairOptions)
            {
                // Các dòng dữ liệu
                table.AddCell(new Cell().Add(new Paragraph(item.VehicleCondition).SetFont(font)).SetPadding(8));
                table.AddCell(new Cell().Add(new Paragraph(string.Join("\n", item.RepairPlan.Select(x => $"- {x}"))).SetFont(font)).SetPadding(8));
                table.AddCell(new Cell().Add(new Paragraph(string.Join("\n", item.ReplacementParts.Select(x => $"- {x}"))).SetFont(font)).SetPadding(8));
                table.AddCell(new Cell().Add(new Paragraph(item.Note).SetFont(font)).SetPadding(8));
            }

            document.Add(table);
        }

        private void CreateFooterSection(Document document, string note, PdfFont font, PdfFont boldFont)
        {
            document.Add(new Paragraph("Ghi chú:")
                .SetFont(boldFont)
                .SetMarginTop(20));

            document.Add(new Paragraph(note)
                .SetFont(font)
                .SetMarginBottom(30));

            // Chữ ký
            var signatureTable = new Table(new float[] { 1, 1 })
                .UseAllAvailableWidth()
                .SetMarginTop(30);

            signatureTable.AddCell(new Cell()
                .Add(new Paragraph("Cố vấn dịch vụ").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph("(Ký, ghi rõ họ tên)").SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER));

            signatureTable.AddCell(new Cell()
                .Add(new Paragraph("Xác nhận của khách hàng").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph("(Ký, ghi rõ họ tên)").SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER));

            document.Add(signatureTable);
        }

        #endregion
    }
}