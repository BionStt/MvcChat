using System.Collections.Generic;
using MvcChat.Model;

namespace MvcChat.Infrastructure
{
    public interface IMessageRepository
    {
        IEnumerable<Message> GetMessages(string id);
        bool Add(string id, Message msg);
        void AddToAll(Message msg);
        bool Init(string id);
        void Free(string id);
    }
}