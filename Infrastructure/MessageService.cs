using MvcChat.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace MvcChat.Infrastructure
{
    public class MessageService : IMessageService
    {
        public MessageService(IMessageRepository messagesStore)
        {
            _messagesStore = messagesStore;
        }
        private IMessageRepository _messagesStore;

        public Task<IEnumerable<Message>> GetMessagesFor(string id, int timeout)
        {
            IEnumerable<Message> messages = null;
            try
            {
                messages = _messagesStore.GetMessages(id);
            }
            catch (KeyNotFoundException)
            {
                return Task<IEnumerable<Message>>.FromResult(new List<Message>().AsEnumerable());
            }

            if (null == messages || messages.Count() < 1) return Task.Run(() => WaitMessagesFor(id, timeout));

            //clear messages
            Init(id);

            return Task<IEnumerable<Message>>.FromResult(messages);
        }
        public bool AddMessage(Message msg)
        {
            string to = msg.to_id?.Trim();
            if (null == to || to.Length < 1)
            {
                _messagesStore.AddToAll(msg);
                return true;
            }

            return AddMessageTo(to, msg);
        }

        public bool AddMessageTo(string id, Message msg)
        {
            IEnumerable<Message> l;
            try
            {
                l = _messagesStore.GetMessages(id);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            Object syncObj = l;
            lock (syncObj)
            {
                _messagesStore.Add(id, msg);
                Monitor.Pulse(syncObj);
            }

            return true;
        }

        public bool Init(string id)
        {
            return _messagesStore.Init(id);
        }

        public void Free(string id)
        {
            IEnumerable<Message> l;
            try
            {
                l = _messagesStore.GetMessages(id);
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            Object syncObj = l;
            lock (syncObj)
            {
                _messagesStore.Free(id);
                Monitor.Pulse(syncObj);
            }
        }

        public IEnumerable<Message> WaitMessagesFor(string id, int timeout)
        {
            IEnumerable<Message> listOfMessages = _messagesStore.GetMessages(id);
            Object syncObj = listOfMessages;

            lock (syncObj)
            {
                if (listOfMessages.Count() < 1)
                    Monitor.Wait(syncObj, timeout);

                //assign new empty List of messages to client
                Init(id);

                //these messages may be lost in case if connection is already broken
                return listOfMessages;
            }
        }
    }
}