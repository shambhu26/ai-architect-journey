#pragma warning disable OPENAI001   // Responses API is still in preview/experimental in the SDK

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Responses;
using System.ClientModel;

// Load secrets (API key + endpoint) from User Secrets instead of hardcoding them.
// Keeps sensitive values out of source control.
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string apiKey = config["AzureOpenAI:ApiKey"]!;
string endpoint = config["AzureOpenAI:Endpoint"]!;

// ResponsesClientOptions tells the SDK which server to talk to —
// here, our Azure Foundry project endpoint (not the default public OpenAI endpoint).
var options = new ResponsesClientOptions
{
    Endpoint = new Uri(endpoint)
};

// Low-level client that talks directly to the Responses API.
ResponsesClient responsesClient = new(
    credential: new ApiKeyCredential(apiKey),
    options: options);

// Wrap the raw ResponsesClient into Microsoft.Extensions.AI's IChatClient abstraction.
// From here on, our code only depends on IChatClient — not on Responses-API-specific details.
IChatClient client = responsesClient.AsIChatClient("gpt-5-mini");

// Conversation history. Starts with just the system prompt (the assistant's persona/rules).
// Every user message and every AI reply gets appended here, because the API is stateless —
// the full history must be resent on every call for the model to have "memory."
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.System, "You are a concise technical assistant. Answer in 1-2 lines only."),
};

var option = new ChatOptions
{
    //Temperature = 0.2f,       // (disabled) would control randomness — low = more predictable/factual
    MaxOutputTokens = 500,      // kept high because gpt-5-mini spends part of this budget on internal reasoning
};

Console.WriteLine("Chat started — type 'exit' to quit.\\n\"");

// Main chat loop — keeps running until the user types "exit" or enters nothing.
while (true)
{
    Console.Write("User: ");

    var userInput = Console.ReadLine();

    // Exit condition: empty input or the word "exit" (case-insensitive).
    if (string.IsNullOrEmpty(userInput) || userInput?.Trim().ToLower() == "exit")
    {
        break;
    }

    // Add the user's message to history before sending — the model needs to see it.
    messages.Add(new ChatMessage(ChatRole.User, userInput));

    // Send the FULL conversation (system + all previous turns + new message) to the model.
    var response = await client.GetResponseAsync(messages, option);

    Console.WriteLine($"AI: {response.Text}\n");

    // Add the assistant's reply back into history too — otherwise the next turn
    // won't know what was said before, and multi-turn context breaks.
    messages.Add(new ChatMessage(ChatRole.Assistant, response.Text));
}

Console.WriteLine("Chat ended.");