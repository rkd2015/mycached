using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Protocol
{
    public enum CommandOpCode
    {
        Get = 0,
        Set,
        Add,
        Replace,
        Delete,
        Increment,
        Decrement,
        Quit,
        Flush,
        GetQ,
        NoOp,
        Version,
        GetK,
        GetKQ,
        Append,
        Prepend,
        Stat,
        SetQ,
        AddQ,
        ReplaceQ,
        DeleteQ,
        IncrementQ,
        DecrementQ,
        QuitQ,
        FlushQ,
        AppendQ,
        PrependQ
    }
}
