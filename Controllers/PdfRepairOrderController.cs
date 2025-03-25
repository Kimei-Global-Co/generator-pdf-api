// Controllers/PdfRepairOrderController.cs
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
    public class PdfRepairOrderController : ControllerBase
    {
        private readonly string _fontPath;

        public PdfRepairOrderController()
        {
            _fontPath = Path.Combine(AppContext.BaseDirectory, "assets", "NotoSans-Regular.ttf");
            if (!System.IO.File.Exists(_fontPath))
                throw new FileNotFoundException($"Font file not found at {_fontPath}");
        }

        [HttpPost("generate")]
        public IActionResult GenerateRepairOrderPdf([FromBody] RepairOrderRequest request)
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = null;
            PdfDocument pdf = null;
            Document document = null;

            try
            {
                // 1. Initialize PDF
                writer = new PdfWriter(stream);
                writer.SetCloseStream(false);
                pdf = new PdfDocument(writer);
                document = new Document(pdf);

                // 2. Load fonts
                PdfFont font = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);

                // Colors
                Color primaryColor = new DeviceRgb(59, 89, 152);
                Color lightGray = new DeviceRgb(240, 240, 240);

                /* ===== PART 1: WORKSHOP HEADER ===== */
                CreateWorkshopHeader(document, request.RepairOrderWorkshopInfo, boldFont, font, primaryColor);

                /* ===== PART 2: REPAIR ORDER TITLE ===== */
                document.Add(new Paragraph(request.RepairOrderInfo.Name)
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetFontColor(primaryColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(15)
                    .SetMarginBottom(20));

                /* ===== PART 3: BASIC INFO SECTION ===== */
                CreateBasicInfoSection(document, request.RepairOrderInfo, font, boldFont);

                /* ===== PART 4: VEHICLE INFORMATION ===== */
                //CreateVehicleInfoSection(document, request.RepairOrderCarInfo, font, boldFont, primaryColor);

                ///* ===== PART 5: CUSTOMER INFORMATION ===== */
                //CreateCustomerInfoSection(document, request.RepairOrderCustomerInfo, font, boldFont, primaryColor);

                CreateVehicleAndCustomerInfoSection(
                    document,
                    request.RepairOrderCarInfo,
                    request.RepairOrderCustomerInfo,
                    font,
                    boldFont,
                    primaryColor
                );

                /* ===== PART 6: REPAIR LABORS SECTION ===== */
                CreateRepairLaborsSection(document, request.RepairOrderLabors, font, boldFont, primaryColor, lightGray);

                /* ===== PART 7: FOOTER SECTION ===== */
                CreateFooterSection(document, request.RepairOrderInfo, request.RepairOrderLabors, font, boldFont);

                /* ===== FINALIZE ===== */
                document.Close();
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"Phieu_sua_chua_{DateTime.Now:yyyyMMddHHmmss}.pdf"
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
                    message = "Error generating repair order",
                    error = ex.Message
                });
            }
        }

        #region Helper Methods

        private void CreateWorkshopHeader(Document document, RepairOrderWorkshopInfo workshop, PdfFont boldFont, PdfFont font, Color primaryColor)
        {
            document.Add(new Paragraph(workshop.Name)
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetFontColor(primaryColor)
                .SetMarginBottom(5));

            var companyInfo = new List<string>
            {
                $"Địa chỉ: {workshop.Address}",
                $"Chi nhánh: {workshop.Chain}",
                $"Điện thoại: {workshop.Phone}",
                $"Website: {workshop.Website}",
                $"Tài khoản: {workshop.BankInfo}"
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

        private void CreateBasicInfoSection(Document document, RepairOrderInfo orderInfo, PdfFont font, PdfFont boldFont)
        {
            var infoTable = new Table(new float[] { 2, 3, 2, 3 })
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetBorder(Border.NO_BORDER);

            // Dòng 1: Mã phiếu báo giá và Cố vấn dịch vụ
            infoTable.AddCell(CreateInfoCell("Mã phiếu báo giá:", boldFont));
            infoTable.AddCell(CreateInfoCell(orderInfo.QuotationCode, font));
            infoTable.AddCell(CreateInfoCell("Cố vấn dịch vụ:", boldFont));
            infoTable.AddCell(CreateInfoCell(orderInfo.ServiceAdvisor, font));

            // Dòng 2: Ngày lập lệnh và Quản đốc
            infoTable.AddCell(CreateInfoCell("Ngày lập lệnh:", boldFont));
            infoTable.AddCell(CreateInfoCell(orderInfo.OrderDate, font));
            infoTable.AddCell(CreateInfoCell("Quản đốc:", boldFont));
            infoTable.AddCell(CreateInfoCell(orderInfo.Manager, font));

            // Dòng 3: Ngày nhận xe và Thời gian dự kiến hoàn thành
            infoTable.AddCell(CreateInfoCell("Ngày nhận xe:", boldFont));
            infoTable.AddCell(CreateInfoCell(orderInfo.DateOfVehicleReceipt, font));
            infoTable.AddCell(CreateInfoCell("Thời gian dự kiến hoàn thành:", boldFont));
            infoTable.AddCell(CreateInfoCell(orderInfo.EstCompletionTime, font));

            document.Add(infoTable);
        }

        private void CreateVehicleAndCustomerInfoSection(Document document, RepairOrderCarInfo car, RepairOrderCustomerInfo customer, PdfFont font, PdfFont boldFont, Color primaryColor)
        {
            // Tạo bảng chứa 2 cột song song
            var twoColTable = new Table(new float[] { 1, 1 })
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetBorder(Border.NO_BORDER);

            /* ===== CỘT THÔNG TIN XE ===== */
            var vehicleCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingRight(15);

            vehicleCell.Add(new Paragraph("1. THÔNG TIN XE")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            // Thêm các thông tin xe
            AddInfoItem(vehicleCell, "Biển số xe:", car.LicensePlateNumber, font, boldFont);
            AddInfoItem(vehicleCell, "Hãng xe:", car.CarModel, font, boldFont);
            AddInfoItem(vehicleCell, "Số máy:", car.MachineNumber, font, boldFont);
            AddInfoItem(vehicleCell, "Số khung:", car.FrameNumber, font, boldFont);
            AddInfoItem(vehicleCell, "Số km tiếp nhận:", car.NumberOfKilometersTo, font, boldFont);
            AddInfoItem(vehicleCell, "Ngày xuất xe dự kiến:", "", font, boldFont);

            vehicleCell.Add(new Paragraph("Ghi chú phiếu tiếp nhận:")
                .SetFont(boldFont)
                .SetMarginTop(10));
            vehicleCell.Add(new Paragraph("")
                .SetFont(font));

            /* ===== CỘT THÔNG TIN KHÁCH HÀNG ===== */
            var customerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(15);

            customerCell.Add(new Paragraph("2. THÔNG TIN KHÁCH HÀNG")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            // Thêm các thông tin khách hàng
            AddInfoItem(customerCell, "Khách hàng:", customer.Name, font, boldFont);
            AddInfoItem(customerCell, "Số điện thoại:", customer.Phone, font, boldFont);
            AddInfoItem(customerCell, "Email:", customer.Email, font, boldFont);
            AddInfoItem(customerCell, "Mã số thuế:", "", font, boldFont);
            AddInfoItem(customerCell, "Địa chỉ:", customer.Address, font, boldFont);
            AddInfoItem(customerCell, "Người liên hệ:", customer.ContactPerson, font, boldFont);

            customerCell.Add(new Paragraph("Yêu cầu của khách hàng:")
                .SetFont(boldFont)
                .SetMarginTop(10));
            customerCell.Add(new Paragraph("")
                .SetFont(font));

            // Thêm 2 cột vào bảng
            twoColTable.AddCell(vehicleCell);
            twoColTable.AddCell(customerCell);
            document.Add(twoColTable);
        }

        private void AddInfoItem(Cell cell, string label, string value, PdfFont font, PdfFont boldFont)
        {
            cell.Add(new Paragraph()
                .Add(new Text(label).SetFont(boldFont))
                .Add(new Text(value).SetFont(font))
                .SetMarginBottom(5));
        }

        //private void CreateVehicleInfoSection(Document document, RepairOrderCarInfo car, PdfFont font, PdfFont boldFont, Color primaryColor)
        //{
        //    document.Add(new Paragraph("1. Thông tin xe")
        //        .SetFont(boldFont)
        //        .SetFontSize(14)
        //        .SetFontColor(primaryColor)
        //        .SetMarginBottom(10));

        //    var table = new Table(new float[] { 2, 4, 2, 4 })
        //        .UseAllAvailableWidth()
        //        .SetMarginBottom(20);

        //    // Vehicle information rows
        //    AddTableRow(table, "Biển số xe", car.LicensePlateNumber, "Số VIN", car.FrameNumber, font, boldFont);
        //    AddTableRow(table, "Hãng xe", car.CarModel, "Loại xe", "", font, boldFont);
        //    AddTableRow(table, "Số km tiếp nhận", car.NumberOfKilometersTo, "Ngày xuất xe dự kiến", car.MachineNumber, font, boldFont);

        //    // Note row
        //    table.AddCell(new Cell(1, 4)
        //        .Add(new Paragraph("Ghi chú phiếu tiếp nhận").SetFont(boldFont))
        //        .SetPadding(5));
        //    table.AddCell(new Cell(1, 4)
        //        .Add(new Paragraph(""))
        //        .SetPadding(5));

        //    document.Add(table);
        //}

        //private void CreateCustomerInfoSection(Document document, RepairOrderCustomerInfo customer, PdfFont font, PdfFont boldFont, Color primaryColor)
        //{
        //    document.Add(new Paragraph("2. Thông tin khách hàng")
        //        .SetFont(boldFont)
        //        .SetFontSize(14)
        //        .SetFontColor(primaryColor)
        //        .SetMarginBottom(10));

        //    var table = new Table(new float[] { 2, 4, 2, 4 })
        //        .UseAllAvailableWidth()
        //        .SetMarginBottom(20);

        //    // Customer information rows
        //    AddTableRow(table, "Khách hàng", customer.Name, "Số điện thoại", customer.Phone, font, boldFont);
        //    AddTableRow(table, "Email", customer.Email, "Mã số thuế", "", font, boldFont);
        //    AddTableRow(table, "Địa chỉ", customer.Address, "", "", font, boldFont);

        //    // Customer request row
        //    table.AddCell(new Cell(1, 4)
        //        .Add(new Paragraph("Yêu cầu của khách hàng").SetFont(boldFont))
        //        .SetPadding(5));
        //    table.AddCell(new Cell(1, 4)
        //        .Add(new Paragraph(""))
        //        .SetPadding(5));

        //    document.Add(table);
        //}

        private void CreateRepairLaborsSection(Document document, List<RepairOrderLabor> labors, PdfFont font, PdfFont boldFont, Color primaryColor, Color lightGray)
        {
            document.Add(new Paragraph("3. Nhân công")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetMarginBottom(10));

            var table = new Table(new float[] { 1, 3, 1, 1, 2 })
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            // Table header
            table.AddHeaderCell(CreateHeaderCell("STT", boldFont, lightGray));
            table.AddHeaderCell(CreateHeaderCell("Tên hạng mục sửa chữa/ tên nhân công", boldFont, lightGray));
            table.AddHeaderCell(CreateHeaderCell("Số lượng", boldFont, lightGray));
            table.AddHeaderCell(CreateHeaderCell("Đơn vị", boldFont, lightGray));
            table.AddHeaderCell(CreateHeaderCell("Kỹ thuật viên sửa chữa", boldFont, lightGray));

            int itemNumber = 1;
            foreach (var labor in labors)
            {
                // Add note if exists
                if (!string.IsNullOrEmpty(labor.Note))
                {
                    table.AddCell(new Cell(1, 5)
                        .Add(new Paragraph(labor.Note).SetFont(font))
                        .SetPadding(5)
                        .SetBorder(Border.NO_BORDER));
                }

                // Add main labor item
                table.AddCell(new Cell()
                    .Add(new Paragraph(itemNumber.ToString()).SetFont(font))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
                table.AddCell(new Cell()
                    .Add(new Paragraph(labor.CategoryName).SetFont(font))
                    .SetPadding(5));
                table.AddCell(new Cell()
                    .Add(new Paragraph(labor.Quantity.ToString()).SetFont(font))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
                table.AddCell(new Cell()
                    .Add(new Paragraph(labor.Unit).SetFont(font))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
                table.AddCell(new Cell()
                    .Add(new Paragraph(labor.Technician).SetFont(font))
                    .SetPadding(5));

                // Add child workers if any
                if (labor.ChildWorker != null && labor.ChildWorker.Count > 0)
                {
                    foreach (var child in labor.ChildWorker)
                    {
                        table.AddCell(new Cell().SetPadding(5));
                        table.AddCell(new Cell()
                            .Add(new Paragraph(child.Product).SetFont(font))
                            .SetPadding(5));
                        table.AddCell(new Cell()
                            .Add(new Paragraph(child.QuantityChild.ToString()).SetFont(font))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPadding(5));
                        table.AddCell(new Cell()
                            .Add(new Paragraph(child.UnitChild).SetFont(font))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPadding(5));
                        table.AddCell(new Cell().SetPadding(5));
                    }
                }

                itemNumber++;
            }

            document.Add(table);
        }

        private void CreateFooterSection(Document document, RepairOrderInfo orderInfo, List<RepairOrderLabor> labors, PdfFont font, PdfFont boldFont)
        {
            // Tạo bảng ký tên với 4 cột
            var signatureTable = new Table(new float[] { 1, 1, 1, 1 })
                .UseAllAvailableWidth()
                .SetMarginTop(30);

            // Lấy danh sách kỹ thuật viên (lấy từ labor đầu tiên)
            var technicians = labors.FirstOrDefault()?.Technician ?? "Không xác định";

            // Thêm các ô ký tên
            signatureTable.AddCell(CreateSignatureCell("KHÁCH HÀNG", "(Ký, ghi rõ họ tên)", font, boldFont));
            signatureTable.AddCell(CreateSignatureCell("CỐ VẤN DỊCH VỤ", orderInfo.ServiceAdvisor, font, boldFont));
            signatureTable.AddCell(CreateSignatureCell("QUẢN ĐỐC", orderInfo.Manager, font, boldFont));
            signatureTable.AddCell(CreateSignatureCell("KỸ THUẬT VIÊN", technicians, font, boldFont));

            document.Add(signatureTable);
        }

        private Cell CreateSignatureCell(string title, string content, PdfFont font, PdfFont boldFont)
        {
            return new Cell()
                .Add(new Paragraph(title).SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph(content).SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER)
                .SetPaddingTop(20);
        }

        private Cell CreateInfoCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5);
        }

        private Cell CreateHeaderCell(string text, PdfFont font, Color backgroundColor)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(backgroundColor)
                .SetPadding(8);
        }

        private void AddTableRow(Table table, string label1, string value1, string label2, string value2, PdfFont font, PdfFont boldFont)
        {
            table.AddCell(new Cell().Add(new Paragraph(label1).SetFont(boldFont)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(value1).SetFont(font)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(label2).SetFont(boldFont)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(value2).SetFont(font)).SetPadding(5));
        }

        #endregion
    }
}