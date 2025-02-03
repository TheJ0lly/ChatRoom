using System.Text.Json;
using System.Windows.Controls;

namespace ChatRoom.Network
{
    public struct Message
    {
        public string? User { get; set; }
        public string? Time { get; set; }
        public string? Text { get; set; }
    }

    public class MessageView : ListBoxItem
    {
        public MessageView(Message msg)
        {
            Content = $"{msg.Time} - {msg.User}: {msg.Text}";
        }
    }

    public static class MessageManager
    {
        public static string ToJson(Message message)
        {
            return JsonSerializer.Serialize(message);
        }

        public static Message FromJson(string json)
        {
            return JsonSerializer.Deserialize<Message>(json);
        }
    }
}
