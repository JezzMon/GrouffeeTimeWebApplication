using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel.Tables;
using System.Globalization;
using GrouffeeTimeWebApplication.Data;

namespace GrouffeeTimeWebApplication.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(Order order);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public byte[] GenerateInvoicePdf(Order order)
        {
            var orderDetails = _context.OrderDetails
                .Include(od => od.Food)
                .Where(od => od.OrderId == order.Id)
                .ToList();

            Document document = CreateDocument(order, orderDetails);
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true)
            {
                Document = document
            };

            pdfRenderer.RenderDocument();
            using (MemoryStream stream = new MemoryStream())
            {
                pdfRenderer.Save(stream, false);
                return stream.ToArray();
            }
        }

        private Document CreateDocument(Order order, List<OrderDetail> orderDetails)
        {
            var philippineCulture = new CultureInfo("en-PH");

            Document document = new Document();
            Section section = document.AddSection();

            Paragraph companyHeader = section.AddParagraph("GrouffeeTea Cafe");
            companyHeader.Format.Font.Size = 16;
            companyHeader.Format.Font.Bold = true;
            companyHeader.Format.Alignment = ParagraphAlignment.Center;

            Paragraph title = section.AddParagraph($"Invoice #{order.Id}");
            title.Format.Font.Size = 14;
            title.Format.Alignment = ParagraphAlignment.Center;

            // Customer Details
            section.AddParagraph($"Name: {order.Name}");
            section.AddParagraph($"Email: {order.Email}");
            section.AddParagraph($"Mobile: {order.MobileNumber}");
            section.AddParagraph($"Address: {order.Address}");
            section.AddParagraph($"Payment Method: {order.PaymentMethod}");
            section.AddParagraph($"Order Date: {order.CreateDate}");

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("5cm");
            table.AddColumn("2cm");
            table.AddColumn("3cm");
            table.AddColumn("3cm");

            Row headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            headerRow.Cells[0].AddParagraph("Item");
            headerRow.Cells[1].AddParagraph("Qty");
            headerRow.Cells[2].AddParagraph("Unit Price");
            headerRow.Cells[3].AddParagraph("Total");

            double total = 0;
            foreach (var detail in orderDetails)
            {
                Row row = table.AddRow();
                row.Cells[0].AddParagraph(detail.Food.FoodName);
                row.Cells[1].AddParagraph(detail.Quantity.ToString());
                row.Cells[2].AddParagraph(detail.UnitPrice.ToString("C", philippineCulture));

                double itemTotal = detail.Quantity * detail.UnitPrice;
                row.Cells[3].AddParagraph(itemTotal.ToString("C", philippineCulture));
                total += itemTotal;
            }

            Row totalRow = table.AddRow();
            totalRow.Cells[2].AddParagraph("Total:");
            totalRow.Cells[3].AddParagraph(total.ToString("C", philippineCulture));

            return document;
        }
    }
}