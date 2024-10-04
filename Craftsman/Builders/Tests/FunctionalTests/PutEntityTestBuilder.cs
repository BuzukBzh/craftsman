﻿namespace Craftsman.Builders.Tests.FunctionalTests;

using System;
using System.IO;
using Domain;
using Domain.Enums;
using Helpers;
using IntegrationTests.Services;
using Services;

public class PutEntityTestBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public PutEntityTestBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateTests(string solutionDirectory, string testDirectory, Entity entity, bool isProtected, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestClassPath(testDirectory, $"Update{entity.Name}RecordTests.cs", entity.Plural, projectBaseName);
        var fileText = WriteTestFileText(solutionDirectory, testDirectory, classPath, entity, isProtected, projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string WriteTestFileText(string solutionDirectory, string testDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
    {
        var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "");
        var fakerClassPath = ClassPathHelper.TestFakesClassPath(testDirectory, "", entity.Name, projectBaseName);
        var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(testDirectory, "", projectBaseName);

        var permissionsUsing = isProtected
            ? $"{Environment.NewLine}using {permissionsClassPath.ClassNamespace};"
            : string.Empty;

        var authOnlyTests = isProtected ? $@"
            {EntityTestUnauthorized(entity)}
            {EntityTestForbidden(entity)}" : "";

        return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};{permissionsUsing}
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {PutEntityTest(entity, isProtected)}{authOnlyTests}
}}";
    }

    private static string PutEntityTest(Entity entity, bool isProtected)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}";
        var fakeDtoVariableName = $"updated{entity.Name}Dto";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        var testName = $"put_{entity.Name.ToLowerInvariant()}_returns_nocontent_when_entity_exists";
        testName += isProtected ? "_and_auth_credentials_are_valid" : "";
        var clientAuth = isProtected ? @$"

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);" : "";

        return $@"[Fact]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var updated{entity.Name}Dto = new {fakeUpdateDto}().Generate();{clientAuth}
        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Put({fakeEntityVariableName}.{pkName});
        var result = await FactoryClient.PutJsonRequestAsync(route, {fakeDtoVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }}";
    }

    private static string EntityTestUnauthorized(Entity entity)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}";
        var fakeDtoVariableName = $"updated{entity.Name}Dto";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        return $@"
    [Fact]
    public async Task put_{entity.Name.ToLowerInvariant()}_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var {fakeDtoVariableName} = new {fakeUpdateDto} {{ }}.Generate();

        // Act
        var route = ApiRoutes.{entity.Plural}.Put({fakeEntityVariableName}.{pkName});
        var result = await FactoryClient.PutJsonRequestAsync(route, {fakeDtoVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
    }

    private static string EntityTestForbidden(Entity entity)
    {
        var fakeEntity = FileNames.FakerName(entity.Name);
        var fakeUpdateDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Update));
        var fakeEntityVariableName = $"{entity.Name.LowercaseFirstLetter()}";
        var fakeDtoVariableName = $"updated{entity.Name}Dto";
        var pkName = Entity.PrimaryKeyProperty.Name;
        var fakeCreationDto = FileNames.FakerName(FileNames.GetDtoName(entity.Name, Dto.Creation));

        return $@"
    [Fact]
    public async Task put_{entity.Name.ToLowerInvariant()}_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {FileNames.FakeBuilderName(entity.Name)}().Build();
        var {fakeDtoVariableName} = new {fakeUpdateDto} {{ }}.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.{entity.Plural}.Put({fakeEntityVariableName}.{pkName});
        var result = await FactoryClient.PutJsonRequestAsync(route, {fakeDtoVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
    }
}
