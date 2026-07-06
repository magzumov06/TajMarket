using System.Text.RegularExpressions;

namespace Infrastructure.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string phrase)
    {
        var str = phrase.ToLowerInvariant().Trim();

        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");   // хориҷ кардани аломатҳои иловагӣ
        str = Regex.Replace(str, @"\s+", " ").Trim();     // фазои холии зиёдатӣ
        str = Regex.Replace(str, @"\s", "-");             // фазо -> тире
        str = Regex.Replace(str, @"-+", "-");             // тиреи такрор

        if (string.IsNullOrWhiteSpace(str))
            str = "item";
        return str;
    }

    public static string WithUniqueSuffix(string baseSlug) =>
        $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";
}