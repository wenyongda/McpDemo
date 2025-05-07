using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

await using IMcpClient mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
{
    Name = "city_date_weather",
    Command = "..\\..\\..\\..\\McpServerDemo\\bin\\Debug\\net9.0\\McpServerDemo.exe"
}));

var tools = await mcpClient.ListToolsAsync();

foreach (AIFunction tool in tools)
{
    Console.WriteLine($"Tool Name: {tool.Name}");
    Console.WriteLine($"Tool Description: {tool.Description}");
    Console.WriteLine();
}

string apiKey = "sk-***";
var chatClient = new ChatClient("qwen-max-2025-01-25", new ApiKeyCredential(apiKey), new OpenAIClientOptions
{
    Endpoint = new Uri("https://dashscope.aliyuncs.com/compatible-mode/v1")
}).AsIChatClient();

IChatClient client = new ChatClientBuilder(chatClient)
    .UseFunctionInvocation()
    .Build();

ChatOptions chatOptions = new()
{
    Tools = [.. tools],
};

List<Microsoft.Extensions.AI.ChatMessage> chatList = [];

string question = "";
do
{
    Console.Write($"User:");
    question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question) || question == "exists")
    {
        break;
    }

    chatList.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, question));

    Console.Write($"Assistant:");
    StringBuilder sb = new StringBuilder();
    await foreach (var update in client.GetStreamingResponseAsync(chatList, chatOptions))
    {
        if (string.IsNullOrWhiteSpace(update.Text))
        {
            continue;
        }
        sb.Append(update.Text);

        Console.Write(update.Text);
    }

    chatList.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, sb.ToString()));

    Console.WriteLine();

} while (true);

Console.ReadLine();



//using HttpClientHandler handler = new HttpClientHandler
//{
//    ClientCertificateOptions = ClientCertificateOption.Automatic
//};

//using HttpClient httpClient = new(handler)
//{
//    BaseAddress = new Uri("https://dashscope.aliyuncs.com/compatible-mode/v1")
//};

//#pragma warning disable SKEXP0070
//IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
//kernelBuilder.AddOpenAIChatCompletion("qwen-max-2025-01-25", "sk-***", httpClient: httpClient);
//kernelBuilder.Plugins.AddFromFunctions("weather", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

//Kernel kernel = kernelBuilder.Build();
//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//PromptExecutionSettings promptExecutionSettings = new()
//{
//    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
//};

//var history = new ChatHistory();

//while (true)
//{
//    Console.Write($"User:");
//    string input = Console.ReadLine();

//    if (string.IsNullOrWhiteSpace(input) || input == "exists")
//    {
//        break;
//    }

//    history.AddUserMessage(input);
//    var chatMessage = await chatCompletionService.GetChatMessageContentAsync(
//    history,
//    executionSettings: promptExecutionSettings,
//    kernel: kernel);

//    Console.WriteLine("Assistant:" + chatMessage.Content);

//    history.AddAssistantMessage(chatMessage.Content);
//}

//Console.ReadLine();