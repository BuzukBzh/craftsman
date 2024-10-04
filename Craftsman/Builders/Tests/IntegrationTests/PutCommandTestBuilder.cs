﻿namespace Craftsman.Builders.Tests.IntegrationTests;

using Craftsman.Services;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class PutCommandTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PutCommandTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, string srcDirectory, Entity entity,
        string projectBaseName, bool featureIsProtected, string permission)
    {
        var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"Update{entity.Name}CommandTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, srcDirectory, classPath, entity, projectBaseName, featureIsProtected, permission);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, string srcDirectory,
        ClassPath classPath, Entity entity, string projectBaseName, bool featureIsProtected, string permission)
    {
        var featureName = FileNames.UpdateEntityFeatureClassName(entity.Name);
        var commandName = FileNames.CommandUpdateName();
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";
        var lowercaseEntityName = entity.Name.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var dtoClassPath = ClassPathHelper.DtoClassPath(srcDirectory, "", entity.Plural, projectBaseName);
        var featuresClassPath = ClassPathHelper.FeaturesClassPath(srcDirectory, featureName, entity.Plural, projectBaseName);

        var permissionTest = !featureIsProtected ? null : GetPermissionTest(commandName, entity, featureName, permission);

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {dtoClassPath.ClassNamespace};
using {featuresClassPath.ClassNamespace};
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class {classPath.ClassNameWithoutExt} : TestBase
{{
    [Fact]
    public async Task can_update_existing_{entity.Name.ToLowerInvariant()}_in_db()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        var {lowercaseEntityName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        await testingServiceScope.InsertAsync({lowercaseEntityName});
        var updated{entity.Name}Dto = new {fakeUpdateDto}().Generate();

        // Act
        var command = new {featureName}.{commandName}({lowercaseEntityName}.{pkName}, updated{entity.Name}Dto);
        await testingServiceScope.SendAsync(command);
        var updated{entity.Name} = await testingServiceScope
            .ExecuteDbContextAsync(db => db.{entity.Plural}
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{pkName} == {lowercaseEntityName}.{pkName}));

        // Assert{GetAssertions(entity.Properties, entity.Name)}
    }}{permissionTest}
}}";
    }

    private static string GetPermissionTest(string commandName, Entity entity, string featureName, string permission)
    {
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}One";

        return $@"

    [Fact]
    public async Task must_be_permitted()
    {{
        // Arrange
        var testingServiceScope = new {FileNames.TestingServiceScope()}();
        testingServiceScope.SetUserNotPermitted(Permissions.{permission});
        var {fakeEntityVariableName} = new {fakeUpdateDto}();

        // Act
        var command = new {featureName}.{commandName}(Guid.NewGuid(), {fakeEntityVariableName});
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }}";
    }

    private static string GetAssertions(List<EntityProperty> properties, string entityName)
    {
        var entityAssertions = "";
        foreach (var entityProperty in properties.Where(x => x.IsPrimitiveType && x.GetDbRelationship.IsNone && x.CanManipulate))
        {
            entityAssertions += entityProperty.Type switch
            {
                "DateTime" or "DateTimeOffset" or "TimeOnly" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo(updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "DateTime?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo((DateTime)updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "DateTimeOffset?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo((DateTimeOffset)updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "TimeOnly?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeCloseTo((TimeOnly)updated{entityName}Dto.{entityProperty.Name}, 1.Seconds());",
                "decimal" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeApproximately(updated{entityName}Dto.{entityProperty.Name}, 0.001M);",
                "decimal?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeApproximately((decimal)updated{entityName}Dto.{entityProperty.Name}, 0.001M);",
                "float" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeApproximately(updated{entityName}Dto.{entityProperty.Name}, 0.001F);",
                "float?" =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeApproximately((float)updated{entityName}Dto.{entityProperty.Name}, 0.001F);",
                _ =>
                    $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().Be(updated{entityName}Dto.{entityProperty.Name});"
            };
        }
        foreach (var entityProperty in properties.Where(x => x.IsStringArray && x.CanManipulate))
        {
            entityAssertions += $@"{Environment.NewLine}        updated{entityName}.{entityProperty.Name}.Should().BeEquivalentTo(updated{entityName}Dto.{entityProperty.Name});";
        }
        foreach (var entityProperty in properties.Where(x => x.IsValueObject && x.CanManipulate))
        {
            entityAssertions += entityProperty.ValueObjectType.GetIntegratedUpdatedRecordAssertion($"updated{entityName}", entityProperty.Name, $"updated{entityName}Dto", entityProperty.Type);
        }

        return entityAssertions;
    }
}
