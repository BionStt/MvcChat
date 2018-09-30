using MvcChat.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvcChat.Infrastructure
{
    public interface IMessageRepository
    {
        /// <summary>
        /// Gets messages for user with specified id
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="timeout">Timeout in milliseconds for waiting
        /// if there are not messages for user at the monent of calling
        /// the function</param>
        /// <returns>Enumerable sequence of messages for user with specified id</returns>
        Task<IEnumerable<Message>> GetMessagesFor(string id, int timeout);

        /// <summary>
        /// Adds message to the list
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool AddMessage(Message msg);

        /// <summary>
        /// Initializes a store for messages for user with specified id
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns></returns>
        bool Init(string id);

        /// <summary>
        /// Destroys messages store for user
        /// </summary>
        /// <param name="id">User identifier</param>
        void Free(string id);
    }
}