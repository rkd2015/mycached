using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public class ProtocolException : Exception
    {
        public ResponseStatus Status;
    }
}
