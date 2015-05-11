using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public class ErrorResponse : ProtocolPacket
    {
        public ErrorResponse()
        {
            this.Header = new ProtocolHeader();
            this.Header.Magic = ProtocolPacket.ResponseMagic;
        }

        public ErrorResponse(ProtocolHeader header)
            :base(header)
        {
            this.Header.Magic = ProtocolPacket.ResponseMagic;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
