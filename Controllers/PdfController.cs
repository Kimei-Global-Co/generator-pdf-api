using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using System.IO;
using iText.Layout.Borders;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;

namespace PdfGeneratorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        [HttpPost("create")]
        public IActionResult CreatePdf()
        {
            // Sử dụng MemoryStream để lưu trữ PDF trong bộ nhớ
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Tạo file PDF trong bộ nhớ
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Thiết lập font hỗ trợ tiếng Việt
                string fontPath = Path.Combine(AppContext.BaseDirectory, "assets", "NotoSans-Regular.ttf");
                PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, true);
                document.SetFont(font);

                // Phần 1: Logo và Thông Tin Công Ty
                Table headerTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));

                // Ô bên trái chứa logo
                Cell logoCell = new Cell().SetBorder(Border.NO_BORDER);
                Image logo = new Image(ImageDataFactory.Create("https://www.pngall.com/wp-content/uploads/13/Car-Logo-PNG.png")).ScaleToFit(200, 200);
                logoCell.Add(logo);
                headerTable.AddCell(logoCell);

                // Ô bên phải chứa thông tin công ty và QR Code
                Cell companyInfoCell = new Cell().SetBorder(Border.NO_BORDER);

                // Tạo bảng để chia bố cục thành hai cột: một cho văn bản và một cho QR code
                Table companyInfoTable = new Table(new float[] { 4, 1 }).SetWidth(UnitValue.CreatePercentValue(100));

                // Ô bên trái cho thông tin công ty
                Cell textInfoCell = new Cell().SetBorder(Border.NO_BORDER);
                textInfoCell.Add(new Paragraph("oliu@kimei.vn workshop default").SetFontSize(10).SetBold());
                textInfoCell.Add(new Paragraph("Thuộc chuỗi: workshop default\nĐịa chỉ: __\nSố điện thoại: 0868781111\nWebsite: __")
                    .SetFontSize(10));
                textInfoCell.Add(new Paragraph("Số tài khoản: 19036449572012\nNgân hàng: TCB\nChủ tài khoản: 19036449572012")
                    .SetFontSize(10));
                companyInfoTable.AddCell(textInfoCell);

                // Ô bên phải cho QR code
                Cell qrCodeCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE);
                qrCodeCell.Add(new Paragraph("QR Code").SetFontSize(10).SetFontColor(DeviceRgb.RED).SetTextAlignment(TextAlignment.CENTER));
                Image qrCode = new Image(ImageDataFactory.Create("https://png.pngtree.com/png-clipart/20200727/original/pngtree-modern-car-logo-concept-sports-car-speed-logo-icon-png-image_5177006.jpg")).ScaleToFit(50, 50);
                qrCodeCell.Add(qrCode);
                companyInfoTable.AddCell(qrCodeCell);

                // Thêm bảng thông tin công ty vào ô chính
                companyInfoCell.Add(companyInfoTable);
                headerTable.AddCell(companyInfoCell);

                // Thêm bảng header vào tài liệu
                document.Add(headerTable);


                // Phần 2: Tiêu Đề và Ngày Tháng
                document.Add(new Paragraph("Phiếu báo giá")
                    .SetFontSize(18)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(10)
                    .SetMarginBottom(10));

                Table dateTable = new Table(3).SetWidth(UnitValue.CreatePercentValue(100));

                // Ô đầu tiên (bên trái)
                dateTable.AddCell(new Cell().Add(new Paragraph("Mã phiếu tiếp nhận: PTN00000000002"))
                    .SetBorder(Border.NO_BORDER)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.LEFT));

                // Ô thứ hai (ở giữa)
                dateTable.AddCell(new Cell().Add(new Paragraph("Ngày nhận xe: 17/07/2024"))
                    .SetBorder(Border.NO_BORDER)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER));

                // Ô thứ ba (bên phải)
                dateTable.AddCell(new Cell().Add(new Paragraph("Ngày lập phiếu báo giá: 17/07/2024"))
                    .SetBorder(Border.NO_BORDER)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT));

                // Thêm bảng vào tài liệu
                document.Add(dateTable);


                // Phần 3: Thông Tin Xe và Khách Hàng
                Table infoTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(100));

                // Thông tin xe (bên trái)
                Cell carInfoCell = new Cell().SetBorder(Border.NO_BORDER);
                carInfoCell.Add(new Paragraph("1. Thông tin xe").SetBold());
                carInfoCell.Add(new Paragraph("Biển số xe: 30A-39969\nSố VIN: RLMGF4JX3EV005900\nHãng xe: TOYOTA\nLoại xe: Lexus\nSố km tiếp nhận: __\nNgày xuất xe dự kiến: 18/07/2024").SetFontSize(10));
                infoTable.AddCell(carInfoCell);

                // Thông tin khách hàng (bên phải)
                Cell customerInfoCell = new Cell().SetBorder(Border.NO_BORDER);
                customerInfoCell.Add(new Paragraph("2. Thông tin khách hàng").SetBold());
                customerInfoCell.Add(new Paragraph("Khách hàng: oanh tests\nSố điện thoại: 0868789122\nEmail: admin2@kimei.vn\nMã số thuế: __\nĐịa chỉ: dong me1234").SetFontSize(10));
                infoTable.AddCell(customerInfoCell);

                document.Add(infoTable);

                // Phần 4: Bảng Báo Giá (Nhân công và Sản phẩm/Phụ tùng)
                document.Add(new Paragraph("3. Báo giá").SetBold().SetFontSize(12).SetMarginTop(10));

                // Bảng nhân công
                document.Add(new Paragraph("Nhân công").SetBold().SetFontSize(10));
                Table laborTable = new Table(new float[] { 3, 1, 1, 2, 1, 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100));
                laborTable.AddHeaderCell("Tên nhân công").AddHeaderCell("Đơn vị").AddHeaderCell("Số lượng")
                    .AddHeaderCell("Thành tiền").AddHeaderCell("Chiết khấu").AddHeaderCell("Thuế").AddHeaderCell("Ghi chú");
                laborTable.AddCell("Thay dầu").AddCell("Công").AddCell("2").AddCell("200,000").AddCell("0").AddCell("0%").AddCell("");
                document.Add(laborTable);

                // Bảng sản phẩm/phụ tùng
                document.Add(new Paragraph("Sản phẩm/ phụ tùng").SetBold().SetFontSize(10));
                Table productTable = new Table(new float[] { 3, 1, 1, 2, 1, 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100));
                productTable.AddHeaderCell("Tên sản phẩm").AddHeaderCell("Đơn vị").AddHeaderCell("Số lượng")
                    .AddHeaderCell("Thành tiền").AddHeaderCell("Chiết khấu").AddHeaderCell("Thuế").AddHeaderCell("Ghi chú");
                productTable.AddCell("Dầu Liqui").AddCell("Lọ").AddCell("2").AddCell("200,000").AddCell("0").AddCell("0%").AddCell("");
                document.Add(productTable);

                // Bố cục cho phần cuối cùng
                Table footerTable = new Table(new float[] { 1, 1 }).SetWidth(UnitValue.CreatePercentValue(100));

                // Ô bên trái cho "Cố vấn dịch vụ"
                Cell leftCell = new Cell().SetBorder(Border.NO_BORDER);
                leftCell.Add(new Paragraph("Cố vấn dịch vụ").SetBold().SetFontSize(10).SetMarginBottom(15));
                footerTable.AddCell(leftCell);

                // Ô bên phải cho các chi tiết thanh toán
                Cell rightCell = new Cell().SetBorder(Border.NO_BORDER);

                // Thêm thông tin chi tiết thanh toán vào ô bên phải
                Table paymentTable = new Table(new float[] { 3, 2 }).SetWidth(UnitValue.CreatePercentValue(100));
                paymentTable.AddCell(new Cell().Add(new Paragraph("Số lượng").SetFontSize(10).SetBold()).SetBorder(Border.NO_BORDER));
                paymentTable.AddCell(new Cell().Add(new Paragraph("4").SetFontSize(10)).SetBorder(Border.NO_BORDER));

                paymentTable.AddCell(new Cell().Add(new Paragraph("Tổng cộng").SetFontSize(10).SetBold()).SetBorder(Border.NO_BORDER));
                paymentTable.AddCell(new Cell().Add(new Paragraph("400,000 VNĐ").SetFontSize(10)).SetBorder(Border.NO_BORDER));

                paymentTable.AddCell(new Cell().Add(new Paragraph("Chiết khấu").SetFontSize(10).SetBold()).SetBorder(Border.NO_BORDER));
                paymentTable.AddCell(new Cell().Add(new Paragraph("0 VNĐ").SetFontSize(10)).SetBorder(Border.NO_BORDER));

                paymentTable.AddCell(new Cell().Add(new Paragraph("Thuế VAT").SetFontSize(10).SetBold()).SetBorder(Border.NO_BORDER));
                paymentTable.AddCell(new Cell().Add(new Paragraph("0 VNĐ").SetFontSize(10)).SetBorder(Border.NO_BORDER));

                paymentTable.AddCell(new Cell().Add(new Paragraph("Tổng thanh toán").SetFontSize(10).SetBold()).SetBorder(Border.NO_BORDER));
                paymentTable.AddCell(new Cell().Add(new Paragraph("400,000 VNĐ").SetFontSize(10).SetFontColor(DeviceRgb.BLUE).SetBold()).SetBorder(Border.NO_BORDER));

                // Thêm bảng chi tiết thanh toán vào ô bên phải
                rightCell.Add(paymentTable);
                footerTable.AddCell(rightCell);

                // Thêm một hàng mới với "Ghi chú" và "Xác nhận của khách hàng" dàn đều hai bên
                Cell noteCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT);
                noteCell.Add(new Paragraph("Ghi chú").SetFontSize(10));
                footerTable.AddCell(noteCell);

                Cell customerConfirmationCell = new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);
                customerConfirmationCell.Add(new Paragraph("Xác nhận của khách hàng").SetFontSize(10).SetBold());
                footerTable.AddCell(customerConfirmationCell);

                // Thêm footer vào tài liệu
                document.Add(footerTable);


                // Đóng tài liệu
                document.Close();

                // Trả về PDF
                byte[] pdfBytes = memoryStream.ToArray();
                return File(pdfBytes, "application/pdf", "quotation.pdf");
            }
        }
    }
}
