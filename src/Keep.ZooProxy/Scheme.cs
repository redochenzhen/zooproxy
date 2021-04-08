using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.ZooProxy
{
    public static class Scheme
    {
        public const string DIGEST = "digest";
        public const string AUTH = "auth";
        public const string IP = "ip";
        public const string WORLD = "world";

        public static class World
        {
            public const string ANYONE = "anyone";
        }

        public static class Digest
        {
            public const string SUPER = "super";
        }

        public static class Ip
        {
            public const string LOCALHOST = "127.0.0.1";
        }
    }
}
