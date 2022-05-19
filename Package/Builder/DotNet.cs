using System.Diagnostics;

namespace konet.Package.Builder;

public static class DotNet
{
    public static async Task Build(string targetDirectory, string project, string runtimeIdentifier)
    {
        if (string.IsNullOrWhiteSpace(runtimeIdentifier))
            throw new ArgumentException("no runtime identifier provided");

        if (!Directory.Exists(project))
            throw new ArgumentException($"project directory {project} does not exist");
        
        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = project,
                FileName = "dotnet",
                ArgumentList =
                {
                    "publish", "-c", "release",
                    "-o", targetDirectory,
                    "-r", runtimeIdentifier, "--self-contained", "true", "/p:PublishTrimmed=true",
                    "/p:PublishReadyToRun=true", "/p:PublishSingleFile=true"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        p.Start();
        await p.WaitForExitAsync();
        
        if (p.ExitCode != 0)
        {
            var stdOut = await p.StandardOutput.ReadToEndAsync();
            var stdErr = await p.StandardError.ReadToEndAsync();
            
            throw new DotNetException($"DotNet build failed: \n{stdOut}\n\n{stdErr}");
        }
    }
    
    public static string DetermineBinaryName(string project)
    {
        var projectFile = Directory.EnumerateFiles(project, "*.csproj").FirstOrDefault();

        if (string.IsNullOrWhiteSpace(projectFile))
            throw new DotNetException($"No *.csproj file found in {project}");

        return Path.GetFileNameWithoutExtension(projectFile);
    }
}