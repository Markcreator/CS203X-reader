/*
Copyright (c) 2023 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace CSLibrary.Structures
{
    internal class CoolLogRegSet
    {
        public static UInt32 Reg0x0403 = 0;
        public static UInt32 Reg0x0404 = 0;
        public static UInt32 Reg0x0405 = 0;
        public static UInt32 Reg0x0406 = 0;
        public static UInt32 Reg0x0407 = 0;
        public static UInt32 Reg0x0907 = 0;
        public static UInt32 Reg0x0908 = 0;
        public static UInt32 Reg0x0909 = 0;
        public static UInt32 Reg0x090A = 0;
        public static UInt32 Reg0x090B = 0;
        public static UInt32 Reg0x090C = 0;
        public static UInt32 Reg0x090D = 0;
        public static UInt32 Reg0x090E = 0;
        public static UInt32 Reg0x090F = 0;
        public static UInt32 Reg0x0B16 = 0;
        public static UInt32 Reg0x0B17 = 0;
        public static UInt32 Reg0x0B18 = 0;
    }

    /// <summary>
    /// Enable / Disable Switch
    /// </summary>
    public enum EnableSwitch
    {
        /// <summary>
        /// Disable option
        /// </summary>
        Disable = 0,
        /// <summary>
        /// Enable option
        /// </summary>
        Enable = 1
    };

    /// <summary>
    /// Pass / Fail
    /// </summary>
    public enum ErrorBit
    {
        /// <summary>
        /// Pass
        /// </summary>
        OK = 0,
        /// <summary>
        /// Fail
        /// </summary>
        Error = 1
    };

    /// <summary>
    /// Password Level
    /// </summary>
    public enum PasswordLevel
    {
        /// <summary>
        /// System Password
        /// </summary>
        System = 1,
        /// <summary>
        /// Application Password
        /// </summary>
        Application = 2,
        /// <summary>
        /// Measurement Password
        /// </summary>
        Measurement = 3
    }

    /// <summary>
    /// Store Rule
    /// </summary>
    public enum StorageRule

    {
        /// <summary>
        /// Not save new data
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Save new data
        /// </summary>
        Rolling = 1
    }

    /// <summary>
    /// Logging Form
    /// </summary>
    public enum LoggingForm

    {
        Dense           = 0,
        OutOfLimit      = 1,
        LimitsCrossing  = 3,
        IntExt1         = 5,
        IntExt2         = 6,
        IntExt1_2       = 7
    }

    /// <summary>
    /// Sensor Type
    /// </summary>
    public enum Sensor
    {
        Temperature = 0,
        External1,
        External2,
        BatteryVoltage
    }

    /// <summary>
    /// Feedback resistor value
    /// </summary>
    public enum FeedbackResistor
    {
        /// <summary>
        /// Resistor 3875K
        /// </summary>
	    R3875K  = 0x01,
        /// <summary>
        /// Resistor 1875K
        /// </summary>
        R1875K = 0x02,
        /// <summary>
        /// Resistor 875K
        /// </summary>
        R875K = 0x04,
        /// <summary>
        /// Resistor 400K
        /// </summary>
        R400K = 0x08,
        /// <summary>
        /// Resistor 185K
        /// </summary>
        R185K = 0x10
    };

    /// <summary>
    /// External sensor 1 type
    /// </summary>
    public enum Ext1SensorType
    {
        /// <summary>
        /// Liner Resistive Sensor
        /// </summary>
        LinerResistiveSensor = 0,
        /// <summary>
        /// High Impedance Input
        /// </summary>
        HighImpedanceInput = 1,
        /// <summary>
        /// Capacitive or resistive sensor without DC (AC signal on EXC pin)
        /// </summary>
        ACExcitation = 3
    };

    /// <summary>
    /// External sensor 2 type
    /// </summary>
    public enum Ext2SensorType
    {
        /// <summary>
        /// Liner Conductive Sensor
        /// </summary>
        LinerConductiveSensor = 0,
        /// <summary>
        /// High Impedance Input
        /// </summary>
        HighImpedanceInput = 1
    };

    /// <summary>
    /// Battery Type
    /// </summary>
    public enum BatteryType
    {
        /// <summary>
        /// 1.5V
        /// </summary>
        B1_5V = 0,
        /// <summary>
        /// 3V
        /// </summary>
        B3V = 1
    };

    /// <summary>
    /// Access FIFO Subcommand
    /// </summary>
    public enum FIFOSubcommand
    {
        /// <summary>
        /// Read data from FIFO
        /// </summary>
        ReadData = 4,
        /// <summary>
        /// Write data to FIFO
        /// </summary>
        WriteData = 5,
        /// <summary>
        /// Read status register
        /// </summary>
        ReadStatus = 6
    }

    /// <summary>
    /// CLSetPasswordParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLSetPasswordParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLSetPasswordParms()
        {
            // NOP
        }
        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0403;
            }
        }
        public UInt32 InternalRegister2
        {
            get
            {
                return CoolLogRegSet.Reg0x0404;
            }
        }
        /// <summary>
        /// Password Level
        /// </summary>
        public PasswordLevel PasswordLevel
        {
            set
            {
                CoolLogRegSet.Reg0x0403 = (UInt32)value & 0x03;
            }
        }
        /// <summary>
        /// Password
        /// </summary>
        public UInt32 Password
        {
            set
            {
                CoolLogRegSet.Reg0x0404 = value;
            }
        }
    }

    /// <summary>
    /// CLSetLogModeParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLSetLogModeParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLSetLogModeParms()
        {
            // NOP
        }
        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0405;
            }
        }

        public LoggingForm LoggingForm
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0xe00000U; // clear bit 21-23
                CoolLogRegSet.Reg0x0405 |= (UInt32)value << 21;
            }
        }
        /// <summary>
        /// Storage Rule (0 or 1)
        /// </summary>
        public StorageRule StorageRule
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0x100000U;  // clear bit 20
                CoolLogRegSet.Reg0x0405 |= ((UInt32)value & 0x1) << 20;
            }
        }

        /// <summary>
        /// External 1 sensor enable
        /// </summary>
        public EnableSwitch Ext1SensorEnable
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0x080000U; // clear bit 19
                CoolLogRegSet.Reg0x0405 |= ((UInt32)value & 0x1) << 19;
            }
        }

        /// <summary>
        /// External 2 sensor enable
        /// </summary>
        public EnableSwitch Ext2SensorEnable
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0x040000U; // clear bit 18
                CoolLogRegSet.Reg0x0405 |= ((UInt32)value & 0x1) << 18;
            }
        }

        /// <summary>
        /// Temp sensor enable
        /// </summary>
        public EnableSwitch TempSensorEnable
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0x020000U; // clear bit 17
                CoolLogRegSet.Reg0x0405 |= ((UInt32)value & 0x1) << 17;
            }
        }

        /// <summary>
        /// Battery Check
        /// </summary>
        public EnableSwitch BatteryCheckEnable
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0x010000U; // clear bit 16
                CoolLogRegSet.Reg0x0405 |= ((UInt32)value & 0x1) << 16;
            }
        }

        /// <summary>
        /// Log interval seconds (0-32767)
        /// </summary>
        public UInt16 LogInterval
        {
            set
            {
                CoolLogRegSet.Reg0x0405 &= ~0x00fffeU; // clear bit 1-15
                CoolLogRegSet.Reg0x0405 |= ((UInt32)value & 0x7fff) << 1;
            }
        }
    }

    /// <summary>
    /// CLSetLogLimitsParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLSetLogLimitsParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLSetLogLimitsParms()
        {
            // NOP
        }
        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0406;
            }
        }

        public UInt32 InternalRegister2
        {
            get
            {
                return CoolLogRegSet.Reg0x0407;
            }
        }

        public UInt16 LowerLimit
        {
            set
            {
                CoolLogRegSet.Reg0x0406 &= ~0x003ffU; // clear bit 0-9
                CoolLogRegSet.Reg0x0406 |= ((UInt32)value & 0x3ff);
            }
        }
        public UInt16 ExtLowerLimit
        {
            set
            {
                CoolLogRegSet.Reg0x0406 &= ~0xffc00U; // clear bit 10-19
                CoolLogRegSet.Reg0x0406 |= ((UInt32)value & 0x3ff) << 10;
            }
        }
        public UInt16 UpperLimit
        {
            set
            {
                CoolLogRegSet.Reg0x0407 &= ~0xffc00U; // clear bit 10-19
                CoolLogRegSet.Reg0x0407 |= ((UInt32)value & 0x3ff) << 10;
            }
        }
        public UInt16 ExtUpperLimit
        {
            set
            {
                CoolLogRegSet.Reg0x0407 &= ~0x003ffU; // clear bit 0-9
                CoolLogRegSet.Reg0x0407 |= ((UInt32)value & 0x3ff);
            }
        }
    }
    /// <summary>
    /// CLSetSFEParaParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLSetSFEParaParms
    {
        public CLSetSFEParaParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0907;
            }
        }

        public UInt32 SFEParameters
        {
            set
            {
                CoolLogRegSet.Reg0x0907 = value;
            }
        }
        /// <summary>
        /// Resistor feedback ladder
        /// </summary>
        public FeedbackResistor Rang
        {
            set
            {
                CoolLogRegSet.Reg0x0907 &= ~0xf800U; // clear bit 11-15
                CoolLogRegSet.Reg0x0907 |= ((UInt32)value & 0x1f) << 11;
            }
        }
        /// <summary>
        /// Current Source value (0.25uA pre step)
        /// </summary>
        public byte Seti
        {
            set
            {
                CoolLogRegSet.Reg0x0907 &= ~0x07c0U; // clear bit 6-10
                CoolLogRegSet.Reg0x0907 |= ((UInt32)value & 0x1f) << 6;
            }
        }
        /// <summary>
        /// Extenal sensor 1 type
        /// </summary>
        public Ext1SensorType EXT1
        {
            set
            {
                CoolLogRegSet.Reg0x0907 &= ~0x0030U; // clear bit 4-5
                CoolLogRegSet.Reg0x0907 |= ((UInt32)value & 0x03) << 4;
            }
        }
        /// <summary>
        /// Extenal sensor 2 type
        /// </summary>
        public Ext2SensorType EXT2
        {
            set
            {
                CoolLogRegSet.Reg0x0907 &= ~0x0008U; // clear bit 3
                CoolLogRegSet.Reg0x0907 |= ((UInt32)value & 0x01) << 3;
            }
        }
        /// <summary>
        /// Range preset
        /// </summary>
        public EnableSwitch AutorangeDisable
        {
            set
            {
                CoolLogRegSet.Reg0x0907 &= ~0x0004U; // clear bit 2
                CoolLogRegSet.Reg0x0907 |= ((UInt32)value & 0x01) << 2;
            }
        }
        /// <summary>
        /// Verify Sensor ID (0 - 3)
        /// </summary>
        public byte VerigfySensorID
        {
            set
            {
                CoolLogRegSet.Reg0x0907 &= ~0x0003U; // clear bit 0-1
                CoolLogRegSet.Reg0x0907 |= ((UInt32)value & 0x03);
            }
        }
    }
    /// <summary>
    /// CLSetCalDataParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLSetCalDataParms
    {
        public CLSetCalDataParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0908;
            }
        }

        public UInt32 InternalRegister2
        {
            get
            {
                return CoolLogRegSet.Reg0x0909;
            }
        }

        public UInt64 CalibrationData
        {
            set
            {
                CoolLogRegSet.Reg0x0908 = (UInt32)value;
                CoolLogRegSet.Reg0x0909 = (UInt32)(value >> 32);
            }
        }
    }
    /// <summary>
    /// CLStartLogParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLStartLogParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLStartLogParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x090A;
            }
        }

        public UInt32 StartTime
        {
            set
            {
                CoolLogRegSet.Reg0x090A = value;
            }
        }
        /// <summary>
        /// Year (0-63)
        /// </summary>
        public byte Year
        {
            set
            {
                CoolLogRegSet.Reg0x090A &= 0x3FFFFFF;    // clear bit 26-31
                CoolLogRegSet.Reg0x090A |= ((UInt32)value & 0x3f) << 26;
            }
        }
        /// <summary>
        /// Month (1-12)
        /// </summary>
        public byte Month
        {
            set
            {
                CoolLogRegSet.Reg0x090A &= 0xFC3FFFFF;    // clear bit 22-25
                CoolLogRegSet.Reg0x090A |= ((UInt32)value & 0x0f) << 22;
            }
        }
        /// <summary>
        /// Day (1-31)
        /// </summary>
        public byte Day
        {
            set
            {
                CoolLogRegSet.Reg0x090A &= 0xFFC1FFFF;    // clear bit 17-21
                CoolLogRegSet.Reg0x090A |= ((UInt32)value & 0x1f) << 17;
            }
        }
        /// <summary>
        /// Hour (0-23)
        /// </summary>
        public byte Hour
        {
            set
            {
                CoolLogRegSet.Reg0x090A &= 0xFFFE0FFF;    // clear bit 12-16
                CoolLogRegSet.Reg0x090A |= ((UInt32)value & 0x1f) << 12;
            }
        }
        /// <summary>
        /// Minute (0-59)
        /// </summary>
        public byte Minute
        {
            set
            {
                CoolLogRegSet.Reg0x090A &= 0xFFFFF03F;    // clear bit 6-11
                CoolLogRegSet.Reg0x090A |= ((UInt32)value & 0x3f) << 6;
            }
        }
        /// <summary>
        /// Second (0-59)
        /// </summary>
        public byte Second
        {
            set
            {
                CoolLogRegSet.Reg0x090A &= 0xFFFFFFC0;    // clear bit 0-5
                CoolLogRegSet.Reg0x090A |= ((UInt32)value & 0x3f);
            }
        }
    }

    /// <summary>
    /// CLSetShelfLifeParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLSetShelfLifeParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLSetShelfLifeParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x090C;
            }
        }

        public UInt32 InternalRegister2
        {
            get
            {
                return CoolLogRegSet.Reg0x090D;
            }
        }

        public UInt32 SLBlock0
        {
            set
            {
                CoolLogRegSet.Reg0x090C = value;
            }
        }
        public UInt32 SLBlock1
        {
            set
            {
                value &= 0xfffffffeU;
                CoolLogRegSet.Reg0x090D &= 0x01;
                CoolLogRegSet.Reg0x090D |= value;
            }
        }
    }

    /// <summary>
    /// CLInitParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLInitParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLInitParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x090E;
            }
        }

        /// <summary>
        /// Delay Time
        /// </summary>
        public UInt16 DelayTime
        {
            set
            {
                CoolLogRegSet.Reg0x090E &= ~0x0000fff0U;
                CoolLogRegSet.Reg0x090E |= ((UInt32)value & 0xfff) << 4;
            }
        }
        /// <summary>
        /// Delay Time
        /// </summary>
        public byte DelayMode
        {
            set
            {
                CoolLogRegSet.Reg0x090E &= ~0x00000002U; // clear bit 1
                CoolLogRegSet.Reg0x090E |= ((UInt32)value & 0x01) << 1;
            }
        }
        /// <summary>
        /// Delay Time
        /// </summary>
        public EnableSwitch TimerEnable
        {
            set
            {
                CoolLogRegSet.Reg0x090E &= ~0x00000001U; // clear bit 0
                CoolLogRegSet.Reg0x090E |= (UInt32)value & 01;
            }
        }

        public UInt16 ApplicationData
        {
            set
            {
                CoolLogRegSet.Reg0x090E &= ~0xffff0000U;
                CoolLogRegSet.Reg0x090E |= (UInt32)value << 16;
            }
        }
        public UInt16 NumberOfWordsForApplicationData
        {
            set
            {
                CoolLogRegSet.Reg0x090E &= ~0xff800000U;
                CoolLogRegSet.Reg0x090E |= ((UInt32)value & 0x1ff) << 23;
            }
        }
        public byte BrokenWordPointer
        {
            set
            {
                CoolLogRegSet.Reg0x090E &= ~0x00070000U;
                CoolLogRegSet.Reg0x090E |= ((UInt32)value & 0x07) << 16;
            }
        }
    }

    /// <summary>
    /// CLOpenAreaParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLOpenAreaParms
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CLOpenAreaParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0403;
            }
        }

        public UInt32 InternalRegister2
        {
            get
            {
                return CoolLogRegSet.Reg0x0404;
            }
        }

        public PasswordLevel PasswordLevel
        {
            set
            {
                CoolLogRegSet.Reg0x0403 = (UInt32)value & 0x03;
            }
        }
        public UInt32 Password
        {
            set
            {
                CoolLogRegSet.Reg0x0404 = value;
            }
        }
    }

    /// <summary>
    /// CLAccessFifoParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLAccessFifoParms
    {
        /// <summary>
        /// Get Sensor Value Response Data
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] AccessFIFOData = new byte[8];

        /// <summary>
        /// constructor
        /// </summary>
        public CLAccessFifoParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x0B16;
            }
        }

        public UInt32 InternalRegister2
        {
            get
            {
                return CoolLogRegSet.Reg0x0B17;
            }
        }

        public UInt32 InternalRegister3
        {
            get
            {
                return CoolLogRegSet.Reg0x0B18;
            }
        }

        public FIFOSubcommand Subcommand
        {
            set
            {
                CoolLogRegSet.Reg0x0B16 &= (UInt32)~0xe0U;
                CoolLogRegSet.Reg0x0B16 |= (UInt32)value << 5;
            }
        }
        public byte PayloadLen
        {

            set
            {
                CoolLogRegSet.Reg0x0B16 &= (UInt32)~0x0fU;
//                Reg0x0B16 |= (UInt32)0x10U;
                if (value > 15)
                    value = 15;

                CoolLogRegSet.Reg0x0B16 |= value;
            }
        }

        public UInt64 Payload
        {
            set
            {
                CoolLogRegSet.Reg0x0B17 = (UInt32)value;
                CoolLogRegSet.Reg0x0B18 = (UInt32)(value >> 32);
            }
        }
        public byte Payload0
        {
            set
            {
                CoolLogRegSet.Reg0x0B17 &= 0xffffff00;
                CoolLogRegSet.Reg0x0B17 |= (UInt32)value;
            }
        }
        public byte Payload1
        {
            set
            {
                CoolLogRegSet.Reg0x0B17 &= 0xffff00ff;
                CoolLogRegSet.Reg0x0B17 |= (UInt32)value << 8;
            }
        }
        public byte Payload2
        {
            set
            {
                CoolLogRegSet.Reg0x0B17 &= 0xff00ffff;
                CoolLogRegSet.Reg0x0B17 |= (UInt32)value << 16;
            }
        }
        public byte Payload3
        {
            set
            {
                CoolLogRegSet.Reg0x0B17 &= 0x00ffffff;
                CoolLogRegSet.Reg0x0B17 |= (UInt32)value << 24;
            }
        }
        public byte Payload4
        {
            set
            {
                CoolLogRegSet.Reg0x0B18 &= 0xffffff00;
                CoolLogRegSet.Reg0x0B18 |= (UInt32)value;
            }
        }
        public byte Payload5
        {
            set
            {
                CoolLogRegSet.Reg0x0B18 &= 0xffff00ff;
                CoolLogRegSet.Reg0x0B18 |= (UInt32)value << 8;
            }
        }
        public byte Payload6
        {
            set
            {
                CoolLogRegSet.Reg0x0B18 &= 0xff00ffff;
                CoolLogRegSet.Reg0x0B18 |= (UInt32)value << 16;
            }
        }
        public byte Payload7
        {
            set
            {
                CoolLogRegSet.Reg0x0B18 &= 0x00ffffff;
                CoolLogRegSet.Reg0x0B18 |= (UInt32)value << 24;
            }
        }
        public UInt64 PayloadInReg
        {
            get
            {
                UInt64 value;

                value  = (UInt64)AccessFIFOData[7] << 56;
                value |= (UInt64)AccessFIFOData[6] << 48;
                value |= (UInt64)AccessFIFOData[5] << 40;
                value |= (UInt64)AccessFIFOData[4] << 32;
                value |= (UInt64)AccessFIFOData[3] << 24;
                value |= (UInt64)AccessFIFOData[2] << 16;
                value |= (UInt64)AccessFIFOData[1] << 8;
                value |= (UInt64)AccessFIFOData[0];

                return value;
            }
        }
        public byte PayloadInReg0
        {
            get
            {
                return AccessFIFOData[0];
            }
        }
        public byte PayloadInReg1
        {
            get
            {
                return AccessFIFOData[1];
            }
        }
        public UInt64 PayloadInReg2
        {
            get
            {
                return AccessFIFOData[2];
            }
        }
        public UInt64 PayloadInReg3
        {
            get
            {
                return AccessFIFOData[3];
            }
        }
        public UInt64 PayloadInReg4
        {
            get
            {
                return AccessFIFOData[4];
            }
        }
        public UInt64 PayloadInReg5
        {
            get
            {
                return AccessFIFOData[5];
            }
        }
        public UInt64 PayloadInReg6
        {
            get
            {
                return AccessFIFOData[6];
            }
        }
        public UInt64 PayloadInReg7
        {
            get
            {
                return AccessFIFOData[7];
            }
        }
    }

    /// <summary>
    /// Get Mesurement Setup Response Data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLGetMesurementSetupParms
    {
//        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte [] MesurementSetupData = new byte [16];
        /// <summary>
        /// 
        /// </summary>
        public CLGetMesurementSetupParms()
        {
            // NOP
        }
        /// <summary>
        /// Get Response Data
        /// </summary>
        public UInt32 StartTime
        {
            get 
            {
                UInt32 value;
                
                value  = (UInt32)MesurementSetupData[0] << 24;
                value |= (UInt32)MesurementSetupData[1] << 16;
                value |= (UInt32)MesurementSetupData[2] << 8;
                value |= (UInt32)MesurementSetupData[3];

                return (value); 
            }
        }

        public UInt16 ExtLowerLimit
        {
            get
            {
                UInt32 value;

                value  = (UInt32)MesurementSetupData[4] << 8;
                value |= (UInt32)MesurementSetupData[5];
                value = value >> 6;

                return (UInt16)value;
            }
        }

        public UInt16 LowerLimit
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[5] << 8;
                value |= (UInt32)MesurementSetupData[6];
                value = (value >> 4) & 0x3ff;

                return (UInt16)value;
            }
        }
        
        public UInt16 UpperLimit
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[6] << 8;
                value |= (UInt32)MesurementSetupData[7];
                value = (value >> 2) & 0x3ff;

                return (UInt16)value;
            }
        }
        
        public UInt16 ExtUpperLimit
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[7] << 8;
                value |= (UInt32)MesurementSetupData[8];
                value &= 0x3ff;

                return (UInt16)value;
            }
        }

        public byte LoggingForm
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[9];
                value = value >> 5;

                return (byte)value;
            }
        }

        public byte StorageRule
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[9];
                value = (value >> 4) & 0x01;

                return (byte)value;
            }
        }

        public EnableSwitch Ext1SensorEnable
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[9];
                value = (value >> 3) & 0x01;

                if (value == 0)
                    return EnableSwitch.Disable;

                return EnableSwitch.Enable;
            }
        }

        public EnableSwitch Ext2SensorEnable
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[9];
                value = (value >> 2) & 0x01;

                if (value == 0)
                    return EnableSwitch.Disable;

                return EnableSwitch.Enable;
            }
        }

        public EnableSwitch TempSensorEnable
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[9];
                value = (value >> 1) & 0x01;

                if (value == 0)
                    return EnableSwitch.Disable;

                return EnableSwitch.Enable;
            }
        }

        public EnableSwitch BatteryCheckEnable
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[9];
                value = value & 0x01;

                if (value == 0)
                    return EnableSwitch.Disable;

                return EnableSwitch.Enable;
            }
        }

        public byte LogInterval
        {
            get
            {
                UInt32 value;

                value  = (UInt32)MesurementSetupData[10] << 8;
                value |= (UInt32)MesurementSetupData[11];
                value  = value >> 1;

                return (byte)value;
            }
        }

        public UInt16 DelayTime
        {
            get
            {
                UInt32 value;

                value  = (UInt32)MesurementSetupData[12] << 8;
                value |= (UInt32)MesurementSetupData[13];
                value  = value >> 4;

                return (UInt16)value;
            }
        }

        public byte DelayMode
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[13];
                value = (value >> 1) & 0x01;

                return (byte)value;
            }
        }

        public EnableSwitch TimerEnable
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[13];
                value = value & 0x01;

                if (value == 0)
                    return EnableSwitch.Disable;

                return EnableSwitch.Enable;
            }
        }

        public UInt16 NumberOfWorldForAppData
        {
            get
            {
                UInt32 value;

                value  = (UInt32)MesurementSetupData[14] << 8;
                value |= (UInt32)MesurementSetupData[15];
                value = value >> 7;

                return (UInt16)value;
            }
        }

        public byte BrokeWordPointer
        {
            get
            {
                UInt32 value;

                value = (UInt32)MesurementSetupData[15];
                value = value & 0x07;

                return (byte)value;
            }
        }
    }

    /// <summary>
    /// Get Log State
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLGetLogStateParms
    {
        /// <summary>
        /// Response Data
        /// </summary>
        public byte[] LogStateData = new byte[20];

        public EnableSwitch ShelfLifeFlag
        {
            set
            {
                CoolLogRegSet.Reg0x090D = (UInt32)value;
//                CoolLogRegSet.Reg0x090D &= ~0x00000001U;
//                CoolLogRegSet.Reg0x090D |= (UInt32)value;
            }
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x090D;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CLGetLogStateParms()
        {
            // NOP
        }
        /// <summary>
        /// 
        /// </summary>
        public byte ExtLower
        {
            get
            {
                return LogStateData[0];
            }
        }

        public byte Lower
        {
            get
            {
                return LogStateData[1];
            }
        }

        public byte Upper
        {
            get
            {
                return LogStateData[2];
            }
        }

        public byte ExtUpper
        {
            get
            {
                return LogStateData[3];
            }
        }

        /// <summary>
        /// Measurement Address Pointer
        /// </summary>
        public UInt16 MeasurementAddressPointer
        {
            get
            {
                UInt32 value;

                value  = (UInt32)LogStateData[4] << 8;
                value |= (UInt32)LogStateData[5];
                value  = value >> 7 ;

                return (UInt16)value;
            }
        }
        /// <summary>
        /// Number Of Memory Replacements
        /// </summary>
        public byte NumberOfMemoryReplacements
        {
            get
            {
                return (byte)(LogStateData[5] & 0x3f);
            }
        }

        /// <summary>
        /// Number Of Measurements
        /// </summary>
        public UInt16 NumberOfMeasurements
        {
            get
            {
                UInt32 value;

                value = (UInt32)LogStateData[6] << 8;
                value |= (UInt32)LogStateData[7];
                value = value >> 1;

                return (UInt16)value;
            }
        }

        public byte Active
        {
            get
            {
                return (byte)(LogStateData[7] & 0x01);
            }
        }

        public byte LoggingProcessActive
        {
            get
            {
                int pos = 19;

                return (byte)(LogStateData[pos] >> 7);
            }
        }

        public byte MeasurementAreaFull
        {
            get
            {
                int pos = 19;

                return (byte)((LogStateData[pos] >> 6) & 0x01);
            }
        }

        public byte MeasurementOverWritten
        {
            get
            {
                int pos = 19;

                return (byte)((LogStateData[pos] >> 5) & 0x01);
            }
        }

        /// <summary>
        /// A/D Error
        /// </summary>
        public ErrorBit ADErr
        {
            get
            {
                int pos = 19;

                if (((LogStateData[pos] >> 4) & 0x01) == 0)
                    return ErrorBit.OK;

                return ErrorBit.Error;
            }
        }

        public byte LowBattery
        {
            get
            {
                int pos = 19;

                return (byte)((LogStateData[pos] >> 3) & 0x01);
            }
        }

        /// <summary>
        /// Shelf Life High Error
        /// </summary>
        public ErrorBit ShelfLifeHighError
        {
            get
            {
                int pos = 19;

                if (((LogStateData[pos] >> 2) & 0x01) == 0)
                    return ErrorBit.OK;

                return ErrorBit.Error;
            }
        }

        public ErrorBit ShelfLifeLowError
        {
            get
            {
                int pos = 19;

                if (((LogStateData[pos] >> 1) & 0x01) == 0)
                    return ErrorBit.OK ;

                return ErrorBit.Error;
            }
        }

        public ErrorBit ShelfLifeExpired
        {
            get
            {
                int pos = 19;

                if ((LogStateData[pos] & 0x01) == 0)
                    return ErrorBit.OK;

                return ErrorBit.Error;
            }
        }
    }
    /// <summary>
    /// Read TID structures, configure this before read current TID
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLGetCalDataParms
    {
        /// <summary>
        /// Response Data
        /// </summary>
        public byte[] CalData = new byte[9];
        /// <summary>
        /// constructor
        /// </summary>
        public CLGetCalDataParms()
        {
            // NOP
        }
    }
    /// <summary>
    /// CLGetBatLvParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLGetBatLvParms
    {
        /// <summary>
        /// Get Battery Level Response Data
        /// </summary>
        public byte[] BatLvData = new byte[2];

        /// <summary>
        /// constructor
        /// </summary>
        public CLGetBatLvParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x090B;
            }
        }

        public byte Retrigger
        {
            set
            {
                CoolLogRegSet.Reg0x090B = (UInt32)value & 0x01;
            }
        }

        public ErrorBit ADError
        {
            get 
            {
                if (((BatLvData[0] >> 7) & 0x01) == 0)
                    return ErrorBit.OK; 

                return ErrorBit.Error; 
            }
        }

        public BatteryType BatteryType
        {
            get
            {
                if (((BatLvData[0] >> 6) & 0x01) == 0)
                    return BatteryType.B1_5V;

                return BatteryType.B3V;
            }
        }

        /// <summary>
        /// Battery A/D Value (0-1023)
        /// </summary>
        public UInt16 BatteryValue
        {
            get
            {
                UInt32 value;

                value  = (UInt32)BatLvData[0] << 8;
                value |= (UInt32)BatLvData[1];
                value  = value & 0x3ff;

                return (UInt16)(value);
            }
        }

        /// <summary>
        /// Battery volatge (V)
        /// </summary>
        public double BatteryLevel
        {
            get
            {
                if (((BatLvData[0] >> 6) & 0x01) == 0)
                    return (BatteryValue * 1.5 / 1024);

                return (BatteryValue * 3.0 / 1024);
            }
        }
    }

    /// <summary>
    /// CLGetSensorValueParms
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CLGetSensorValueParms
    {
        /// <summary>
        /// Get Sensor Value Response Data
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] SensorValueData = new byte[2];

        public CLGetSensorValueParms()
        {
            // NOP
        }

        public UInt32 InternalRegister1
        {
            get
            {
                return CoolLogRegSet.Reg0x090F;
            }
        }

        /// <summary>
        /// Sensor Type
        /// </summary>
        public Sensor SensorType
        {
            set
            {
                CoolLogRegSet.Reg0x090F = (UInt32)value;
            }
        }

        /// <summary>
        /// A/D Error
        /// </summary>
        public ErrorBit ADError
        {
            get
            {
                if ((SensorValueData[0] >> 7 & 0x01) == 0)
                    return ErrorBit.OK;

                return ErrorBit.Error;
            }
        }

        /// <summary>
        /// RangeLimit (0-31)
        /// </summary>
        public byte RangeLimit
        {
            get
            {
                return (byte)(SensorValueData[0] >> 2 & 0x1f);
            }
        }

        /// <summary>
        /// Sensor Value (0-1023)
        /// </summary>
        public UInt16 SensorValue
        {
            get
            {
                UInt32 value;

                value = (UInt32)SensorValueData[0] << 8;
                value |= SensorValueData[1];
                value  = value & 0x3ff;


                return (UInt16)(value);
            }
        }
    }

    /// <summary>
    /// QT Command Parameter
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class QTCommandParms
    {
        /// <summary>
        /// Read / Write, 
        /// 0: read the QT control bits in cache
        /// 1: write the QT control bits
        /// </summary>
        public int RW;

        /// <summary>
        /// written to nonvolatile (NVM) or volatile (DFF) memory
        /// 0: write to DFF memory
        /// 1: write to NVM memory
        /// </summary>
        public int TP;

        /// <summary>
        /// Reduces Range if in or about to be in OPEN or SECURED state
        /// 0: Tag does not reduce range
        /// 1: Tag reduces range if in or about to be in OPEN or SECURED state
        /// </summary>
        public int SR;

        /// <summary>
        /// 0: Tag uses Reveal Memory Map (Private Mode)
        /// 1: Tag uses Conceal Memory Map (Public Mode)
        /// </summary>
        public int MEM;

        /// <summary>
        /// Kill/Access Password
        /// </summary>
        public UInt32 accessPassword;

        /// <summary>
        /// constructor
        /// </summary>
        public QTCommandParms()
        {
            // NOP
        }

    }
    /// <summary>
    /// G2 Config Parameter
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class G2ConfigParms
    {
        /// <summary>
        /// Temper Alarm Flag
        /// </summary>
        public bool TempetrAlarm = false;

        /// <summary>
        /// External Supply Flag or Input Signal
        /// </summary>
        public bool ExternalSupply = false;

        /// <summary>
        /// Invert Digital Output
        /// </summary>
        public bool InvertDigitalOutput = false;

        /// <summary>
        /// Transparent Mode On/Off
        /// </summary>
        public bool TransparentMode = false;

        /// <summary>
        /// Transparent Mode Data/Raw
        /// </summary>
        public bool TransparentModeData = false;

        /// <summary>
        /// Max. Backscatter Strength
        /// </summary>
        public bool MaxBackscatterStrength = false;

        /// <summary>
        /// Digital Output
        /// </summary>
        public bool DigitalOutput = false;

        /// <summary>
        /// Read Range Reduction On/Off
        /// </summary>
        public bool ReadRangeReduction = false;

        /// <summary>
        /// Read Protect EPC Bank
        /// </summary>
        public bool ReadProtectEPC = false;

        /// <summary>
        /// Read Protect TID
        /// </summary>
        public bool ReadProtectTID = false;

        /// <summary>
        /// PSF Alarm Flag
        /// </summary>
        public bool PSFAlarm = false;

        /// <summary>
        /// Kill/Access Password
        /// </summary>
        public UInt32 accessPassword = 0;

        /// <summary>
        /// constructor
        /// </summary>
        public G2ConfigParms()
        {
            // NOP
        }
    }

    public class ChangeEASParms
    {
        /// <summary>
        /// Access Password
        /// </summary>
        public UInt32 accessPassword = 0;

        /// <summary>
        /// Retry Count
        /// </summary>
        public uint retryCount = 7;

        /// <summary>
        /// Enable/Disable EAS
        /// </summary>
        public bool enableEAS = false;
    }

}
