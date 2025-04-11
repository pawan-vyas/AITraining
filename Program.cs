using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var modelId = "gpt-4o-mini";
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
             ?? throw new InvalidOperationException("Please set the OpenAI API key in the environment variable.");

var kernelBuilder = Kernel.CreateBuilder()
                          .AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);

var kernel = kernelBuilder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var promptExecutionSettings = new OpenAIPromptExecutionSettings()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

var conversation = new ChatHistory();

var exitKeys = new List<string> { "exit", "quit", "q" };
var userInpt = string.Empty;
do
{
    Console.Write("You > ");
    userInpt = Console.ReadLine() ?? string.Empty;

    if (exitKeys.Any(key => userInpt.Equals(key, StringComparison.OrdinalIgnoreCase)))
        break;

    conversation.AddUserMessage(userInpt);
    var result = await chatCompletionService.GetChatMessageContentAsync(conversation, promptExecutionSettings, kernel);
    Console.WriteLine($"AI > {result.Content}");
    conversation.AddAssistantMessage(result.Content);
} while (true);

Console.WriteLine("Goodbye!");
