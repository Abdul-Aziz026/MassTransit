using System.ComponentModel.DataAnnotations;

namespace MassTransitRequestResponseWebApi2.Models;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Id field shouldn't be empty")]
    public int Id { get; set; }

    public string CustomerName { get; set; } = "abc";
    public string CustomerEmail { get; set; } = "abc@gmail.com";
    public decimal TotalAmount { get; set; } = 100;
}

public class OrderResponse
{
    public int Id { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }

}
