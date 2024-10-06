﻿namespace Craftsman.Builders;

using System.IO.Abstractions;
using Helpers;
using MediatR;
using Services;

public static class CoreExceptionBuilder
{
    public class CoreExceptionBuilderCommand : IRequest<bool>
    {
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IFileSystem fileSystem,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<CoreExceptionBuilderCommand, bool>
    {
        public Task<bool> Handle(CoreExceptionBuilderCommand request, CancellationToken cancellationToken)
        {
            CreateExceptions();
            return Task.FromResult(true);
        }

        public void CreateExceptions()
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, "", scaffoldingDirectoryStore.ProjectBaseName);

            if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

            CreateNotFoundException();
            CreateValidationException();
            CreateForbiddenException();
            CreateNoRolesAssignedException();
        }

        public void CreateValidationException()
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"ValidationException.cs", 
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetValidationExceptionFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
        }

        public void CreateNotFoundException()
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"NotFoundException.cs", 
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetNotFoundExceptionFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
        }

        public void CreateForbiddenException()
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"ForbiddenException.cs", 
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetForbiddenExceptionFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
        }

        public void CreateNoRolesAssignedException()
        {
            var classPath = ClassPathHelper.ExceptionsClassPath(scaffoldingDirectoryStore.SrcDirectory, 
                $"NoRolesAssignedException.cs", 
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetNoRolesAssignedExceptionFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
        }

        public static string GetNotFoundExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

public class NotFoundException : Exception
{{
    public NotFoundException()
        : base()
    {{
    }}

    public NotFoundException(string message)
        : base(message)
    {{
    }}

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {{
    }}

    public NotFoundException(string name, object key)
        : base($""Entity \""{{name}}\"" ({{key}}) was not found."")
    {{
    }}
}}";
        }

        public static string GetForbiddenExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

public class ForbiddenAccessException : Exception
{{
    public ForbiddenAccessException() : base() {{ }}
}}";
        }

        public static string GetNoRolesAssignedExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

public class NoRolesAssignedException : Exception
{{
    public NoRolesAssignedException() : base() {{ }}
}}";
        }

        public static string GetValidationExceptionFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

public class ValidationException : Exception
{{
    public ValidationException()
        : base(""One or more validation failures have occurred."")
    {{
        Errors = new Dictionary<string, string[]>();
    }}

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {{
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }}

    public ValidationException(ValidationFailure failure)
        : this()
    {{
        Errors = new Dictionary<string, string[]>
        {{
            [failure.PropertyName] = new[] {{ failure.ErrorMessage }}
        }};
    }}

    public ValidationException(string errorPropertyName, string errorMessage)
        : base(errorMessage)
    {{
        Errors = new Dictionary<string, string[]>
        {{
            [errorPropertyName] = new[] {{ errorMessage }}
        }};
    }}

    public ValidationException(string errorMessage)
        : base(errorMessage)
    {{
        Errors = new Dictionary<string, string[]>
        {{
            [""Validation Exception""] = new[] {{ errorMessage }}
        }};
    }}

    public IDictionary<string, string[]> Errors {{ get; }}

    public static void ThrowWhenNullOrEmpty(string value, string message)
    {{
        if (string.IsNullOrEmpty(value))
            throw new ValidationException(message);
    }}
    public static void ThrowWhenNullOrWhitespace(string value, string message)
    {{
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(message);
    }}
    public static void ThrowWhenEmpty(Guid value, string message)
    {{
        if (value == Guid.Empty)
            throw new ValidationException(message);
    }}
    public static void ThrowWhenNullOrEmpty(Guid? value, string message)
    {{
        if (value == null || value == Guid.Empty)
            throw new ValidationException(message);
    }}
    public static void ThrowWhenNull(int? value, string message)
    {{
        if (value == null)
            throw new ValidationException(message);
    }}
    public static void ThrowWhenNull(object value, string message)
    {{
        if (value == null)
            throw new ValidationException(message);
    }}
    public static void Must(bool condition, string message)
    {{
        if(!condition)
            throw new ValidationException(message);
    }}
    public static void MustNot(bool condition, string message)
    {{
        if(condition)
            throw new ValidationException(message);
    }}
}}";
        }
    }
}