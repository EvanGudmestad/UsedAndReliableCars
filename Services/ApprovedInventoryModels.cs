using System.Text.Json;

namespace UsedAndReliableCars.Services;

/// <summary>
/// Curated make/model pairs aligned with the site’s “reliable used cars” list. Inventory search is limited to these.
/// </summary>
public static class ApprovedInventoryModels
{
    private static readonly HashSet<string> Keys = new(StringComparer.OrdinalIgnoreCase)
    {
        Key("Mazda", "Mazda6"),
        Key("Toyota", "Corolla"),
        Key("Chevrolet", "Equinox"),
        Key("Toyota", "Corolla Hybrid"),
        Key("Subaru", "Crosstrek"),
        Key("Toyota", "RAV4 Hybrid"),
        Key("Toyota", "Highlander"),
        Key("Lexus", "NX"),
        Key("Mazda", "MX-5 Miata"),
        Key("Honda", "Ridgeline")
    };

    private static string Key( string make, string model ) =>
        $"{make.Trim()}|{model.Trim()}";

    public static bool IsAllowed( string? make, string? model )
    {
        if (string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model))
            return false;
        return Keys.Contains(Key(make, model));
    }

    public static string JsonErrorNotApproved() =>
        JsonSerializer.Serialize(new
        {
            ok = false,
            error =
                "This assistant only searches the AutoGems curated reliable-used lineup. "
                + "Use one of the approved make/model pairs from the instructions (e.g. Toyota Corolla, Mazda Mazda6)."
        });
}
