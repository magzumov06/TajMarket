namespace Domain.Entities.AddressEntity;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}