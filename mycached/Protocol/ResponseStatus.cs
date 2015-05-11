using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public enum ResponseStatus
    {
        NoError,
        KeyNotFound,
        KeyExists,
        ValueTooLarge,
        InvalidArguments,
        ItemNotStored,
        NonNumericIncDec,
        UnknownCommand = 0x81,
        OutOfMemory = 0x82
    }
}
