﻿namespace Craftsman.Builders.Tests.Utilities;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class ApiRouteModifier(IFileSystem fileSystem, IConsoleWriter consoleWriter)
{
    public void AddRoutes(string testDirectory, Entity entity, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
        {
            consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }

        var entityRouteClasses = CreateApiRouteClasses(entity);
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"new api route marker"))
                    {
                        newText += entityRouteClasses;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    public void AddRoutesForUser(string testDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.FunctionalTestUtilitiesClassPath(testDirectory, projectBaseName, "ApiRoutes.cs");

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var entityRouteClasses = CreateApiRouteClassesForUser();
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"new api route marker"))
                    {
                        newText += entityRouteClasses;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

    private static string CreateApiRouteClasses(Entity entity)
    {
        var entityRouteClasses = "";

        var lowercaseEntityPluralName = entity.Plural.LowercaseFirstLetter();
        var pkName = Entity.PrimaryKeyProperty.Name;

        entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}    public static class {entity.Plural}
    {{
        public static string GetList(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string GetAll(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/all"";
        public static string GetRecord(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Delete(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Put(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Create(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string CreateBatch(string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/batch"";
    }}";

        return entityRouteClasses;
    }

    private static string CreateApiRouteClassesForUser()
    {
        var entityRouteClasses = "";

        var lowercaseEntityPluralName = "users";
        var pkName = Entity.PrimaryKeyProperty.Name;

        entityRouteClasses += $@"{Environment.NewLine}{Environment.NewLine}    public static class Users
    {{
        public static string GetList(string version = ""v1"")  => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string GetRecord(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Delete(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Put(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}"";
        public static string Create(string version = ""v1"")  => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}"";
        public static string CreateBatch(string version = ""v1"")  => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/batch"";
        public static string AddRole(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}/addRole"";
        public static string RemoveRole(Guid id, string version = ""v1"") => $""{{Base}}/{{version}}/{lowercaseEntityPluralName}/{{id}}/removeRole"";
    }}";

        return entityRouteClasses;
    }
}
