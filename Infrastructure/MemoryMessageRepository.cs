using System.Collections.Generic;
using System.Collections.Concurrent;
using MvcChat.Model;
using System;
using System.Threading;

namespace MvcChat.Infrastructure
{
    public class MemoryMessageRepository : IMessageRepository
    {
        private ConcurrentDictionary<string, List<Message>> items = new ConcurrentDictionary<string, List<Message>>();

        public IEnumerable<Message> GetMessages(string id)
        {
            try
            {
                return items[id];
            }
            catch(KeyNotFoundException)
            {
                return null;
            }
        }

        public void AddToAll(Message msg)
        {
            foreach(KeyValuePair<string, List<Message>> l in items)
            {
                Object syncObj = l.Value;
                lock(syncObj)
                {
                    if(msg.type==MessageType.UsersList)
                    {
                        //Old messages with list of users are not necessary anymore 
                        l.Value.RemoveAll(m=>m.type==MessageType.UsersList);
                    }

                    l.Value.Add(msg);
                    Monitor.Pulse(syncObj);
                }
            }
        }

        public bool Add(string id, Message msg)
        {
            try
            {
                items[id].Add(msg);
                return true;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
        }

        public bool Init(string id){
            items[id] = new List<Message>();
            return true;
        }

        public void Free(string id)
        {
            List<Message> l;
            items.TryRemove(id, out l);
        }
    }
}