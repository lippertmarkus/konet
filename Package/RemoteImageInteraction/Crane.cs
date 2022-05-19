using System.Runtime.InteropServices;
using System.Text;

namespace konet.Package.RemoteImageInteraction;

public static class Crane
{
    public static void Login(string registry, string user, string password)
        => ThrowOnErrorMessage(
            CraneInterop.Login(
                Encoding.UTF8.GetBytes(registry),
                Encoding.UTF8.GetBytes(user),
                Encoding.UTF8.GetBytes(password))
        );

    public static void Mutate(string platform, string archive, string tag, string baseImage, string entrypoint)
        => ThrowOnErrorMessage(
            CraneInterop.Mutate(
                Encoding.UTF8.GetBytes(platform),
                Encoding.UTF8.GetBytes(entrypoint),
                Encoding.UTF8.GetBytes(archive),
                Encoding.UTF8.GetBytes(baseImage),
                Encoding.UTF8.GetBytes(tag))
        );

    public static void CreateManifestList(string tag, string[] manifests)
        => ThrowOnErrorMessage(
            CraneInterop.CreateManifestList(
                Encoding.UTF8.GetBytes(tag),
                Encoding.UTF8.GetBytes(string.Join(',', manifests)))
        );

    private static void ThrowOnErrorMessage(IntPtr result)
    {
        var errorMessage = Marshal.PtrToStringAnsi(result) ?? "";

        if (!string.IsNullOrWhiteSpace(errorMessage))
            throw new CraneException(errorMessage);
    }
}