using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    internal class JsonNodeProxy<T> : DataNodeProxy<T>, IDataNodeProxy<T> where T : class
    {
        public JsonNodeProxy(ZooKeeperClient zkClient, string name, string path) : base(zkClient, name, path) { }

        protected override byte[] DataToBytes(T data)
        {
            var bytes = new byte[0];
            if (data != null)
            {
                var option = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                bytes = JsonSerializer.SerializeToUtf8Bytes(data, option);
            }
            return bytes;
        }

        protected override T BytesToData(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            var data = JsonSerializer.Deserialize<T>(bytes);
            return data;
        }
    }
}
