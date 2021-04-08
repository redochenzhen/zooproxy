using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    internal abstract partial class DataNodeProxy<T> : NodeProxy, IDataNodeProxy<T>
    {
        public event EventHandler<DataChangedEventArgs<T>> DataChanged;

        protected DataNodeProxy(ZooKeeperClient zkClient, string name, string path) : base(zkClient, name, path) { }

        public async Task CreateAsync(T data, Permission permission, Mode mode, bool ignoreExists)
        {
            var bytes = DataToBytes(data);
            var acl = AclFromAnyone(permission);
            await CreateAsync(bytes, acl, mode, ignoreExists);
        }

        public async Task CreateAsync(T data, string userName, string password, Permission permission, Mode mode, bool ignoreExists)
        {
            var bytes = DataToBytes(data);
            var acl = AclFromUserPassword(userName, password, permission);
            await CreateAsync(bytes, acl, mode, ignoreExists);
        }

        public async Task SetDataAsync(T data)
        {
            var bytes = DataToBytes(data);
            await SetDataAsync(bytes).ConfigureAwait(false);
        }

        public async Task<T> GetDataAsync()
        {
            var result = await Connection.getDataAsync(Path).ConfigureAwait(false);
            var data = BytesToData(result.Data);
            return data;
        }

        protected abstract byte[] DataToBytes(T data);

        protected abstract T BytesToData(byte[] bytes);

        private async Task SetDataAsync(byte[] data, bool force = false)
        {
            var ver = force ? -1 : _stat?.getVersion() ?? -1;
            await Connection.setDataAsync(Path, data, ver).ConfigureAwait(false);
        }
    }
}
