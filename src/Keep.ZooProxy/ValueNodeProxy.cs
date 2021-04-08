using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Keep.ZooProxy
{
    internal class ValueNodeProxy<T> : DataNodeProxy<T>, IDataNodeProxy<T>
    {
        public ValueNodeProxy(ZooKeeperClient zkClient, string name, string path) : base(zkClient, name, path) { }

        protected override byte[] DataToBytes(T data)
        {
            var bytes = new byte[0];
            if (data != null)
            {
                bytes = Encoding.UTF8.GetBytes(data.ToString());
            }
            return bytes;
        }

        protected override T BytesToData(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            var stringValue = Encoding.UTF8.GetString(bytes);
            return GetValue(stringValue);
        }

        private static T GetValue(string stringValue)
        {
            if (stringValue == null) return default(T);
            //Type type = typeof(T);
            //var methodInfo = (from m in type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            //                  where m.Name == "TryParse"
            //                  select m).FirstOrDefault();

            //if (methodInfo == null)
            //    throw new ApplicationException("Unable to find TryParse method");

            //object result = methodInfo.Invoke(null, new object[] { stringValue, value });
            //if ((result != null) && ((bool)result))

            Type type = typeof(T);
            var methodInfo = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == nameof(int.Parse));
            if (methodInfo == null)
                throw new ApplicationException("Unable to find Parse method.");
            try
            {
                var value = (T)methodInfo.Invoke(null, new object[] { stringValue });
                return value;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    //TODO: log
                    Console.WriteLine($"{ex.InnerException.Message}(Input string: {stringValue})");
                    throw ex.InnerException;
                }
                throw;
            }
        }
    }
}
