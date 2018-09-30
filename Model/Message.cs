using System.ComponentModel.DataAnnotations;

namespace MvcChat.Model
{
    public enum MessageType {UsersList, TextMessage};
    public class Message
    {
        /// <summary>
        /// Type of message: list of user or text message
        /// </summary>
        [Required]
        public MessageType type;

        /// <summary>
        /// Sender's identifier
        /// </summary>
        [Required, RegularExpression(@"\d{1,64}")]
        public string from_id;

        
        /// <summary>
        /// Recipient's identifier
        /// </summary>
        [Required, RegularExpression(@"\d{0,64}")]
        public string to_id;

        /// <summary>
        ///Message payload
        /// </summary>
        [Required]
        public object data;
    }
}