using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// This example demonstrates how to use the OpenAI Chat Completion service with Semantic Kernel in C#.

// Model Name to use for the OpenAI Chat Completion
var modelId = "gpt-4.1-nano";

// API key for OpenAI API Access.
var envVar = "OPENAI_API_KEY";
var apiKey = Environment.GetEnvironmentVariable(envVar) ?? throw new InvalidOperationException($"Env Variable with name: {envVar} not found");

// Create Semantic Kernel Builder with OpenAI Service.
var kernelBuilder = Kernel.CreateBuilder()
                          .AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);

// System Prompt about a helpful DB query execution assistant.
var systemPrompt =
"""
You are a helpful AI assistant.
""";

// Create a list to hold the chat messages.
var conversation = new ChatHistory();

// Add the system prompt to the conversation history.
conversation.AddSystemMessage(systemPrompt);

kernelBuilder
    .Services
    .AddSingleton(conversation)
    .AddSingleton<ConversationPlugin>()
    .AddSingleton(sp =>
    {
        var clearFunction = KernelFunctionFactory.CreateFromMethod(
            functionName: "ConsoleClear",
            description: "Clears the current output in the console for the user",
            method: ([Description("Whether the user confirmed to clear the console or not, clears the console only if confirmed.")] bool confirm) =>
            {
                if (confirm)
                {
                    Console.Clear();
                }
            }
        );

        var exitFunction = KernelFunctionFactory.CreateFromMethod(
            functionName: "ExitApplication",
            description: "Exits the application for the user.",
            method: ([Description("Whether the user confirmed to exit or not, exits only if confirmed.")] bool confirm) =>
            {
                if (confirm)
                {
                    Environment.Exit(0);
                }
            }
        );

        KernelPluginCollection plugins =
        [
            KernelPluginFactory.CreateFromObject(sp.GetRequiredService<ConversationPlugin>()),
            KernelPluginFactory.CreateFromFunctions(pluginName: "AppPlugin", [clearFunction, exitFunction]),
        ];
        return plugins;
    })
    ;

// Build the Kernel instance.
var kernel = kernelBuilder.Build();

// Extract the OpenAI Chat Completion service from the kernel.
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Setup the Open AI Model to be able to use "Tools / Functions" automatically.
var promptExecutionSettings = new OpenAIPromptExecutionSettings()
{
    // LLM will be able to call the function automatically.
    // Meaning it will not ask your permission before
    // calling tools.
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

// Start a conversation loop.
var userInput = string.Empty;
do
{
    Console.Write("You > ");
    userInput = Console.ReadLine() ?? string.Empty;

    // Add user input to the conversation history.
    conversation.AddUserMessage(userInput);

    // Call the OpenAI Chat Completion service to get a response.
    var result = await chatCompletionService.GetChatMessageContentAsync(conversation, promptExecutionSettings, kernel);

    // Add the AI response to the conversation history.
    conversation.AddAssistantMessage(result.Content);

    Console.WriteLine($"AI > {result.Content}");
} while (true);
