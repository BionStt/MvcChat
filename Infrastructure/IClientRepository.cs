using System.Collections.Generic;
using MvcChat.Model;

namespace MvcChat.Infrastructure
{
    public interface IClientRepository
    {
        Client this[string id] {get;}
        string Add(string name);
        Client Delete(string id);

        bool Has(string id);

        IEnumerable<KeyValuePair<string,Client>> Clients {get;}
    }
}