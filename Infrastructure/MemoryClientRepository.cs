using System.Collections.Concurrent;
using System.Collections.Generic;
using MvcChat.Model;

namespace MvcChat.Infrastructure
{
    public class MemoryClientRepository : IClientRepository
    {
        private ConcurrentDictionary<string, Client> items;
        public MemoryClientRepository()
        {
            items = new ConcurrentDictionary<string, Client>();
        }

        // IClientRepository implementation
        
        public Client this[string id] => items.ContainsKey(id) ? items[id] : null;

        public IEnumerable<KeyValuePair<string,Client>> Clients => items;
        public string Add(string name)
        {
            int ClientId = new System.Random().Next();
            string id = ClientId.ToString();
            items[id] = new Client{name=name};
            return id;
        }
        public Client Delete(string id)
        {
            Client c;
            items.TryRemove(id, out c);
            return c;
        }

        public bool Has(string id) => items.ContainsKey(id);
    }
}