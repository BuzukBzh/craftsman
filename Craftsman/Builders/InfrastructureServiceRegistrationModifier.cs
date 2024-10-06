﻿namespace Craftsman.Builders;

using System.IO;
using System.IO.Abstractions;
using Services;

public class InfrastructureServiceRegistrationModifier(IFileSystem fileSystem)
{
    public void InitializeAuthServices(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{FileNames.GetInfraRegistrationName()}.cs", projectBaseName);

        if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
            fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!fileSystem.File.Exists(classPath.FullClassPath))
            throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

        var authUsings = $@"
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HeimGuard;";
        var authServices = $@"
        var authOptions = configuration.GetAuthOptions();
        if (!env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {{
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {{
                    options.Authority = authOptions.Authority;
                    options.Audience = authOptions.Audience;
                    options.RequireHttpsMetadata = !env.IsDevelopment();
                }});
        }}

        services.AddAuthorization(options =>
        {{
        }});

        services.AddHeimGuard<UserPolicyHandler>()
            .MapAuthorizationPolicies()
            .AutomaticallyCheckPermissions();";

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = fileSystem.File.CreateText(tempPath);
            {
                string line;
                bool usingsAdded = false;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"using") && !usingsAdded)
                    {
                        newText += authUsings;
                        usingsAdded = true;
                    }
                    else if (line.Contains($"// Auth -- Do Not Delete"))
                    {
                        newText += authServices;
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        fileSystem.File.Delete(classPath.FullClassPath);
        fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
}

