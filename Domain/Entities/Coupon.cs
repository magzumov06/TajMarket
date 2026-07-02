namespace Domain.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;
}