using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mycached.Utilities;

namespace mycached.Protocol
{
    public class ProtocolHeader
    {
        public const int        HeaderLength = 24;
        public Byte             Magic;
        public CommandOpCode    OpCode;
        public UInt16           KeyLength;
        public Byte             ExtrasLength;
        public Byte             DataType;
        public ResponseStatus   Status; // This is reserved in case of requests
        public UInt32           TotalBodyLength;
        public UInt32           Opaque;
        public UInt64           CAS;
        public CommandExtras    Extras;

        public static ProtocolHeader ReadHeader(BinaryReader reader)
        {
            ProtocolHeader header = new ProtocolHeader();

            header.Read(reader);

            return header;
        }

        public void Read(BinaryReader reader)
        {
            this.Magic = reader.ReadByte();
            this.OpCode = (CommandOpCode)reader.ReadByte();
            this.KeyLength = reader.ReadUInt16FromBigEndian();
            this.ExtrasLength = reader.ReadByte();
            this.DataType = reader.ReadByte();
            this.Status = (ResponseStatus)reader.ReadUInt16FromBigEndian();
            this.TotalBodyLength = reader.ReadUInt32FromBigEndian();
            this.Opaque = reader.ReadUInt32FromBigEndian();
            this.CAS = reader.ReadUInt64FromBigEndian();

            if (this.ExtrasLength != 0)
            {
                this.Extras = new CommandExtras(this.ExtrasLength);
                this.Extras.Read(reader);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Magic);
            writer.Write((Byte)this.OpCode);
            writer.WriteBigEndian(this.KeyLength);
            writer.Write(this.ExtrasLength);
            writer.Write(this.DataType);
            writer.WriteBigEndian((UInt16)this.Status);
            writer.WriteBigEndian(this.TotalBodyLength);
            writer.WriteBigEndian(this.Opaque);
            writer.WriteBigEndian(this.CAS);

            if (this.ExtrasLength != 0 && this.Extras != null)
            {
                this.Extras.Write(writer);
            }
        }
    }
}
