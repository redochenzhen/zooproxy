using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.ZooProxy
{
    internal class StringNodeProxy : ValueNodeProxy<string>
    {
        public StringNodeProxy(ZooKeeperClient zkClient, string name, string path) : base(zkClient, name, path) { }

        protected override string BytesToData(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            var stringValue = Encoding.UTF8.GetString(bytes);
            return stringValue;
        }
    }
}
