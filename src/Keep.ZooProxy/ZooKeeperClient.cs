using System;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Keep.ZooProxy
{
    public partial class ZooKeeperClient : INodeProxyFactory, IDisposable
    {
        private const int DEFAULT_CONNECTION_TIMEOUT_MS = 20000;
        private const int DEFAULT_SESSION_TIMEOUT_MS = 20000;
        private ZooKeeper _connection;
        private readonly string _connectionString;
        private readonly int _sessionTimeout;
        private readonly int _connectionTimeout;

        /// <summary>
        /// 第一次成功连接到服务器事件
        /// </summary>
        public event EventHandler FirstConnected;

        /// <summary>
        /// 丢失连接后，重新连接到服务器事件
        /// </summary>
        public event EventHandler ReConnected;

        /// <summary>
        /// 连接中断事件
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Session过期事件（此时临时节点已经删除）
        /// </summary>
        public event EventHandler SessionExpired;

        internal ZooKeeper Connection
        {
            get
            {
                if (_connection == null)
                {
                    throw new InvalidOperationException("Should open this client before using its connection.");
                }
                return _connection;
            }
        }

        public ZooKeeperClient(string connectionString,
            int sessionTimeout = DEFAULT_SESSION_TIMEOUT_MS,
            int connectionTimeout = DEFAULT_CONNECTION_TIMEOUT_MS)
        {
            _connectionString = connectionString;
            if (sessionTimeout > 0)
            {
                _sessionTimeout = sessionTimeout;
                _connectionTimeout = connectionTimeout;
            }
        }

        /// <summary>
        /// 建立与ZooKeeper服务器的链接
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        public async Task OpenAsync()
        {
            var tcs = new TaskCompletionSource<Void>();
            var watcher = new DefaultWatcher(this, tcs);
            _connection = new ZooKeeper(_connectionString, _sessionTimeout, watcher);
            var task = await Task.WhenAny(tcs.Task, Task.Delay(_connectionTimeout)).ConfigureAwait(false);
            bool succeed = true;
            if (task != tcs.Task)
            {
                try
                {
                    succeed = false;
                    tcs.SetException(new TimeoutException($"Connecting to '{_connectionString}' timeout"));
                    await tcs.Task;
                }
                catch (InvalidOperationException)
                {
                    //ignore
                    //连接即将timeout的时候被抢救
                    succeed = true;
                }
            }
            if (succeed)
            {
                FirstConnected?.Invoke(watcher, null);
            }
        }

        /// <summary>
        /// 创建某个ZNode的代理（普通代理，无数据相关操作）
        /// </summary>
        /// <param name="name">ZNode的名称</param>
        /// <param name="watch">是否监听该ZNode</param>
        /// <returns></returns>
        public async Task<INodeProxy> ProxyNodeAsync(string name, bool watch = false, string parentPath = default)
        {
            var path = NodeProxy.JoinPath(parentPath, name);
            var node = new NodeProxy(this, name, path);
            if (watch)
            {
                var watcher = new NodeProxy.NodeWatcher(node);
                await node.WatchedByAsync(watcher).ConfigureAwait(false);
            }
            return node;
        }

        public async Task<IDataNodeProxy<T>> ProxyValueNodeAsync<T>(string name, bool watch = false, string parentPath = default)
        {
            var path = NodeProxy.JoinPath(parentPath, name);
            DataNodeProxy<T> node;
            if (typeof(T) == typeof(string))
            {
                node = new StringNodeProxy(this, name, path) as ValueNodeProxy<T>;
            }
            else
            {
                node = new ValueNodeProxy<T>(this, name, path);
            }
            if (watch)
            {
                var watcher = new DataNodeProxy<T>.NodeWatcher(node);
                await node.WatchedByAsync(watcher).ConfigureAwait(false);
            }
            return node;
        }

        public async Task<IDataNodeProxy<T>> ProxyJsonNodeAsync<T>(string name, bool watch = false, string parentPath = default) where T : class
        {
            var path = NodeProxy.JoinPath(parentPath, name);
            var node = new JsonNodeProxy<T>(this, name, path);
            if (watch)
            {
                var watcher = new DataNodeProxy<T>.NodeWatcher(node);
                await node.WatchedByAsync(watcher).ConfigureAwait(false);
            }
            return node;
        }

        public async Task<IPropertyNodeProxy> ProxyPropertyNodeAsync(string name, bool watch = false, string parentPath = default)
        {
            var path = NodeProxy.JoinPath(parentPath, name);
            var node = new PropertyNodeProxy(this, name, path);
            if (watch)
            {
                var watcher = new NodeProxy.NodeWatcher(node);
                await node.WatchedByAsync(watcher).ConfigureAwait(false);
            }
            return node;
        }

        public async Task Close()
        {
            if (_connection == null) return;
            await _connection.closeAsync().ConfigureAwait(false);
        }

        public async void Dispose()
        {
            await _connection?.closeAsync();
        }
    }
}
