using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    internal partial class NodeProxy : INodeProxy
    {
        private readonly ZooKeeperClient _zkClient;
        private NodeWatcher _watcher;
        protected Stat _stat;
        public ZooKeeper Connection => _zkClient.Connection;
        public event EventHandler<NodeEventArgs> NodeCreated;
        public event EventHandler<NodeEventArgs> NodeDeleted;
        public event EventHandler<ChildrenChangedEventArgs> ChildrenChanged;

        public string Path { get; private set; }
        public string Name { get; private set; }

        public bool IsWatched
        {
            get { return _watcher != null; }
        }

        public NodeProxy(ZooKeeperClient zkClient, string name, string path = default)
        {
            path = path ?? "/";
            Path = CheckPath(path);
            Name = name;
            _zkClient = zkClient;
        }

        public virtual async Task WatchedByAsync(NodeWatcher watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }
            _watcher = watcher;
            //说明：如果znode存在，则会注册一个existWatch, 反之则会注册一个dataWatch
            _stat = await Connection.existsAsync(Path, _watcher).ConfigureAwait(false);
            //如果znode已经存在，则直接监控其children
            if (_stat != null)
            {
                await WatchChildrenAsync(_watcher).ConfigureAwait(false);
            }
            ////znode被删除时，会丢失对children的监控
            //NodeCreated += async (_, __) =>
            //{
            //    await WatchChildrenAsync(watcher).ConfigureAwait(false);
            //};
        }

        private async Task WatchChildrenAsync(Watcher watcher)
        {
            var result = await Connection.getChildrenAsync(Path, watcher).ConfigureAwait(false);
            _stat = result.Stat;
        }

        public async Task<bool> ExistsAsync()
        {
            _stat = await Connection.existsAsync(Path).ConfigureAwait(false);
            return _stat != null;
        }

        public async Task CreateAsync(Permission permission, Mode mode, bool ignoreExists)
        {
            var acl = AclFromAnyone(permission);
            await CreateAsync(null, acl, mode, ignoreExists);
        }

        public async Task CreateAsync(string userName, string password, Permission permission, Mode mode, bool ignoreExists)
        {
            var acl = AclFromUserPassword(userName, password, permission);
            await CreateAsync(null, acl, mode, ignoreExists);
        }

        protected async Task CreateAsync(byte[] data, List<ACL> acl, Mode mode, bool ignoreExists)
        {
            var cmode = mode.ToCreateMode();
            try
            {
                await Connection.createAsync(Path, data, acl, cmode).ConfigureAwait(false);
            }
            catch (KeeperException.NodeExistsException)
            {
                if (!ignoreExists) throw;
            }
            finally
            {
                if (_watcher != null)
                {
                    await WatchChildrenAsync(_watcher).ConfigureAwait(false);
                }
            }
        }

        public async Task DeleteAsync()
        {
            await Connection.deleteAsync(Path).ConfigureAwait(false);
        }

        public async Task<List<string>> GetChildrenAsync()
        {
            var redult = await Connection.getChildrenAsync(Path).ConfigureAwait(false);
            _stat = redult.Stat;
            return redult?.Children;
        }

        public async Task BindDigestAsync(string userName, string password, Permission permission)
        {
            var acl = AclFromUserPassword(userName, password, permission);
            await SetAclAsync(acl).ConfigureAwait(false);
        }

        public async Task BindIpAsync(string ip, Permission permission)
        {
            var acl = new List<ACL>
            {
                new ACL((int)permission,new Id(Scheme.IP,ip))
            };
            await SetAclAsync(acl).ConfigureAwait(false);
        }

        public async Task BindAnyoneAsync(Permission permission)
        {
            var acl = new List<ACL>
            {
                new ACL((int)permission, new Id(Scheme.WORLD, Scheme.World.ANYONE))
            };
            await SetAclAsync(acl).ConfigureAwait(false);
        }

        private async Task SetAclAsync(List<ACL> acl, bool force = false)
        {
            var aver = force ? -1 : _stat?.getAversion() ?? -1;
            await Connection.setACLAsync(Path, acl, aver).ConfigureAwait(false);
        }

        public async Task<INodeProxy> ProxyNodeAsync(string name, bool watch)
        {
            return await _zkClient.ProxyNodeAsync(name, watch, Path).ConfigureAwait(false);
        }

        public async Task<IDataNodeProxy<T>> ProxyValueNodeAsync<T>(string name, bool watch)
        {
            return await _zkClient.ProxyValueNodeAsync<T>(name, watch, Path).ConfigureAwait(false);
        }

        public async Task<IDataNodeProxy<T>> ProxyJsonNodeAsync<T>(string name, bool watch) where T : class
        {

            return await _zkClient.ProxyJsonNodeAsync<T>(name, watch, Path).ConfigureAwait(false);
        }

        public async Task<IPropertyNodeProxy> ProxyPropertyNodeAsync(string name, bool watch)
        {
            return await _zkClient.ProxyPropertyNodeAsync(name, watch, Path).ConfigureAwait(false);
        }

        protected List<ACL> AclFromAnyone(Permission permission)
        {
            return new List<ACL>
            {
                new ACL((int)permission, new Id(Scheme.WORLD, Scheme.World.ANYONE))
            };
        }

        protected List<ACL> AclFromUserPassword(string userName, string password, Permission permission)
        {
            var id = DigestHelper.GetDigest(userName, password);
            return new List<ACL>
            {
                new ACL((int)permission, new Id(Scheme.DIGEST, id))
            };
        }

        public static string JoinPath(string parentPath, string name) => $"{parentPath}/{name}";

        public static string CheckPath(string path)
        {
            if (path == "/") return path;
            if (Regex.IsMatch(path, "(/[^/]+)+")) return path;
            throw new ArgumentException($"Invalid znode path: {path}", nameof(path));
        }
    }
}
