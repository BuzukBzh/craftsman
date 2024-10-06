﻿namespace Craftsman.Builders;

using System.IO.Abstractions;
using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class EntityMappingModifier(IFileSystem fileSystem, IConsoleWriter consoleWriter)
{
    public void UpdateMappingAttributesForValueObject(string srcDirectory, string entityName, string entityPlural, EntityProperty entityProperty, string projectBaseName)
    {
        var classPath = ClassPathHelper.EntityMappingClassPath(srcDirectory,
            $"{FileNames.GetMappingName(entityName)}.cs",
            entityPlural,
            projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
        {
            consoleWriter.WriteInfo($"The `{classPath.FullClassPath}` file could not be found.");
            return;
        }
        
        var toReadDtoMapperMethodRoot = EntityMappingBuilder.ToReadDtoMapperMethodRoot(entityName);
        var toQueryableMapperMethodRoot = EntityMappingBuilder.ToQueryableMapperMethodRoot(entityName);
        var mapperAttribute = entityProperty.ValueObjectType.GetMapperAttribute(entityName, entityProperty.Name);
        
        var tempPath = $"{classPath.FullClassPath}temp";
        var containLinesFound = 0;
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = string.Empty;
                    if (line.Contains(toReadDtoMapperMethodRoot) || line.Contains(toQueryableMapperMethodRoot))
                    {
                        newText += $@"{Environment.NewLine}{mapperAttribute}";
                        containLinesFound++;
                    }
                    newText += $"{line}";

                    output.WriteLine(newText);
                }
            }
        }
        
        if (containLinesFound == 0)
        {
            consoleWriter.WriteInfo($"It looks like you might have a custom mapper file for {entityName} and may need to add mappings for your value object(s) manually.");
            return;
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}
