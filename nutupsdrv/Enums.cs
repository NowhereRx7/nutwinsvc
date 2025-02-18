using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutupsdrv
{
    public enum UpsState : UInt32
    {
        None = 0,
        Online = 1,
        OnBattery = 2,
        LowBattery = 4,
        NoComm = 8,
        Critical = 16,
    }
}
