using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    public interface ISubNodeProxyFactory
    {
        Task<INodeProxy> ProxyNodeAsync(string name, bool watch = false);

        Task<IDataNodeProxy<T>> ProxyValueNodeAsync<T>(string name, bool watch = false);

        Task<IDataNodeProxy<T>> ProxyJsonNodeAsync<T>(string name, bool watch = false) where T : class;

        Task<IPropertyNodeProxy> ProxyPropertyNodeAsync(string name, bool watch = false);
    }
}
