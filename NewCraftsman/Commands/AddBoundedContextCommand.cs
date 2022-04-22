namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Builders.ExtensionBuilders;
using Builders.Tests.Utilities;
using Domain;
using Domain.Enums;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class AddBoundedContextCommand : Command<AddBoundedContextCommand.Settings>
{
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IAnsiConsole _console;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

    public AddBoundedContextCommand(IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore,
        IAnsiConsole console)
    {
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _console = console;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")] public string Filepath { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var potentialSolutionDir = _utilities.GetRootDir();

        _utilities.IsSolutionDirectoryGuard(potentialSolutionDir);
        _scaffoldingDirectoryStore.SetSolutionDirectory(potentialSolutionDir);

        new FileParsingHelper(_fileSystem).RunInitialTemplateParsingGuards(settings.Filepath);
        var boundedContexts = FileParsingHelper.GetTemplateFromFile<BoundedContextsTemplate>(settings.Filepath);
        _consoleWriter.WriteHelpText($"Your template file was parsed successfully.");

        foreach (var template in boundedContexts.BoundedContexts)
            new ApiScaffoldingService(_console, _consoleWriter, _utilities, _scaffoldingDirectoryStore, _fileSystem)
                .ScaffoldApi(potentialSolutionDir, template);

        _consoleWriter.WriteHelpHeader(
            $"{Environment.NewLine}Your feature has been successfully added. Keep up the good work! {Emoji.Known.Sparkles}");
        return 0;
    }
}