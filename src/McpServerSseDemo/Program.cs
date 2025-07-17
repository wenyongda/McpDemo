using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(3001);
});

IKernelBuilder kernelBuilder = Kernel.CreateBuilder(); ;
Kernel kernel = kernelBuilder.Build();

#pragma warning disable SKEXP0040

await kernel.ImportPluginFromOpenApiAsync(
   pluginName: "city_date_weather",
   uri: new Uri("http://localhost:5021/swagger/v1/swagger.json"),
   executionParameters: new OpenApiFunctionExecutionParameters
   {
       EnablePayloadNamespacing = true
   }
 );

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools(kernel.Plugins);
var app = builder.Build();

app.MapMcp();

app.Run();

public static class McpServerBuilderExtensions
{
    /// <summary>
    /// 把Plugins转换成McpServerTool并添加到McpServerBuilder
    /// </summary>
    public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, KernelPluginCollection plugins)
    {
        foreach (var plugin in plugins)
        {
            foreach (var function in plugin)
            {
#pragma warning disable SKEXP0001 
                builder.Services.AddSingleton(services => McpServerTool.Create(function.AsAIFunction()));
#pragma warning restore SKEXP0001
            }
        }

        return builder;
    }
}