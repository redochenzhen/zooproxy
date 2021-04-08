using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Keep.ZooProxy.Tests
{
    public class NodePathTests
    {
        [Fact]
        public void CheckNodePath()
        {
            var zk = new ZooKeeperClient("");
            var node = zk.ProxyNodeAsync("a").Result
                .ProxyNodeAsync("b").Result
                .ProxyNodeAsync("c").Result;
            Assert.Equal("/a/b/c", node.Path);
            Assert.Equal("c", node.Name);
        }
    }
}
