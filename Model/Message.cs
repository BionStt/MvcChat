using System.ComponentModel.DataAnnotations;

namespace MvcChat.Model
{
    public enum MessageType {UsersList, TextMessage};
    public class Message
    {
        [Required]
        public MessageType type;

        [Required, RegularExpression(@"\d{1,64}")]
        public string from_id;
        [Required, RegularExpression(@"\d{0,64}")]
        public string to_id;
        [Required]
        public object data;
    }
}