using System;
using Xunit;

namespace Keep.ZooProxy.Tests
{
    public class NodeModeTests
    {
        [Fact]
        public void ModeExts()
        {
            var p = Mode.Persistent;
            Assert.True(p.IsPersistent());
            Assert.False(p.IsSequential());

            var e = Mode.Ephemeral;
            Assert.False(e.IsPersistent());
            Assert.False(e.IsSequential());

            var ps = Mode.PersistentSequential;
            Assert.True(ps.IsPersistent());
            Assert.True(ps.IsSequential());

            var es = Mode.EphemeralSequential;
            Assert.False(es.IsPersistent());
            Assert.True(es.IsSequential());
        }
    }
}
