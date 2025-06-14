using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

class ConversationPlugin
{
    private readonly ChatHistory _conversation = [];

    public ConversationPlugin(ChatHistory conversation)
    {
        _conversation = conversation ?? [];
    }

    // Another way of registering functions is to use the
    // KernelFunction attribute on a public method of a class.
    [KernelFunction()]
    [Description("Saves the conversation to a file")]
    public async Task<object> SaveConversation()
    {
        string message = "Conversation saved successfully.";
        bool success = true;
        string conversationFilePath = $"./{DateTime.Now:yyyyMMdd_HHmmss}_AI_CONVERSATION.md";
        try
        {
            var messages = _conversation
                            .Where(msg => !string.IsNullOrWhiteSpace(msg.Content))
                            .Select(msg => $"{msg.Role} > {msg.Content}\n");
            var messagesContent = string.Join("\n------------------------------\n", messages);

            await File.WriteAllTextAsync(path: conversationFilePath,
                                         contents: messagesContent);
        }
        catch (Exception ex)
        {
            message = $"Error saving conversation: {ex.Message}";
            success = false;
        }

        return new
        {
            FilePath = conversationFilePath,
            Success = success,
            Message = message,
            Timestamp = success ? new DateTime?(DateTime.Now) : null,
        };
    }

    [KernelFunction, Description("Counts messages in the conversation")]
    public object CountMessagesInConversation()
    {
        var count = _conversation.Count;
        var sysMessageCount = _conversation.Count(msg => msg.Role == AuthorRole.System);
        var userMessageCount = _conversation.Count(msg => msg.Role == AuthorRole.User);
        var assistantMessageCount = _conversation.Count(msg => msg.Role == AuthorRole.Assistant);
        var toolMessageCount = _conversation.Count(msg => msg.Role == AuthorRole.Tool);

        return new
        {
            TotalMessages = count,
            SystemMessages = sysMessageCount,
            UserMessages = userMessageCount,
            AssistantMessages = assistantMessageCount,
            ToolMessages = toolMessageCount
        };
    }

    [KernelFunction, Description("Gets the current date and time")]
    public static DateTime GetCurrentDate()
    {
        return DateTime.Now;
    }
}
