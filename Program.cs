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

// Create a list to hold the chat messages.
var conversation = new ChatHistory();

kernelBuilder
    .Services
    .AddSingleton(conversation)
    .AddSingleton<ConverstationPlugin>()
    .AddSingleton(sp =>
    {
        var kernelFunction = KernelFunctionFactory.CreateFromMethod(
            functionName: "ConsoleClear",
            description: "Clears the current output in the console",
            method: () =>
            {
                Console.Clear();
            }
        );

        KernelPluginCollection plugins =
        [
            KernelPluginFactory.CreateFromObject(sp.GetRequiredService<ConverstationPlugin>()),
            KernelPluginFactory.CreateFromFunctions(pluginName: "ConsolePlugin", [kernelFunction])
        ];
        return plugins;
    })
    .AddSingleton(sp =>
    {
        var kernel = new Kernel(sp, sp.GetRequiredService<KernelPluginCollection>());
        return kernel;
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
var userInpt = string.Empty;
do
{
    Console.Write("You > ");
    userInpt = Console.ReadLine() ?? string.Empty;

    // Add user input to the conversation history.
    conversation.AddUserMessage(userInpt);

    // Call the OpenAI Chat Completion service to get a response.
    var result = await chatCompletionService.GetChatMessageContentAsync(conversation, promptExecutionSettings, kernel);

    // Add the AI response to the conversation history.
    conversation.AddAssistantMessage(result.Content);

    Console.WriteLine($"AI > {result.Content}");
} while (true);

class ConverstationPlugin
{
    private ChatHistory _conversation = new();

    public ConverstationPlugin(ChatHistory conversation)
    {
        _conversation = conversation ?? new();
    }

    // Another way of registering functions is to use the
    // KernelFunction attribute on a public method of a class.
    [KernelFunction()]
    [Description("Saves the conversation to a file")]
    public bool SaveConversation(string content)
    {
        File.WriteAllText(path: "./AI_SAVED_CONVERSATION.txt", contents: content);
        return true;
    }

    [KernelFunction, Description("Counts the lines in the conversation")]
    public object CountLinesOfConversations()
    {
        var messages = _conversation.Where(msg => (msg.Role == AuthorRole.System
                                                  || msg.Role == AuthorRole.Assistant
                                                  || msg.Role == AuthorRole.User)
                                                  && !string.IsNullOrWhiteSpace(msg.Content))
                                    .Select(msg => msg.Content);
        var messagesBlock = string.Join("\n", messages);
        Console.WriteLine($"{messagesBlock}");
        var result = new
        {
            messageCount = messages.Count(),
            lines = messagesBlock.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).Count()
        };

        Console.WriteLine($"result: messages: {result.messageCount}, lines: {result.lines}");
        return result;
    }

    [KernelFunction, Description("Exits the application. Keep in mind that they user may say bye, or good bye, or similar, then too the applicatio should exit.")]
    public void ExitApplication()
    {
        Environment.Exit(0);
    }
}
