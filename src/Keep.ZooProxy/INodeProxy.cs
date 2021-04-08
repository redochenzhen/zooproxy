using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    public interface INodeProxy : ISubNodeProxyFactory
    {
        string Path { get; }

        string Name { get; }

        event EventHandler<NodeEventArgs> NodeCreated;

        event EventHandler<NodeEventArgs> NodeDeleted;

        event EventHandler<ChildrenChangedEventArgs> ChildrenChanged;

        Task<bool> ExistsAsync();

        Task CreateAsync(Permission permission = Permission.Read, Mode mode = Mode.Persistent, bool ignoreExists = false);

        Task CreateAsync(string userName, string password, Permission permission = Permission.Read, Mode mode = Mode.Persistent, bool ignoreExists = true);

        Task DeleteAsync();

        Task<List<string>> GetChildrenAsync();

        Task BindDigestAsync(string userName, string password, Permission permission = Permission.Read);

        Task BindIpAsync(string ip, Permission permission = Permission.Read);

        Task BindAnyoneAsync(Permission permission = Permission.Read);
    }

    public interface IDataNodeProxy<T> : INodeProxy
    {
        event EventHandler<DataChangedEventArgs<T>> DataChanged;

        Task CreateAsync(T data, Permission permission = Permission.Read, Mode mode = Mode.Persistent, bool ignoreExists = true);

        Task CreateAsync(T data, string userName, string password, Permission permission = Permission.Read, Mode mode = Mode.Persistent, bool ignoreExists = true);

        Task<T> GetDataAsync();

        Task SetDataAsync(T data);
    }

    public interface IPropertyNodeProxy : INodeProxy
    {
        event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        Task CreateAsync(IDictionary<string, string> properties, Permission permission = Permission.Read, Mode mode = Mode.Persistent, bool ignoreExists = false);

        Task<string> GetValueAsync(string key);

        Task SetValueAsync(string key, string value);

        Task<bool> ContainsKeyAsync(string key);
    }
}
