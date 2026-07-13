namespace Infrastructure.Helpers;

using System.Security.Cryptography;

public static class OtpGenerator
{
    public static string Generate()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();
    }
}