using System.Collections.Concurrent;
using System.CommandLine;
using konet.Package.Archiving;
using konet.Package.Builder;
using konet.Package.PlatformManagement;
using konet.Package.RemoteImageInteraction;
using Microsoft.Extensions.Logging;

namespace konet.Commands;

internal class BuildCommand : Command
{
    private readonly ILogger _logger;
    private readonly string _tempDirectory;

    public BuildCommand(ILogger logger) : base("build",
        "Builds the provided project into a binary, containerizes it, and publishes it.")
    {
        _logger = logger;
        _tempDirectory = GetTempDirectory();

        var project = new Argument<string?>("path", "Project directory with the *.csproj")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        var platforms = new Option<string[]>("--platform", "With platforms to include in the multiarch image");
        var tag = new Option<string>( new[] {"--tag", "-t"}, "Tag") {IsRequired = true};

        Add(project);
        Add(platforms);
        Add(tag);

        this.SetHandler<string, string[], string>(Build, project, platforms, tag);
    }

    private async Task Build(string project, string[] platforms, string tag)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(project))
                project = Directory.GetCurrentDirectory();

            var binaryName = DotNet.DetermineBinaryName(project);
            var targetPlatforms = PlatformMappingList.DetermineTargetPlatformMappings(platforms);

            // build binaries and tar them
            foreach (var runtimeIdentifier in targetPlatforms.DotNetRuntimeIdentifiers)
            {
                _logger.LogInformation("Building project for {runtime}", runtimeIdentifier);
                await DotNetBuildAndTar(project, runtimeIdentifier);
            }

            // push tar archives as new layers and create manifests referencing them
            _logger.LogInformation("Creating and pushing images for platforms {platforms}",
                string.Join(", ", targetPlatforms.Platforms.Select(p => p.ToString())));
            var pushedManifests = new ConcurrentBag<string>();
            await Parallel.ForEachAsync(targetPlatforms, (targetPlatform, _) =>
            {
                pushedManifests.Add(CreateAndPushImage(tag, targetPlatform, binaryName));
                return ValueTask.CompletedTask;
            });

            // create manifest list referencing all manifests created before
            _logger.LogInformation("Creating manifest list {tag}", tag);
            CreateManifestList(tag, pushedManifests.ToArray());

            Directory.Delete(_tempDirectory, true);
            _logger.LogInformation("Successfully pushed to {tag}", tag);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "failed to build project and push images for project {project}", project);
        }
    }

    private static void CreateManifestList(string tag, string[] pushedManifests)
    {
        try
        {
            Crane.CreateManifestList(tag, pushedManifests);
        }
        catch (CraneException e)
        {
            throw new Exception($"couldn't create manifest list {tag}", e);
        }
    }

    private string CreateAndPushImage(string tag, PlatformMapping targetPlatform, string binaryName)
    {
        var archive = GetArchivePath(targetPlatform.RuntimeIdentifier);
        var platform = targetPlatform.Platform.ToString(true);
        var targetTag = $"{tag}-{targetPlatform.Platform.GetTagSuffix()}";
        var entryPoint = GetEntryPoint(targetPlatform.RuntimeIdentifier, binaryName);

        try
        {
            Crane.Mutate(platform, archive, targetTag, targetPlatform.DefaultDotNetBaseImage,
                entryPoint);
        }
        catch (CraneException e)
        {
            throw new Exception(
                $"failed to create image {targetTag} based on archive {archive} for platform {platform}", e);
        }

        return targetTag;
    }

    private async Task DotNetBuildAndTar(string project, string runtimeIdentifier)
    {
        var targetDirectory = Path.Join(_tempDirectory, $"out-{runtimeIdentifier}");

        var buildDirectory = GetBuildDirectory(targetDirectory, runtimeIdentifier);
        await DotNet.Build(buildDirectory, project, runtimeIdentifier);

        await TarArchiving.CreateTarArchive(targetDirectory, GetArchivePath(runtimeIdentifier));
    }

    private static string GetBuildDirectory(string targetDirectory, string runtimeIdentifier)
        // linux container images can't have the binary in root for some archs, therefore we put it in an "app" directory
        => runtimeIdentifier.Contains("linux", StringComparison.InvariantCultureIgnoreCase)
            ? Path.Join(targetDirectory, "app")
            : targetDirectory;
    
    private static string GetEntryPoint(string runtimeIdentifier, string appName)
    {
        var path = $"{GetBuildDirectory("", runtimeIdentifier)}/{appName}".Trim('/');

        return runtimeIdentifier.Contains("linux", StringComparison.InvariantCultureIgnoreCase)
            ? $"./{path}"
            : $"{path}.exe";
    }

    private string GetArchivePath(string runtimeIdentifier)
        => $"{Path.Join(_tempDirectory, runtimeIdentifier)}.tar";

    private static string GetTempDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);

        return tempDirectory;
    }
}