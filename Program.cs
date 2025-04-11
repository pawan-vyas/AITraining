using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// This example demonstrates how to use the OpenAI Chat Completion service with Semantic Kernel in C#.

// Model Name to use for the OpenAI Chat Completion
var modelId = "gpt-4o-mini";

// API key for OpenAI API Access.
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
             ?? throw new InvalidOperationException("Please set the OpenAI API key in the environment variable.");

// Create Semantic Kernel Builder with OpenAI Service.
var kernelBuilder = Kernel.CreateBuilder()
                          .AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);

// Build the Kernel instance.
var kernel = kernelBuilder.Build();

// Extract the OpenAI Chat Completion service from the kernel.
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Setup the Open AI Model to be able to use "Tools / Functions" automatically.
var promptExecutionSettings = new OpenAIPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

// Create a list to hold the chat messages.
var conversation = new ChatHistory();

// Setup common exit keys for the chat loop.
var exitKeys = new List<string> { "exit", "quit", "q" };

// Start a conversation loop.
var userInpt = string.Empty;
do
{
    Console.Write("You > ");
    userInpt = Console.ReadLine() ?? string.Empty;

    if (exitKeys.Any(key => userInpt.Equals(key, StringComparison.OrdinalIgnoreCase)))
        break;

    // Add user input to the conversation history.
    conversation.AddUserMessage(userInpt);
    
    // Call the OpenAI Chat Completion service to get a response.
    var result = await chatCompletionService.GetChatMessageContentAsync(conversation, promptExecutionSettings, kernel);
    
    // Add the AI response to the conversation history.
    conversation.AddAssistantMessage(result.Content);

    Console.WriteLine($"AI > {result.Content}");
} while (true);

Console.WriteLine("Goodbye!");
