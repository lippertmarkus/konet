namespace konet.Package.PlatformManagement;

public class PlatformMapping
{
    public Platform Platform { get; }
    public string RuntimeIdentifier { get; }
    public string DefaultDotNetBaseImage { get; }

    public PlatformMapping(Platform platform, string runtimeIdentifier, string defaultDotNetBaseImage)
    {
        Platform = platform;
        RuntimeIdentifier = runtimeIdentifier;
        DefaultDotNetBaseImage = defaultDotNetBaseImage;
    }
}