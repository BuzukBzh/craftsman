﻿namespace Craftsman.Builders.Features;

using System.IO;
using System.IO.Abstractions;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class AddUserFeatureOverrideModifier
{
    private readonly IFileSystem _fileSystem;

    public AddUserFeatureOverrideModifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void UpdateAddUserFeature(string srcDirectory, string projectBaseName)
    {
        var entityName = "User";
        var entityPlural = "Users";
        var permissionName = FeatureType.AddRecord.DefaultPermission("Users", "AddUser");
        var classPath = ClassPathHelper.FeaturesClassPath(srcDirectory, $"{FileNames.AddEntityFeatureClassName(entityName)}.cs", entityPlural, projectBaseName);

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var className = FileNames.AddEntityFeatureClassName(entityName);
        var addCommandName = FileNames.CommandAddName();
        var readDto = FileNames.GetDtoName(entityName, Dto.Read);
        var createDto = FileNames.GetDtoName(entityName, Dto.Creation);

        var entityNameLowercase = entityName.LowercaseFirstLetter();
        var commandProp = $"{entityName}ToAdd";
        var newEntityProp = $"{entityNameLowercase}ToAdd";
        var repoInterface = FileNames.EntityRepositoryInterface(entityName);
        var repoInterfaceProp = $"{entityName.LowercaseFirstLetter()}Repository";
        var modelToCreateVariableName = $"{entityName.LowercaseFirstLetter()}ToAdd";

        var entityClassPath = ClassPathHelper.EntityClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var entityServicesClassPath = ClassPathHelper.EntityServicesClassPath(srcDirectory, "", entityPlural, projectBaseName);
        var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
        var exceptionsClassPath = ClassPathHelper.ExceptionsClassPath(srcDirectory, "", projectBaseName);
        var modelClassPath = ClassPathHelper.EntityModelClassPath(srcDirectory, entityName, entityPlural, null, projectBaseName);
        
        FeatureBuilderHelpers.GetPermissionValuesForHandlers(srcDirectory, 
            projectBaseName, 
            true, 
            permissionName, 
            out string heimGuardSetter, 
            out string heimGuardCtor, 
            out string _, 
            out string permissionsUsing,
            out string heimGuardField);

        var tempPath = $"{classPath.FullClassPath}temp";
        using var output = _fileSystem.File.CreateText(tempPath);
        {
            output.WriteLine($@"namespace {classPath.ClassNamespace};

using {entityServicesClassPath.ClassNamespace};
using {entityClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {modelClassPath.ClassNamespace};
using {servicesClassPath.ClassNamespace};
using {exceptionsClassPath.ClassNamespace};{permissionsUsing}
using Mappings;
using MediatR;

public static class {className}
{{
    public sealed record {addCommandName}({createDto} {commandProp}, bool SkipPermissions = false) : IRequest<{readDto}>;

    public sealed class Handler : IRequestHandler<{addCommandName}, {readDto}>
    {{
        private readonly {repoInterface} _{repoInterfaceProp};
        private readonly IUnitOfWork _unitOfWork;{heimGuardField}

        public Handler({repoInterface} {repoInterfaceProp}, IUnitOfWork unitOfWork{heimGuardCtor})
        {{
            _{repoInterfaceProp} = {repoInterfaceProp};
            _unitOfWork = unitOfWork;{heimGuardSetter}
        }}

        public async Task<{readDto}> Handle({addCommandName} request, CancellationToken cancellationToken)
        {{
            if(!request.SkipPermissions)
                await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.{permissionName});

            var {modelToCreateVariableName} = request.{commandProp}.To{EntityModel.Creation.GetClassName(entityName)}();
            var {entityNameLowercase} = {entityName}.Create({modelToCreateVariableName});
            await _{repoInterfaceProp}.Add({entityNameLowercase}, cancellationToken);

            await _unitOfWork.CommitChanges(cancellationToken);

            var {entityNameLowercase}Added = await _{repoInterfaceProp}.GetById({entityNameLowercase}.Id, cancellationToken: cancellationToken);
            return {entityNameLowercase}Added.To{readDto}();
        }}
    }}
}}");
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        output.Close();
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
