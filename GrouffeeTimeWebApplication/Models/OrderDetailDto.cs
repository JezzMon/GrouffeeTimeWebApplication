namespace GrouffeeTimeWebApplication.Models;

public class OrderDetailDto
{
    public string DivId { get; set; }

    public IEnumerable<OrderDetail> OrderDetail { get; set; }
}
