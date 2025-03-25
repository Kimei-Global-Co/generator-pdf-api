using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Image;
using System.IO;
using System;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Borders;

namespace PdfGeneratorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfRepairOrderController : ControllerBase
    {
        private readonly string _fontPath;
        private readonly string _logoPath;

        public PdfRepairOrderController()
        {
            // Đường dẫn đến các file trong wwwroot/assets
            _fontPath = Path.Combine(AppContext.BaseDirectory, "assets", "NotoSans-Regular.ttf");
            _logoPath = Path.Combine(AppContext.BaseDirectory, "assets", "logo.png");

            // Kiểm tra file tồn tại
            if (!System.IO.File.Exists(_fontPath))
                throw new FileNotFoundException($"Không tìm thấy file font tại {_fontPath}");

            if (!System.IO.File.Exists(_logoPath))
                throw new FileNotFoundException($"Không tìm thấy file logo tại {_logoPath}");
        }

        [HttpGet("generate")]
        public IActionResult GeneratePdf()
        {
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = null;
            PdfDocument pdf = null;
            Document document = null;

            try
            {
                // Cấu hình PDF Writer không tự động đóng stream
                writer = new PdfWriter(stream);
                writer.SetCloseStream(false); // Tách thành dòng riêng

                pdf = new PdfDocument(writer);
                document = new Document(pdf);

                // Tải font và logo
                PdfFont font = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
                Image logo = new Image(ImageDataFactory.Create(_logoPath))
                    .SetWidth(100)
                    .SetAutoScale(true);

                // Thiết lập font và margin cho toàn bộ tài liệu
                document.SetFont(font);
                document.SetMargins(30, 30, 30, 30);

                /* ===== TẠO NỘI DUNG PDF ===== */

                // 1. Header với logo
                var headerTable = new Table(2).UseAllAvailableWidth();
                headerTable.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER));
                headerTable.AddCell(new Cell()
                    .Add(new Paragraph("CÔNG TY DỊCH VỤ Ô TÔ")
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER));
                document.Add(headerTable);

                // 2. Tiêu đề chính
                document.Add(new Paragraph("MẪU IN LỆNH SỬA CHỮA")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetBold()
                    .SetMarginTop(15));

                // 3. Thông tin chung
                document.Add(CreateInfoTable(font));

                // 4. Thông tin xe
                document.Add(CreateSectionTitle("1. THÔNG TIN XE", font));
                document.Add(CreateVehicleInfoTable(font));

                // 5. Thông tin khách hàng
                document.Add(CreateSectionTitle("2. THÔNG TIN KHÁCH HÀNG", font));
                document.Add(CreateCustomerInfoTable(font));

                // 6. Nhân công
                document.Add(CreateSectionTitle("3. NHÂN CÔNG", font));
                document.Add(CreateLaborTable(font));

                // 7. Ghi chú và chữ ký
                document.Add(new Paragraph("GHI CHÚ").SetBold().SetMarginTop(15));
                document.Add(new Paragraph("Vui lòng kiểm tra kỹ thông tin trước khi ký xác nhận"));
                document.Add(CreateSignatureSection(font));

                /* ===== HOÀN TẤT TÀI LIỆU ===== */

                // Đóng document trước khi đọc stream
                document.Close();

                // Reset vị trí stream
                stream.Seek(0, SeekOrigin.Begin);

                // Trả về file PDF
                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"Lenh_sua_chua_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                // Đảm bảo đóng tất cả tài nguyên nếu có lỗi
                document?.Close();
                pdf?.Close();
                writer?.Close();
                stream?.Dispose();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi tạo PDF",
                    error = ex.Message
                });
            }
        }

        private Table CreateInfoTable(PdfFont font)
        {
            var table = new Table(new float[] { 2, 3 }).UseAllAvailableWidth();

            AddTableCell(table, "Mã phiếu bảo hành:", "PRO526251 (3m/6h)", font);
            AddTableCell(table, "Ngày lập lệnh:", DateTime.Now.ToString("dd/MM/yyyy"), font);
            AddTableCell(table, "Ngày nhận xe:", DateTime.Now.ToString("dd/MM/yyyy"), font);

            return table.SetMarginTop(15);
        }

        private Table CreateVehicleInfoTable(PdfFont font)
        {
            var table = new Table(new float[] { 2, 3 }).UseAllAvailableWidth();

            AddTableCell(table, "Biển số xe:", "16K2-355.6688", font);
            AddTableCell(table, "Số VIN:", "2354255", font);
            AddTableCell(table, "Hãng xe:", "Toyota Camry", font);
            AddTableCell(table, "Loại xe:", "Sedan", font);
            AddTableCell(table, "Số km tiếp nhận:", "83.566 km", font);
            AddTableCell(table, "Ngày xuất xe dự kiến:", DateTime.Now.AddDays(3).ToString("dd/MM/yyyy"), font);
            AddTableCell(table, "Ghi chú:", "Xe có bảo hành chính hãng", font);

            return table.SetMarginTop(10);
        }

        private Table CreateCustomerInfoTable(PdfFont font)
        {
            var table = new Table(new float[] { 2, 3 }).UseAllAvailableWidth();

            AddTableCell(table, "Khách hàng:", "Nguyễn Ngọc Tú", font);
            AddTableCell(table, "Số điện thoại:", "034.458.3249", font);
            AddTableCell(table, "Email:", "ngocnguyen@gmail.com", font);
            AddTableCell(table, "Mã số thuế:", "0353259752", font);
            AddTableCell(table, "Địa chỉ:", "Ngõ 12, Láng Hạ, Thành Công, Ba Đình, Hà Nội", font);

            return table.SetMarginTop(10);
        }

        private Table CreateLaborTable(PdfFont font)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 5, 20, 10, 10, 5, 10, 15, 15, 10 }))
                .UseAllAvailableWidth()
                .SetMarginTop(10);

            // Header
            AddTableHeaderCell(table, "STT", font);
            AddTableHeaderCell(table, "Mô tả công việc", font);
            AddTableHeaderCell(table, "Mã công việc", font);
            AddTableHeaderCell(table, "Loại", font);
            AddTableHeaderCell(table, "SL", font);
            AddTableHeaderCell(table, "Đơn vị", font);
            AddTableHeaderCell(table, "Ghi chú", font);
            AddTableHeaderCell(table, "Kỹ thuật viên", font);
            AddTableHeaderCell(table, "Ngày hoàn thành", font);

            // Cấp 1: Công việc chính
            AddMainJobRow(table, "Q1", "VỆ SINH HỆ THỐNG ĐIỀU HÒA", font);

            // Cấp 2: Các công việc con (thụt lề)
            AddSubJobRow(table, "", "Vệ sinh dàn nóng", "VS-DN", "Vệ sinh", "1", "Cái", "Dùng dung dịch chuyên dụng", "Nguyễn Văn A", "25/06/2024", font);
            AddSubJobRow(table, "", "Vệ sinh dàn lạnh", "VS-DL", "Vệ sinh", "1", "Cái", "Vệ sinh cabin", "Trần Văn B", "25/06/2024", font);
            AddSubJobRow(table, "", "Thay lọc gió", "TLG", "Thay thế", "1", "Cái", "Lọc gió than hoạt tính", "Lê Thị C", "25/06/2024", font);

            // Cấp 1: Công việc chính khác
            AddMainJobRow(table, "Q2", "BẢO DƯỠNG ĐỘNG CƠ", font);

            // Cấp 2: Các công việc con
            AddSubJobRow(table, "", "Thay nhớt động cơ", "TN", "Bảo dưỡng", "1", "Lần", "Nhớt Total 5W30", "Phạm Văn D", "26/06/2024", font);
            AddSubJobRow(table, "", "Thay lọc nhớt", "TLN", "Thay thế", "1", "Cái", "Lọc nhớt chính hãng", "Nguyễn Thị E", "26/06/2024", font);

            return table;
        }

        private void AddMainJobRow(Table table, string stt, string jobName, PdfFont font)
        {
            // Tạo cell trải dài 9 cột
            Cell mainJobCell = new Cell(1, 9)
                .Add(new Paragraph(jobName).SetFont(font).SetBold())
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetPaddingLeft(10);

            table.AddCell(mainJobCell);
        }

        private void AddSubJobRow(Table table, string stt, string desc, string code, string type,
                                 string qty, string unit, string note, string tech, string date, PdfFont font)
        {
            // STT (để trống hoặc có thể thêm ký tự con)
            table.AddCell(new Cell().Add(new Paragraph(stt).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));

            // Mô tả công việc (thụt lề)
            table.AddCell(new Cell().Add(new Paragraph(desc).SetFont(font)).SetPaddingLeft(20));

            // Các cell còn lại
            table.AddCell(new Cell().Add(new Paragraph(code).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(type).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(qty).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(unit).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(note).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(tech).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(date).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
        }

        private void AddTableCell(Table table, string label, string value, PdfFont font)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(font).SetBold()));
            table.AddCell(new Cell().Add(new Paragraph(value).SetFont(font)));
        }

        private void AddTableHeaderCell(Table table, string text, PdfFont font)
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(text).SetFont(font).SetBold())
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetTextAlignment(TextAlignment.CENTER));
        }

        private void AddLaborDataRow(Table table, string stt, string desc, string code, string type,
                                   string qty, string unit, string note, string tech, string date, PdfFont font)
        {
            table.AddCell(new Cell().Add(new Paragraph(stt).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(desc).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(code).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(type).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(qty).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(new Cell().Add(new Paragraph(unit).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(note).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(tech).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(date).SetFont(font)).SetTextAlignment(TextAlignment.CENTER));
        }

        private Paragraph CreateSectionTitle(string title, PdfFont font)
        {
            return new Paragraph(title)
                .SetFont(font)
                .SetBold()
                .SetFontSize(14)
                .SetMarginBottom(5);
        }

        private LineSeparator CreateLineSeparator()
        {
            return new LineSeparator(new SolidLine(1f))
                .SetMarginTop(10)
                .SetMarginBottom(10);
        }

        private Table CreateSignatureSection(PdfFont font)
        {
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1 }))
                .UseAllAvailableWidth()
                .SetMarginTop(30);

            AddSignatureCell(table, "KHÁCH HÀNG", "(Ký và ghi rõ họ tên)", font);
            AddSignatureCell(table, "KỸ THUẬT VIÊN", "(Ký và ghi rõ họ tên)", font);
            AddSignatureCell(table, "QUẢN LÝ", "(Ký, đóng dấu)", font);

            return table;
        }

        private void AddSignatureCell(Table table, string title, string sub, PdfFont font)
        {
            table.AddCell(new Cell()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(40)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(title).SetFont(font).SetBold())
                .Add(new Paragraph(sub).SetFont(font).SetFontSize(10)));
        }

    }
}