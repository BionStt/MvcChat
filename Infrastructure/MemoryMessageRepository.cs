using MvcChat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvcChat.Infrastructure
{
    public class MemoryMessageRepository : IMessageRepository
    {
        private Dictionary<string, List<Message>> items = new Dictionary<string, List<Message>>();

        // IMessageRepository implementation

        public Task<IEnumerable<Message>> GetMessagesFor(string id, int timeout)
        {
            List<Message> messages = null;
            try
            {
                messages = items[id];
            }
            catch (KeyNotFoundException)
            {
                return Task<IEnumerable<Message>>.FromResult(new List<Message>().AsEnumerable());
            }

            if (null == messages || messages.Count < 1) return Task.Run(() => WaitMessagesFor(id, timeout).AsEnumerable());

            // clear messages
            Init(id);

            return Task<IEnumerable<Message>>.FromResult(messages.AsEnumerable());
        }

        public bool AddMessage(Message msg)
        {
            string to = msg.to_id?.Trim();
            if (null == to || to.Length < 1)
            {
                AddToAll(msg);
                return true;
            }

            return Add(to, msg);
        }

        public bool Init(string id)
        {
            try
            {
                List<Message> listMsgId = items[id];
                lock (listMsgId)
                {
                    Monitor.Pulse(listMsgId);
                }       
            }
            catch (KeyNotFoundException)
            {
            }

            items[id] = new List<Message>();
            return true;
        }

        public void Free(string id)
        {
            try
            {
                List<Message> listMsgId = items[id];
                lock (listMsgId)
                {
                    items.Remove(id);
                    Monitor.Pulse(listMsgId);
                }       
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// Adds a message to the list of messages for user with specified id
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="msg">Incoming message</param>
        /// <returns>True if the message was added successfully</returns>
        private bool Add(string id, Message msg)
        {
            List<Message> listMsgId;
            try
            {
                listMsgId = items[id];
                lock (listMsgId)
                {
                    listMsgId.Add(msg);
                    Monitor.Pulse(listMsgId);
                }

                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a message to the lists of messages for all users
        /// </summary>
        /// <param name="msg">Incoming message</param>
        private void AddToAll(Message msg)
        {
            foreach(KeyValuePair<string, List<Message>> l in items)
            {
                Object syncObj = l.Value;
                lock(syncObj)
                {
                    if(msg.type==MessageType.UsersList)
                    {
                        //Previous messages with list of users are already outdated
                        l.Value.RemoveAll(m=>m.type==MessageType.UsersList);
                    }

                    l.Value.Add(msg);
                    Monitor.Pulse(syncObj);
                }
            }
        }

        /// <summary>
        /// Wait until neww message is added to the list of message for user with specified id
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="timeout">Timeout in milliseconds for waiting
        /// if there are not messages at the monent of calling
        /// the function</param>
        /// <returns></returns>
        private List<Message> WaitMessagesFor(string id, int timeout)
        {
            try
            {
                List<Message> listOfMessages = items[id];
                Object syncObj = listOfMessages;

                lock (syncObj)
                {
                    if (listOfMessages.Count < 1)
                        Monitor.Wait(syncObj, timeout);

                    // assign new empty List of messages to client
                    Init(id);

                    // these messages may be lost in case if connection is already broken
                    return listOfMessages;
                }
            }
            catch(KeyNotFoundException)
            {
                return new List<Message>();
            } 
        }
    }
}