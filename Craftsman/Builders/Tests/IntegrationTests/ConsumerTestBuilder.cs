﻿namespace Craftsman.Builders.Tests.IntegrationTests
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class ConsumerTestBuilder
    {
        public static void CreateTests(string testDirectory, Consumer consumer, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.FeatureTestClassPath(testDirectory, $"{consumer.ConsumerName}Tests.cs", "EventHandlers", projectBaseName);
            var fileText = WriteTestFileText(testDirectory, classPath, consumer, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, Consumer consumer, string projectBaseName)
        {
            var testFixtureName = Utilities.GetIntegrationTestFixtureName();
            var testUtilClassPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "");
            var consumerClassPath = ClassPathHelper.ConsumerFeaturesClassPath(solutionDirectory, "", consumer.DomainDirectory, projectBaseName);
            
            return @$"namespace {classPath.ClassNamespace};

using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using {consumerClassPath.ClassNamespace};
using {testUtilClassPath.ClassNamespace};
using static {testFixtureName};

public class {consumer.ConsumerName}Tests : TestBase
{{
    {ConsumerTest(consumer)}
}}";
        }

        private static string ConsumerTest(Consumer consumer)
        {
            var lowerConsumerName = consumer.ConsumerName.ToLower();
            var messageName = consumer.MessageName;

            return $@"[Test]
    public async Task {lowerConsumerName}_can_consume_{consumer.MessageName}_message()
    {{
        // Arrange
        var message = new Mock<{messageName}>();

        // Act
        await PublishMessage<{messageName}>(message);

        // Assert
        (await IsConsumed<{messageName}>()).Should().Be(true);
        (await IsConsumed<{messageName}, {consumer.ConsumerName}>()).Should().Be(true);
    }}";
        }
    }
}