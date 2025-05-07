using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using ModelContextProtocol.Server;

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();;
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

#pragma warning restore SKEXP0040

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools(kernel.Plugins);

await builder.Build().RunAsync();


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