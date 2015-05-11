﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public class GetResponse : ProtocolPacket
    {
        public GetResponse()
        {
            this.Header = new ProtocolHeader();
            this.Header.Magic = ProtocolPacket.ResponseMagic;
        }

        public GetResponse(ProtocolHeader header)
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
