using Domain.Enums;

namespace Domain.Entities.UserEntity;

public class Courier
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public CourierStatus Status { get; set; } = CourierStatus.Offline;
    public string VehicleType { get; set; } // нақлиёти курьерро нигоҳ медорад
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}