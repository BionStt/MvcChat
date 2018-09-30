using System.Collections.Generic;
using MvcChat.Model;
using System.Threading.Tasks;

namespace MvcChat.Infrastructure
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesFor(string id, int timeout);
        
        bool AddMessage(Message msg);

        bool Init(string id);

        void Free(string id);
    }
}