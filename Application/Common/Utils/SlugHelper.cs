using System.Text;
using System.Text.RegularExpressions;

namespace Application.Common.Utils;

public static class SlugHelper
{
    public static string GenerateBase(string input)
    {
        var slug = input.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }

    public static string WithSuffix(string baseSlug, int attempt) =>
        attempt == 0 ? baseSlug : $"{baseSlug}-{attempt}";
}