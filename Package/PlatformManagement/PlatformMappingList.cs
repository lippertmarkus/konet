using System.Collections;

namespace konet.Package.PlatformManagement;

public class PlatformMappingList : IEnumerable<PlatformMapping>
{
    private static readonly PlatformMappingList PlatformMappings = new()
    {
        {"windows/amd64:1809", "win-x64", "mcr.microsoft.com/windows/nanoserver:1809"},
        {"windows/amd64:1903", "win-x64", "mcr.microsoft.com/windows/nanoserver:1903"},
        {"windows/amd64:1909", "win-x64", "mcr.microsoft.com/windows/nanoserver:1909"},
        {"windows/amd64:2004", "win-x64", "mcr.microsoft.com/windows/nanoserver:2004"},
        {"windows/amd64:20H2", "win-x64", "mcr.microsoft.com/windows/nanoserver:20H2"},
        {"windows/amd64:ltsc2022", "win-x64", "mcr.microsoft.com/windows/nanoserver:ltsc2022"},
        {"linux/amd64", "linux-musl-x64", "mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine"},
        {"linux/arm/v7", "linux-arm", "mcr.microsoft.com/dotnet/runtime-deps:6.0"},
        {"linux/arm64/v8", "linux-arm64", "mcr.microsoft.com/dotnet/runtime-deps:6.0"},
    };

    private readonly Dictionary<Platform, PlatformMapping> _mappings = new();
    public Platform[] Platforms => _mappings.Keys.ToArray();
    public string[] DotNetRuntimeIdentifiers => _mappings.Values.Select(m => m.RuntimeIdentifier).Distinct().ToArray();
    
    private PlatformMappingList() { }

    public static PlatformMappingList DetermineTargetPlatformMappings(string[] platforms)
    {
        if (!platforms.Any())
            return PlatformMappings; // all available platforms

        var plats = new PlatformMappingList();
        foreach (var platform in platforms)
        {
            var (plat, platformMapping) = PlatformMappings._mappings.FirstOrDefault(pair =>
                string.CompareOrdinal(pair.Key.ToString(), platform) == 0);

            if (plat == null || platformMapping == null)
                throw new ArgumentException($"Platform {platform} is not valid");

            plats._mappings.Add(plat, platformMapping);
        }

        return plats;
    }
    
    private void Add(string platformString, string runtimeIdentifier, string baseImage)
    {
        var platform = new Platform(platformString);
        _mappings.Add(platform, new PlatformMapping(platform, runtimeIdentifier, baseImage));
    }

    public IEnumerator<PlatformMapping> GetEnumerator()
    {
        return _mappings.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}