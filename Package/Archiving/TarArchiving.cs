using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Tar;

namespace konet.Package.Archiving;

public static class TarArchiving
{
    public static async Task CreateTarArchive(string directory, string archivePath)
    {
        await using var outStream = File.Create(archivePath);
        using var tarArchive = TarArchive.CreateOutputTarArchive(outStream);
        
        // on linux systems tar entries have a path like a/b/c so we need to remove the root / of the source path
        tarArchive.RootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) 
            ? Path.GetRelativePath(Path.GetPathRoot(directory)!, directory) 
            : directory;

        AddDirectoryFilesToTar(tarArchive, directory);
    }

    private static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory)
    {
        // Recursively add sub-folders
        var directories = Directory.GetDirectories(sourceDirectory);
        foreach (var directory in directories)
            AddDirectoryFilesToTar(tarArchive, directory);

        // Add files
        var filenames = Directory.GetFiles(sourceDirectory);
        foreach (var filename in filenames)
        {
            var tarEntry = TarEntry.CreateEntryFromFile(filename);
            tarArchive.WriteEntry(tarEntry, true);
        }
    }
}