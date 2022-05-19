using System.Runtime.InteropServices;

namespace konet.Package.RemoteImageInteraction;

public static class CraneInterop
{
    [DllImport("crane")]
    public static extern IntPtr Mutate(byte[] platform, byte[] entrypoint, byte[] appendArchive, byte[] baseImage,
        byte[] tag);

    [DllImport("crane")]
    public static extern IntPtr Login(byte[] registry, byte[] user, byte[] password);

    [DllImport("crane")]
    public static extern IntPtr CreateManifestList(byte[] target, byte[] manifests);
}