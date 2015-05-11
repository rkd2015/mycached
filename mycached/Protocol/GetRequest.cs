using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public class GetRequest : ProtocolPacket
    {
        public GetRequest(CommandOpCode opCode = CommandOpCode.Get)
        {
            this.Header = new ProtocolHeader();
            this.Header.OpCode = opCode;
            this.Header.Magic = ProtocolPacket.RequestMagic;
        }

        public GetRequest(ProtocolHeader header)
            :base(header)
        {
            this.Header.Magic = ProtocolPacket.RequestMagic;
        }

        public override bool Validate()
        {
            // MUST NOT have extras.
            // MUST have key.
            // MUST NOT have value.

            if (this.Header.ExtrasLength != 0 ||
                String.IsNullOrEmpty(this.Key) ||
                !String.IsNullOrEmpty(this.Value))
            {
                return false;
            }

            return true;
        }
    }
}
