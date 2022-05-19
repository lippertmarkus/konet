using System.CommandLine;
using konet.Package.RemoteImageInteraction;
using Microsoft.Extensions.Logging;

namespace konet.Commands;

internal class LoginCommand : Command
{
    private readonly ILogger _logger;

    public LoginCommand(ILogger logger) : base("login", "Log in to a registry")
    {
        _logger = logger;

        var registryArg = new Argument<string>("registry", "Registry server");
        var userParam = new Option<string>(new[] {"--username", "-u"}, "Username") {IsRequired = true};
        var passwordParam = new Option<string>(new[] {"--password", "-p"}, "Password") {IsRequired = true};

        Add(registryArg);
        Add(userParam);
        Add(passwordParam);

        this.SetHandler<string, string, string>(Login, registryArg, userParam, passwordParam);
    }

    private void Login(string registry, string user, string password)
    {
        try
        {
            Crane.Login(registry, user, password);
            _logger.LogInformation("Credentials for {registry} successfully stored", registry);
        }
        catch (CraneException e)
        {
            _logger.LogError(e, "Failed to store credentials for {registry}", registry);
        }
    }
}