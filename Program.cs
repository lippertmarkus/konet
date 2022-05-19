using System.CommandLine;
using System.Reflection;
using System.Runtime.InteropServices;
using konet.Commands;
using Microsoft.Extensions.Logging;

namespace konet;

internal class Program
{
    private static int Main(string[] args)
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        var logger = CreateLogger();

        return new RootCommand
        {
            new LoginCommand(logger),
            new BuildCommand(logger)
        }.Invoke(args);
    }

    private static ILogger<Program> CreateLogger()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddConsole();
        });
            
        return loggerFactory.CreateLogger<Program>();
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return NativeLibrary.Load($"{libraryName}.dll.so", assembly, searchPath);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return NativeLibrary.Load($"{libraryName}.dll", assembly, searchPath);

        return IntPtr.Zero; // default behaviour
    }
}