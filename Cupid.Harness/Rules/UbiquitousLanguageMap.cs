using System.Text.Json;

namespace Cupid.Harness.Rules;

// Loads the ubiquitous-language-map.json vocabulary: which method-name prefixes belong to which
// bounded-context concern (Catalog, Pricing, Storage, ...). Used by HA6 to detect a class whose
// public API spans more concerns than a single-responsibility class should.
public sealed class UbiquitousLanguageMap
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _concerns;

    private UbiquitousLanguageMap(IReadOnlyDictionary<string, IReadOnlyList<string>> concerns)
    {
        _concerns = concerns;
    }

    public static UbiquitousLanguageMap Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);

        var concerns = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        foreach (var concern in doc.RootElement.GetProperty("concerns").EnumerateObject())
        {
            var keywords = concern.Value.EnumerateArray().Select(v => v.GetString()!).ToList();
            concerns[concern.Name] = keywords;
        }

        return new UbiquitousLanguageMap(concerns);
    }

    // Returns every concern whose vocabulary contains a keyword that the method name starts with
    // (case-insensitive). A method can legitimately match zero concerns (e.g. framework overrides,
    // generic helpers) - that's not itself a violation, only spanning 2+ concerns is.
    public IReadOnlyList<string> ConcernsFor(string methodName)
    {
        return _concerns
            .Where(kv => kv.Value.Any(keyword => methodName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)))
            .Select(kv => kv.Key)
            .ToList();
    }
}
