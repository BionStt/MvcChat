using MvcChat.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvcChat.Infrastructure
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetMessagesFor(string id, int timeout);
        bool AddMessage(Message msg);

        bool Init(string id);

        void Free(string id);
    }
}