using mycached.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public class SetResponse : ProtocolPacket
    {
        public SetResponse(ResponseStatus status)
        {
            this.Header = new ProtocolHeader();
            this.Header.Magic = ProtocolPacket.ResponseMagic;
            this.Header.Status = status;
            this.Header.OpCode = CommandOpCode.Set;
        }

        public SetResponse(ProtocolHeader header)
            : base(header)
        {
            this.Header.Magic = ProtocolPacket.ResponseMagic;
        }

        public override bool Validate()
        {
            return true;
        }

    }
}
