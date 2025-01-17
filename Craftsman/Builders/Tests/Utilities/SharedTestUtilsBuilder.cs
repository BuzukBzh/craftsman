﻿namespace Craftsman.Builders.Tests.Utilities;

using Craftsman.Helpers;
using Craftsman.Services;
using MediatR;

public static class SharedTestUtilsBuilder
{
    public class Command : IRequest<bool>
    {
    }

    public class Handler(
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        : IRequestHandler<Command, bool>
    {
        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.SharedTestUtilitiesClassPath(scaffoldingDirectoryStore.TestDirectory,
                $"{FileNames.UnitTestUtilsName()}.cs", 
                scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace);
            utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }

        private string GetFileText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using System.Net;
using System.Net.Sockets;

public class DockerUtilities
{{
    public static int GetFreePort()
    {{
        // From https://stackoverflow.com/a/150974/4190785
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }}
}}
";
        }
    }
}