using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.ZooProxy
{
    public class NodeEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }

    public class DataChangedEventArgs<T> : NodeEventArgs
    {
        public T Data { get; set; }
    }

    public class PropertyChangedEventArgs : NodeEventArgs
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ChildrenChangedEventArgs : NodeEventArgs
    {
        public List<string> Children { get; set; }
    }
}
