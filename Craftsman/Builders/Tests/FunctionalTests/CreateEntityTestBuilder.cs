﻿namespace Craftsman.Builders.Tests.FunctionalTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class CreateEntityTestBuilder
    {
        public static void CreateTests(string solutionDirectory, Entity entity, bool isProtected, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FunctionalTestClassPath(solutionDirectory, $"Create{entity.Name}Tests.cs", entity.Name, projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, entity, isProtected, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Entity entity, bool isProtected, string projectBaseName)
        {
            var testUtilClassPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var fakerClassPath = ClassPathHelper.TestFakesClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            var hasRestrictedEndpoints = isProtected;
            var authOnlyTests = hasRestrictedEndpoints ? $@"
            {CreateEntityTestUnauthorized(entity)}
            {CreateEntityTestForbidden(entity)}" : "";

            return @$"namespace {classPath.ClassNamespace};

using {fakerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using FluentAssertions;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)} : TestBase
{{
    {CreateEntityTest(entity, hasRestrictedEndpoints)}{authOnlyTests}
}}";
        }

        private static string CreateEntityTest(Entity entity, bool hasRestrictedEndpoints)
        {
            var fakeEntityForCreation = $"Fake{Utilities.GetDtoName(entity.Name, Dto.Creation)}";
            var fakeEntityVariableName = $"fake{entity.Name}";
            var pkName = Entity.PrimaryKeyProperty.Name;

            var testName = $"create_{entity.Name.ToLower()}_returns_created_using_valid_dto";
            testName += hasRestrictedEndpoints ? "_and_valid_auth_credentials" : "";
            var clientAuth = hasRestrictedEndpoints ? @$"

        _client.AddAuth(new[] {{Permissions.SuperAdmin}});" : "";

            return $@"[Test]
    public async Task {testName}()
    {{
        // Arrange
        var {fakeEntityVariableName} = new {fakeEntityForCreation} {{ }}.Generate();{clientAuth}

        // Act
        var route = ApiRoutes.{entity.Plural}.Create;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }}";
        }

        private static string CreateEntityTestUnauthorized(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"
    [Test]
    public async Task create_{entity.Name.ToLower()}_returns_unauthorized_without_valid_token()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Create;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }}";
        }

        private static string CreateEntityTestForbidden(Entity entity)
        {
            var fakeEntity = Utilities.FakerName(entity.Name);
            var fakeEntityVariableName = $"fake{entity.Name}";
            var fakeCreationDto = Utilities.FakerName(Utilities.GetDtoName(entity.Name, Dto.Creation));

            return $@"
    [Test]
    public async Task create_{entity.Name.ToLower()}_returns_forbidden_without_proper_scope()
    {{
        // Arrange
        var {fakeEntityVariableName} = {fakeEntity}.Generate(new {fakeCreationDto}().Generate());
        _client.AddAuth();

        await InsertAsync({fakeEntityVariableName});

        // Act
        var route = ApiRoutes.{entity.Plural}.Create;
        var result = await _client.PostJsonRequestAsync(route, {fakeEntityVariableName});

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }}";
        }
    }
}