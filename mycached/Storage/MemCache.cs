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

    /// <summary>
    /// Encapsulates the cache. The cache is a table of hash tables. The first level table
    /// serves as a striping unit. The inner level is a dictionary. The lock is at the 
    /// dictionary level.
    /// </summary>
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
            uint tableSlot = ((uint)key.GetHashCode()) % Configuration.CacheTableSize;
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

        public CacheStatus Set(string key, string value, UInt32 flags, UInt32 expiry, UInt64 cas, out UInt64 newCas)
        {
            uint tableSlot = ((uint)key.GetHashCode()) % Configuration.CacheTableSize;
            CacheStatus status = CacheStatus.NoError;
            
            newCas = 0;

            lock (this.cacheTable[tableSlot])
            {
                CacheRecord record = null;

                if (!this.cacheTable[tableSlot].TryGetValue(key, out record))
                {
                    record = new CacheRecord{ Key = key };
                    this.cacheTable[tableSlot] [key] = record;
                }

                if (cas == 0 || cas == record.CAS)
                {
                    record.Value = value;
                    record.Flags = flags;
                    record.Expiry = expiry;
                    record.CAS++;
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
