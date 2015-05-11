using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public class SetRequest : ProtocolPacket
    {
        public SetRequest(CommandOpCode opCode = CommandOpCode.Set)
        {
            this.Header = new ProtocolHeader();
            this.Header.Magic = ProtocolPacket.RequestMagic;
            this.Header.OpCode = opCode;
        }

        public SetRequest(ProtocolHeader header)
            : base(header)
        {
            this.Header.Magic = ProtocolPacket.RequestMagic;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
