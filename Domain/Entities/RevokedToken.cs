namespace Domain.Entities;

public class RevokedToken
{
    public int Id { get; set; }
    public string Jti { get; set; } = null!;      
    public DateTime ExpiresAt { get; set; } 
}