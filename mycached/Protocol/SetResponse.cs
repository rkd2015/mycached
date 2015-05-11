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
        public SetResponse()
        {
            this.Header = new ProtocolHeader();
            this.Header.Magic = ProtocolPacket.ResponseMagic;
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
