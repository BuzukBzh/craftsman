namespace Craftsman.Builders;

using Craftsman.Helpers;
using Craftsman.Services;

public class OptionsConfigurationsBuilder(ICraftsmanUtilities utilities)
{
    public void CreateConfig(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, $"{FileNames.OptionsClassName(projectBaseName)}.cs", projectBaseName);
        var fileText = GetConfigText(classPath.ClassNamespace, projectBaseName);
        utilities.CreateFile(classPath, fileText);
    }

    private static string GetConfigText(string classNamespace, string projectBaseName)
    {
        var className = FileNames.OptionsClassName(projectBaseName);
        var cleanName = CraftsmanUtilities.GetCleanProjectName(projectBaseName);
        return 
        /* language=c# */
        $$"""
          namespace {{classNamespace}};
          
          public class {{className}}
          {
              public const string SectionName = "{{projectBaseName}}";
              
              public RabbitMqOptions RabbitMq { get; set; } = new RabbitMqOptions();
              public ConnectionStringOptions ConnectionStrings { get; set; } = new ConnectionStringOptions();
              public AuthOptions Auth { get; set; } = new AuthOptions();
              public string JaegerHost { get; set; } = String.Empty;
              
              public class RabbitMqOptions
              {
                  public const string SectionName = $"{{{className}}.SectionName}:RabbitMq";
                  public const string HostKey = nameof(Host);
                  public const string VirtualHostKey = nameof(VirtualHost);
                  public const string UsernameKey = nameof(Username);
                  public const string PasswordKey = nameof(Password);
                  public const string PortKey = nameof(Port);
          
                  public string Host { get; set; } = String.Empty;
                  public string VirtualHost { get; set; } = String.Empty;
                  public string Username { get; set; } = String.Empty;
                  public string Password { get; set; } = String.Empty;
                  public string Port { get; set; } = String.Empty;
              }
          
              public class ConnectionStringOptions
              {
                  public const string SectionName = $"{{{className}}.SectionName}:ConnectionStrings";
                  public const string {{cleanName}}Key = nameof({{cleanName}}); 
                      
                  public string {{cleanName}} { get; set; } = String.Empty;
              }
              
              
              public class AuthOptions
              {
                  public const string SectionName = $"{{{className}}.SectionName}:Auth";
          
                  public string Audience { get; set; } = String.Empty;
                  public string Authority { get; set; } = String.Empty;
                  public string AuthorizationUrl { get; set; } = String.Empty;
                  public string TokenUrl { get; set; } = String.Empty;
                  public string ClientId { get; set; } = String.Empty;
                  public string ClientSecret { get; set; } = String.Empty;
              }
          }
          
          public static class {{className}}Extensions
          {
              public static {{className}} Get{{className}}(this IConfiguration configuration)
              {
                  return configuration
                      .GetSection({{className}}.SectionName)
                      .Get<{{className}}>();
              }
              
              public static {{className}}.RabbitMqOptions GetRabbitMqOptions(this IConfiguration configuration)
              {
                  return configuration
                      .GetSection({{className}}.RabbitMqOptions.SectionName)
                      .Get<{{className}}.RabbitMqOptions>();
              }
              
              public static {{className}}.ConnectionStringOptions GetConnectionStringOptions(this IConfiguration configuration)
              {
                  return configuration
                      .GetSection({{className}}.ConnectionStringOptions.SectionName)
                      .Get<{{className}}.ConnectionStringOptions>();
              }
              
              public static {{className}}.AuthOptions GetAuthOptions(this IConfiguration configuration)
              {
                  return configuration
                      .GetSection({{className}}.AuthOptions.SectionName)
                      .Get<{{className}}.AuthOptions>();
              }
          
              public static string GetJaegerHostValue(this IConfiguration configuration)
              {
                  return configuration.GetSection(nameof({{className}}.JaegerHost)).Value;
              }
          }
          """;
    }
}
