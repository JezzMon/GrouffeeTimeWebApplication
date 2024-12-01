namespace GrouffeeTimeWebApplication.Models
{
    public class InvoiceModel
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public DateTime OrderDate { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public double TotalAmount { get; set; }
    }

    public class InvoiceItem
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice => Quantity * UnitPrice;
    }

}
