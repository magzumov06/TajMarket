using System.Security.Cryptography;

namespace Application.Common.Utils;

public static class OtpGenerator
{
    public static string Generate()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();
    }
}