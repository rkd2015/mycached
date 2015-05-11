using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public abstract class ProtocolPacket
    {
        public const Byte RequestMagic = 0x80;
        public const Byte ResponseMagic = 0x81;

        public ProtocolHeader  Header;
        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
                this.Header.KeyLength = (ushort)this.key.Length;
            }
        }

        public string Value;
        private string key;

        public ProtocolPacket()
        {
        }

        public ProtocolPacket(ProtocolHeader header)
        {
            this.Header = header;
        }
        public static ProtocolPacket ReadRequest(BinaryReader reader)
        {
            return ProtocolPacket.ReadRequest(reader, ProtocolHeader.ReadHeader(reader));
        }

        public static ProtocolPacket ReadRequest(BinaryReader reader, ProtocolHeader header)
        {
            ProtocolPacket packet = null;

            switch(header.OpCode)
            {
                case CommandOpCode.Get:
                case CommandOpCode.GetK:
                case CommandOpCode.GetKQ:
                    if (header.Magic == ProtocolPacket.RequestMagic)
                    {
                        packet = new GetRequest(header);
                    }
                    else if (header.Magic == ProtocolPacket.ResponseMagic)
                    {
                        packet = new GetResponse(header);
                    }
                    break;

                case CommandOpCode.Set:
                case CommandOpCode.SetQ:
                    if (header.Magic == ProtocolPacket.RequestMagic)
                    {
                        packet = new SetRequest(header);
                    }
                    else if (header.Magic == ProtocolPacket.ResponseMagic)
                    {
                        packet = new SetResponse(header);
                    }
                    break;
            }

            if (packet != null)
            {
                packet.Read(reader);

                if (!packet.Validate())
                {
                    throw new ProtocolException() { Status = ResponseStatus.InvalidArguments };
                }
            }
            else
            {
                throw new ProtocolException() { Status = ResponseStatus.UnknownCommand };
            }

            return packet;
        }

        public void Read(BinaryReader reader)
        {
            this.key = Encoding.Default.GetString(reader.ReadBytes(this.Header.KeyLength));
            int valueLength = (int)(this.Header.TotalBodyLength - (uint)this.Header.KeyLength - (uint)this.Header.ExtrasLength);
            this.Value = Encoding.Default.GetString(reader.ReadBytes(valueLength));
        }

        public void Write(BinaryWriter writer)
        {
            this.Header.Write(writer);

            if (this.Key.Length != 0)
            {
                byte[] tempArray = new byte[this.Key.Length];

                Encoding.Default.GetBytes(this.Key, 0, this.Key.Length, tempArray, 0);
                writer.Write(tempArray);
            }

            if (this.Value.Length != 0)
            {
                byte[] tempArray = new byte[this.Value.Length];

                Encoding.Default.GetBytes(this.Value, 0, this.Value.Length, tempArray, 0);
                writer.Write(tempArray);
            }
        }

        public abstract bool Validate();
    }
}
