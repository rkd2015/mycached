using mycached.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Storage
{
    public enum CacheStatus
    {
        NoError = 0,
        KeyDoesNotExist,
        CasDosentMatch
    }

    public class MemCache
    {
        private Dictionary<string, CacheRecord>[] cacheTable;

        public MemCache()
        {
            this.cacheTable = new Dictionary<string, CacheRecord>[Configuration.CacheTableSize];

            for(int i=0; i<Configuration.CacheTableSize; i++)
            {
                this.cacheTable[i] = new Dictionary<string, CacheRecord>();
            }
        }

        public CacheStatus Get(string key, out string value, out UInt32 flags)
        {
            int tableSlot = key.GetHashCode() % Configuration.CacheTableSize;
            CacheStatus status = CacheStatus.NoError;

            lock (this.cacheTable[tableSlot])
            {
                CacheRecord record = null;

                if (this.cacheTable[tableSlot].TryGetValue(key, out record))
                {
                    value = record.Value;
                    flags = record.Flags;
                }
                else
                {
                    status = CacheStatus.KeyDoesNotExist;
                    value = String.Empty;
                    flags = 0;
                }
            }

            return status;
        }

        public CacheStatus Set(string key, string value, UInt32 flags, UInt32 expiry, UInt64 cas)
        {
            int tableSlot = key.GetHashCode() % Configuration.CacheTableSize;
            CacheStatus status = CacheStatus.NoError;

            lock (this.cacheTable[tableSlot])
            {
                CacheRecord record = null;

                if (!this.cacheTable[tableSlot].TryGetValue(key, out record))
                {
                    record = new CacheRecord{ Key = key };
                    this.cacheTable[tableSlot] [key] = record;
                }

                if (cas == record.CAS)
                {
                    record.Value = value;
                    record.Flags = flags;
                    record.Expiry = expiry;
                    record.CAS = cas;
                }
                else
                {
                    status = CacheStatus.CasDosentMatch;
                }
            }

            return status;
        }
    }
}
