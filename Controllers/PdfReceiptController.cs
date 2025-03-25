using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font;
using System;
using System.IO;
using System.Collections.Generic;
using iText.Layout.Borders;

namespace PdfGeneratorApi.Controllers
{
    public class RepairItem
    {
        public string Condition { get; set; }
        public List<string> Solutions { get; set; }
        public string ReplacementParts { get; set; }
        public string Notes { get; set; }
    }

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

        [HttpGet("generate")]
        public IActionResult GenerateReceiptPdf()
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
                document.Add(new Paragraph("XƯỞNG AUTO CAR NAM TỪ LIÊM")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetFontColor(primaryColor)
                    .SetMarginBottom(5));

                var companyInfo = new List<string>
                {
                    "Thuộc chuỗi: Chi nhánh Hà Nội",
                    "Địa chỉ: Số 68 Đường 70, Xuân Phương, Nam Từ Liêm, Hà Nội",
                    "Số điện thoại: 033.456.3456",
                    "Website: autocar.vn",
                    "Số tài khoản: 190235348346 - Techcombank - Chủ tài khoản: Trần Ngọc Thăng"
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

                /* ===== PHẦN 2: TIÊU ĐỀ PHIẾU ===== */
                document.Add(new Paragraph("PHIẾU TIẾP NHẬN PTN1214")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetFontColor(primaryColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(15)
                    .SetMarginBottom(20));

                /* ===== PHẦN 3: THÔNG TIN CƠ BẢN - CÙNG HÀNG ===== */
                var infoTable = new Table(new float[] { 1, 1, 1 })
                    .UseAllAvailableWidth()
                    .SetMarginBottom(20)
                    .SetBorder(Border.NO_BORDER);

                infoTable.AddCell(new Cell()
                    .Add(new Paragraph("Ngày tiếp nhận: 11/06/2024")
                    .SetFont(font))
                    .SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell()
                    .Add(new Paragraph("Cố vấn dịch vụ: Trần Hùng")
                    .SetFont(font))
                    .SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell()
                    .Add(new Paragraph("Ngày ra dự kiến: 29/06/2024")
                    .SetFont(font))
                    .SetBorder(Border.NO_BORDER));

                document.Add(infoTable);

                /* ===== PHẦN 4: THÔNG TIN XE & KHÁCH HÀNG - 2 CỘT SONG SONG ===== */
                var twoColTable = new Table(new float[] { 1, 1 })
                    .UseAllAvailableWidth()
                    .SetMarginBottom(20)
                    .SetBorder(Border.NO_BORDER);

                // Cột thông tin xe
                var vehicleCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetPaddingRight(15); // Thêm khoảng cách giữa 2 cột

                vehicleCell.Add(new Paragraph("THÔNG TIN XE")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetFontColor(primaryColor)
                    .SetMarginBottom(10));

                var vehicleInfo = new List<(string, string)>
                    {
                        ("Biển số xe:", "18K1-353.6688"),
                        ("Mẫu xe:", "SUV-B"),
                        ("Số máy:", "23"),
                        ("Số khung:", "46"),
                        ("Số km vào:", "153214 km")
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
                    .SetPaddingLeft(15); // Thêm khoảng cách giữa 2 cột

                customerCell.Add(new Paragraph("THÔNG TIN KHÁCH HÀNG")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetFontColor(primaryColor)
                    .SetMarginBottom(10));

                var customerInfo = new List<(string, string)>
                    {
                        ("Khách hàng:", "Nguyễn Ngọc Tú"),
                        ("Số điện thoại:", "034.458.3245"),
                        ("Email:", "ngoclu34@gmail.com"),
                        ("Địa chỉ:", "Số 5 ngõ 12/15 Đồng Me, Nam Từ Liêm, Hà Nội"),
                        ("Người liên hệ:", "Trần Hùng (0348468233)"),
                        ("Là chủ xe:", "Có")
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

                /* ===== PHẦN 5: TÌNH TRẠNG XE & PHƯƠNG ÁN SỬA CHỮA (4 CỘT) ===== */
                document.Add(new Paragraph("TÌNH TRẠNG XE & PHƯƠNG ÁN SỬA CHỮA")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetFontColor(primaryColor)
                    .SetMarginBottom(15));

                // Tạo bảng với 4 cột
                var repairTable = new Table(new float[] { 30, 25, 25, 20 }) // Điều chỉnh % độ rộng các cột
                    .UseAllAvailableWidth()
                    .SetMarginBottom(20);

                // Header với màu nền
                var headerStyle = new Style()
                    .SetBackgroundColor(lightGray)
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(8);

                repairTable.AddHeaderCell(new Cell().Add(new Paragraph("Tình trạng xe").AddStyle(headerStyle)));
                repairTable.AddHeaderCell(new Cell().Add(new Paragraph("Phương án sửa chữa").AddStyle(headerStyle)));
                repairTable.AddHeaderCell(new Cell().Add(new Paragraph("Bộ phận thay thế").AddStyle(headerStyle)));
                repairTable.AddHeaderCell(new Cell().Add(new Paragraph("Ghi chú").AddStyle(headerStyle)));

                // Dữ liệu mẫu
                var repairData = new List<RepairItem>
                {
                    new RepairItem
                    {
                        Condition = "Gạt mưa Audi A6 Sedan 2019-2022 bị hỏng",
                        Solutions = new List<string> { "Thay mới gạt mưa", "Kiểm tra hệ thống điện" },
                        ReplacementParts = "Gạt mưa chính hãng Audi",
                        Notes = "Bảo hành 6 tháng"
                    },
                    new RepairItem
                    {
                        Condition = "Đèn pha bên trái không sáng",
                        Solutions = new List<string> { "Thay bóng đèn", "Kiểm tra hệ thống điện" },
                        ReplacementParts = "Bóng đèn Osram H7",
                        Notes = "Kiểm tra thêm công tắc đèn"
                    },
                    new RepairItem
                    {
                        Condition = "Lốp trước bên phải mòn không đều",
                        Solutions = new List<string> { "Thay lốp mới", "Cân bằng lốp" },
                        ReplacementParts = "Lốp Hankook 235/45R18",
                        Notes = "Kiểm tra hệ thống treo"
                    }
                };

                // Thêm dữ liệu vào bảng
                foreach (var item in repairData)
                {
                    // Cột Tình trạng xe
                    repairTable.AddCell(new Cell()
                        .Add(new Paragraph(item.Condition).SetFont(font))
                        .SetPadding(8)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE));

                    // Cột Phương án sửa chữa (list các phương án)
                    var solutionCell = new Cell().SetPadding(5);
                    foreach (var solution in item.Solutions)
                    {
                        solutionCell.Add(new Paragraph($"• {solution}").SetFont(font));
                    }
                    repairTable.AddCell(solutionCell);

                    // Cột Bộ phận thay thế
                    repairTable.AddCell(new Cell()
                        .Add(new Paragraph(item.ReplacementParts).SetFont(font))
                        .SetPadding(8)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE));

                    // Cột Ghi chú
                    repairTable.AddCell(new Cell()
                        .Add(new Paragraph(item.Notes).SetFont(font))
                        .SetPadding(8)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE));
                }

                document.Add(repairTable);

                /* ===== PHẦN 6: CHÂN TRANG ===== */
                document.Add(new Paragraph("Ghi chú:")
                    .SetFont(boldFont)
                    .SetMarginTop(20));

                document.Add(new Paragraph("Vui lòng kiểm tra kỹ thông tin trước khi ký xác nhận")
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
    }
}