namespace Domain.DTOs.AddressDtos;

public record CreateAddressDto(
    string FullName,
    string PhoneNumber,
    string Country,
    string City,
    string Street,
    string? PostalCode,
    bool IsDefault
);