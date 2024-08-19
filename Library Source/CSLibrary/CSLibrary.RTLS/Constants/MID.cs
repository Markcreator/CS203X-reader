using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    enum MID : byte
    {
        GetVersion = 0x0,
        KeepAlive = 0x10,
        StartFirmwareUpgrade = 0x11,
        EnterFirmwareUpgrade = 0x1A,
        ClearAllRegisteredTags = 0x13,
        ConfirmRejectRegistration = 0x1C,
        TagAnchorSearch = 0x21,
        TagAnchorSearchNtf = 0x92,
        AdhocBeacon = 0x22,
        UDControl = 0x23,
        LedControl = 0x26,
        KeepAliveNtf = 0x84,
        PowerUpVersionRequestNtf = 0x88,
        TagPositionNtf = 0x93
    }
}
