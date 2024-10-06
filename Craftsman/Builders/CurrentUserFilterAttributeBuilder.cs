namespace Craftsman.Builders;

using Helpers;
using MediatR;
using Services;

public static class CurrentUserFilterAttributeBuilder
{
    public record Command() : IRequest;

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.HangfireResourcesClassPath(scaffoldingDirectoryStore.SrcDirectory,
                $"CurrentUserFilterAttribute.cs",
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private static string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using Hangfire.Client;
using Hangfire.Common;

public class CurrentUserFilterAttribute : JobFilterAttribute, IClientFilter
{{
    public void OnCreating(CreatingContext context)
    {{
        var argue = context.Job.Args.FirstOrDefault(x => x is IJobWithUserContext);
        if (argue == null)
            throw new Exception($""This job does not implement the {{nameof(IJobWithUserContext)}} interface"");

        var jobParameters = argue as IJobWithUserContext;
        var user = jobParameters?.User;

        if(user == null)
            throw new Exception($""A User could not be established"");

        context.SetJobParameter(""User"", user);
    }}

    public void OnCreated(CreatedContext context)
    {{
    }}
}}";
        }
    }
}