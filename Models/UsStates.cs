namespace UsedAndReliableCars.Models;

/// <summary>US state codes for location search dropdowns.</summary>
public static class UsStates
{
    /// <summary>Resolves a 2-letter code or full state name to a US state code, or null if unknown.</summary>
    public static string? ResolveCode( string? input )
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        var t = input.Trim();
        if (t.Length == 2)
            return t.ToUpperInvariant();
        foreach (var (code, name) in All)
        {
            if (name.Equals(t, StringComparison.OrdinalIgnoreCase))
                return code;
            if (code.Equals(t, StringComparison.OrdinalIgnoreCase))
                return code;
        }

        return null;
    }

    public static readonly IReadOnlyList<(string Code, string Name)> All =
        new List<(string Code, string Name)>
        {
            ("AL", "Alabama"), ("AK", "Alaska"), ("AZ", "Arizona"), ("AR", "Arkansas"), ("CA", "California"),
            ("CO", "Colorado"), ("CT", "Connecticut"), ("DE", "Delaware"), ("DC", "District of Columbia"),
            ("FL", "Florida"), ("GA", "Georgia"), ("HI", "Hawaii"), ("ID", "Idaho"), ("IL", "Illinois"),
            ("IN", "Indiana"), ("IA", "Iowa"), ("KS", "Kansas"), ("KY", "Kentucky"), ("LA", "Louisiana"),
            ("ME", "Maine"), ("MD", "Maryland"), ("MA", "Massachusetts"), ("MI", "Michigan"),
            ("MN", "Minnesota"), ("MS", "Mississippi"), ("MO", "Missouri"), ("MT", "Montana"),
            ("NE", "Nebraska"), ("NV", "Nevada"), ("NH", "New Hampshire"), ("NJ", "New Jersey"),
            ("NM", "New Mexico"), ("NY", "New York"), ("NC", "North Carolina"), ("ND", "North Dakota"),
            ("OH", "Ohio"), ("OK", "Oklahoma"), ("OR", "Oregon"), ("PA", "Pennsylvania"),
            ("RI", "Rhode Island"), ("SC", "South Carolina"), ("SD", "South Dakota"), ("TN", "Tennessee"),
            ("TX", "Texas"), ("UT", "Utah"), ("VT", "Vermont"), ("VA", "Virginia"), ("WA", "Washington"),
            ("WV", "West Virginia"), ("WI", "Wisconsin"), ("WY", "Wyoming")
        }.AsReadOnly();
}
