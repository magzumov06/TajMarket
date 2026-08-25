namespace Application.Common.Utils;

internal static class DeliveryCodeHelper
{
    public static string GenerateCode() =>
        Random.Shared.Next(100000, 999999).ToString();
}