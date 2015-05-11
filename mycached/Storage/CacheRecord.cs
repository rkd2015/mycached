using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Storage
{
    public class CacheRecord
    {
        public string Key;
        public string Value;
        public UInt32 Flags;
        public UInt32 Expiry;
        public UInt64 CAS;
    }
}
