using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

class ConversationPlugin
{
    private ChatHistory _conversation = new();

    public ConversationPlugin(ChatHistory conversation)
    {
        _conversation = conversation ?? new();
    }

    // Another way of registering functions is to use the
    // KernelFunction attribute on a public method of a class.
    [KernelFunction()]
    [Description("Saves the conversation to a file")]
    public bool SaveConversation(string content)
    {
        File.WriteAllText(path: $"./{DateTime.Now:yyyyMMdd_HHmmss}_AI_SAVED_CONVERSATION.txt", contents: content);
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
