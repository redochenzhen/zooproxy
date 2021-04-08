using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.ZooProxy
{
    [Flags]
    public enum Mode
    {
        Persistent = 2,
        PersistentSequential = Persistent | ModeExts.SEQUENTIAL,
        Ephemeral = 4,
        EphemeralSequential = Ephemeral | ModeExts.SEQUENTIAL
    }

    public static class ModeExts
    {
        public const int SEQUENTIAL = 1;

        public static bool IsPersistent(this Mode mode)
        {
            return (mode & Mode.Persistent) == Mode.Persistent;
        }

        public static bool IsSequential(this Mode mode)
        {
            return ((int)mode & SEQUENTIAL) == SEQUENTIAL;
        }

        public static CreateMode ToCreateMode(this Mode mode)
        {
            switch (mode)
            {
                case Mode.Ephemeral:
                    return CreateMode.EPHEMERAL;
                case Mode.EphemeralSequential:
                    return CreateMode.EPHEMERAL_SEQUENTIAL;
                case Mode.PersistentSequential:
                    return CreateMode.PERSISTENT_SEQUENTIAL;
                case Mode.Persistent:
                default:
                    return CreateMode.PERSISTENT;
            }
        }
    }
}
