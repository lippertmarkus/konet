namespace konet.Package.PlatformManagement;

public class Platform
{
    private string? Os { get; }
    private string? Arch { get; }
    private string? Variant { get; }
    private string? Version { get; }

    public override string ToString() => ToString(false);

    public string ToString(bool skipVersion) => $"{Os}" +
                                                $"{(string.IsNullOrWhiteSpace(Arch) ? "" : "/" + Arch)}" +
                                                $"{(string.IsNullOrWhiteSpace(Variant) ? "" : "/" + Variant)}" +
                                                $"{(string.IsNullOrWhiteSpace(Version) || skipVersion ? "" : ":" + Version)}";

    public Platform(string input)
    {
        var inp = input.Split(":", StringSplitOptions.RemoveEmptyEntries);
        if (inp.Length > 1)
        {
            Version = inp.Last();
            input = inp.First();
        }

        string?[] parts = input.Split("/", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
            Os = parts[0];
        if (parts.Length > 1)
            Arch = parts[1];
        if (parts.Length > 2)
            Variant = parts[2];
    }

    public string GetTagSuffix() => ToString().Replace("/", "-").Replace(":", "-");
}