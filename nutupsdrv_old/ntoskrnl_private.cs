using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutupsdrv
{
    public struct EPROCESS
    {
        DISPATCHER_HEADER header;
        PROCESS_BASIC_INFORMATION info;
        BOOL wow64;
    };
}
