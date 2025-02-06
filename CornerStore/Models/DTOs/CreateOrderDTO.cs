namespace CornerStore.Models.DTO;

public class CreateOrderDTO
{
    public int CashierId { get; set; }
    public List<CreateOrderProductDTO> OrderProducts { get; set; }
}