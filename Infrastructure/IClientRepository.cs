using MvcChat.Model;
using System.Collections.Generic;

namespace MvcChat.Infrastructure
{
    // Interface for repository of joined users.
    public interface IClientRepository
    {
        /// <summary>
        /// Adds a user with name
        /// </summary>
        /// <param name="name">User name</param>
        /// <returns>identifier of created user</returns>
        string Add(string name);

        /// <summary>
        /// Removes a user from repository
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>User object that was deleted from repository</returns>
        Client Delete(string id);

        /// <summary>
        /// Tests whether users exists in repository
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>True if exists in repository</returns>
        bool Has(string id);

        /// <summary>
        /// Enumerable sequence of all users in repository
        /// </summary>
        IEnumerable<KeyValuePair<string,Client>> Clients {get;}
    }
}