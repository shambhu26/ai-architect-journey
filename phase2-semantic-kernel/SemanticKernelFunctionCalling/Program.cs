#pragma warning disable SKEXP0010

using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelFunctionCalling;

// Load API key + endpoint from User Secrets.
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string apiKey = config["AzureOpenAI:ApiKey"]!;
string endpoint = config["AzureOpenAI:Endpoint"]!;

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4.1-mini",   // matches the deployment name we just created
    endpoint: endpoint,
    apiKey: apiKey);


builder.Plugins.AddFromType<WeatherPlugin>();

Kernel kernel = builder.Build();

var chatService = kernel.GetRequiredService<IChatCompletionService>();

var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    MaxTokens = 500
};

#region for single message
//var response = await kernel.InvokePromptAsync(
//     "Delhi mein aaj kya pehnu?",
//     new(settings));

//Console.WriteLine(response);
#endregion

var history = new ChatHistory();
history.AddSystemMessage("You are a helpful assistant. Use the available functions when a question is about weather or clothing.");

Console.WriteLine("Chat started — type 'exit' to quit.\n");

while(true)
{
    Console.Write("User: ");
    var userIntpu = Console.ReadLine();

    if (string.IsNullOrEmpty(userIntpu) || userIntpu == "exit")
        break;


    history.AddUserMessage(userIntpu);

    var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);

    Console.WriteLine(response);

    history.AddMessage(response.Role, response.Content ?? string.Empty);
}

Console.WriteLine("Chat ended.");

