using Domain.Entities.AddressEntity;
using Domain.Entities.PaymentEntity;
using Domain.Entities.UserEntity;
using Domain.Enums;

namespace Domain.Entities.OrderEntity;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public int UserId { get; set; }
    public User Buyer { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public int ShippingAddressId { get; set; }
    public Address ShippingAddress { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public Payment? Payment { get; set; }
    
    public int DeliveryCodeAttempts { get; set; } = 0;
    
    public int? CourierId { get; set; }
    public Courier? Courier { get; set; }

    public string DeliveryConfirmationCode { get; set; }

    public DateTime? CustomerConfirmedAt { get; set; }   

}