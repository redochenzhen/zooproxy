using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    public interface INodeProxyFactory
    {
        Task<INodeProxy> ProxyNodeAsync(string name, bool watch = false, string parentPath = default);

        Task<IDataNodeProxy<T>> ProxyValueNodeAsync<T>(string name, bool watch = false, string parentPath = default);

        Task<IDataNodeProxy<T>> ProxyJsonNodeAsync<T>(string name, bool watch = false, string parentPath = default) where T : class;

        Task<IPropertyNodeProxy> ProxyPropertyNodeAsync(string name, bool watch = false, string parentPath = default);
    }
}
