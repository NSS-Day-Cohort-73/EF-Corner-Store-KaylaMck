namespace CornerStore.Models.DTO;

public class OrderDTO
{
    public int Id { get; set; }
    public CashierDTO Cashier { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public decimal Total { get; set; }
    public List<OrderProductDTO> OrderProducts { get; set; }
}