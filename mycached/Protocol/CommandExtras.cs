using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mycached.Utilities;

namespace mycached.Protocol
{
    public class CommandExtras
    {
        public Byte ExtrasLength; 
        public UInt32 Flags;
        public UInt32 Expiry;

        public CommandExtras(Byte extrasLength)
        {
            this.ExtrasLength = extrasLength;
        }

        public void Read(BinaryReader reader)
        {
            if (this.ExtrasLength >= 4)
            {
                this.Flags = reader.ReadUInt32FromBigEndian();

                if (this.ExtrasLength == 8)
                {
                    this.Expiry = reader.ReadUInt32FromBigEndian();
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            if (this.ExtrasLength >= 4)
            {
                writer.WriteBigEndian(this.Flags);

                if (this.ExtrasLength == 8)
                {
                    writer.WriteBigEndian(this.Expiry);
                }
            }
        }
    }
}
