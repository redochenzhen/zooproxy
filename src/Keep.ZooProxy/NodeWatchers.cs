using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    //Watcher作为内部类，方便访问后者的事件和私有成员

    public partial class ZooKeeperClient
    {
        private class DefaultWatcher : Watcher
        {
            private readonly TaskCompletionSource<Void> _tcs;
            private readonly ZooKeeperClient _client;
            private bool _firstConnected = true;

            public DefaultWatcher(ZooKeeperClient client, TaskCompletionSource<Void> tcs)
            {
                _tcs = tcs;
                _client = client;
            }

            public override Task process(WatchedEvent @event)
            {
                var state = @event.getState();
                if (state == Event.KeeperState.SyncConnected)
                {
                    if (_tcs.TrySetResult(default))
                    {
                        _firstConnected = false;
                    }
                    else if (!_firstConnected)
                    {
                        _client.ReConnected?.Invoke(this, null);
                    }
                }
                else if (state == Event.KeeperState.Disconnected)
                {
                    _client.Disconnected?.Invoke(this, null);
                }
                else if (state == Event.KeeperState.Expired)
                {
                    _client.SessionExpired?.Invoke(this, null);
                }
                return Task.CompletedTask;
            }
        }

        struct Void { }
    }

    internal partial class NodeProxy
    {
        internal class NodeWatcher : Watcher
        {
            protected readonly NodeProxy _node;

            public NodeWatcher(NodeProxy node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }
                _node = node;
            }

            protected virtual void OnNodeCreated(string name, string path)
            {
                if (_node.NodeCreated == null) return;
                var args = new NodeEventArgs
                {
                    Name = name,
                    Path = path
                };
                _node.NodeCreated(this, args);
            }

            protected virtual void OnNodeDeleted(string name, string path)
            {
                if (_node.NodeDeleted == null) return;
                var args = new NodeEventArgs
                {
                    Name = name,
                    Path = path
                };
                _node.NodeDeleted(this, args);
            }

            protected virtual void OnChildrenChanged(string name, string path, List<string> children)
            {
                if (_node.ChildrenChanged == null) return;
                var args = new ChildrenChangedEventArgs
                {
                    Name = name,
                    Path = path,
                    Children = children
                };
                _node.ChildrenChanged.Invoke(this, args);
            }

            public override async Task process(WatchedEvent @event)
            {
                var state = @event.getState();
                //异常状态由DefaultWatcher处理
                if (state != Event.KeeperState.SyncConnected) return;

                var path = @event.getPath();
                var type = @event.get_Type();
                var conn = _node.Connection;

                for (; ; )
                {
                    try
                    {

                        switch (type)
                        {
                            case Event.EventType.NodeCreated:
                                {
                                    await conn.existsAsync(path, this);
                                    OnNodeCreated(_node.Name, path);
                                    return;
                                }
                            case Event.EventType.NodeDeleted:
                                {
                                    await conn.existsAsync(path, this);
                                    OnNodeDeleted(_node.Name, path);
                                    return;
                                }
                            case Event.EventType.NodeChildrenChanged:
                                {
                                    ChildrenResult result;
                                    result = await conn.getChildrenAsync(path, this);
                                    OnChildrenChanged(_node.Name, path, result.Children);
                                    return;
                                }
                            default:
                                return;
                        }
                    }
                    catch (KeeperException.ConnectionLossException)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    catch (KeeperException.SessionExpiredException) { break; }
                    //process中抛出的异常会被ZooKeeperNetEx吞没
                    catch (Exception)
                    {
                        //TODO: log
                        throw;
                    }
                }
            }
        }
    }

    internal partial class DataNodeProxy<T>
    {
        internal new class NodeWatcher : NodeProxy.NodeWatcher
        {
            public NodeWatcher(DataNodeProxy<T> node) : base(node) { }

            protected void OnDataChanged(string name, string path, byte[] data)
            {
                var node = _node as DataNodeProxy<T>;
                if (node.DataChanged == null) return;
                var args = new DataChangedEventArgs<T>
                {
                    Name = name,
                    Path = path,
                    Data = node.BytesToData(data)
                };
                node.DataChanged(this, args);
            }

            public override async Task process(WatchedEvent @event)
            {
                await base.process(@event);
                //process中的异常会被ZooKeeperNetEx吞没
                var node = _node as DataNodeProxy<T>;
                var conn = node.Connection;
                var path = @event.getPath();
                var type = @event.get_Type();
                if (type == Event.EventType.NodeDataChanged)
                {
                    DataResult result;
                    try
                    {
                        result = await conn.getDataAsync(path, this);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        throw;
                    }
                    OnDataChanged(_node.Name, path, result.Data);
                }
            }
        }
    }
}
