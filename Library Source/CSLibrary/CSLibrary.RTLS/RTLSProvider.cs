#define __NATIVE_COM__

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using CSLibrary.RTLS.Structures;
using CSLibrary.RTLS.Constants;
#if __NATIVE_COM__
using CSLibrary.RTLS.Transport;
#endif

namespace CSLibrary.RTLS
{
    /// <summary>
    /// RTLS provider
    /// </summary>
    public class RTLSProvider : IDisposable
    {
        #region Field
#if __NATIVE_COM__
        Port mPort = null;
#else
        SerialPort mPort = null;
#endif
        SelectMask mask = new SelectMask();
        Result result = Result.OK;
        IOperationParms optParms = null;
        DeviceStatus currentStatus = DeviceStatus.Unknown;
        float avg_rssi = 0;
        float avg_distance = 0;
        object syncLock = new object();

        string sbuffer = "";
        Byte[] id = new byte[6];
        Byte[] fwUpgradeBuffer = new byte[0];
        uint fwUpgradeRetry = 0;
        uint startBlockIndex = 0;//Start from 0
        uint totalBlocks = 0;
        uint tagStopCount = 0;
        int currentUpgradePercent = 0;
        byte powerLevel = 0x3f;
        byte BackOff = 1;
        bool updateSuccess = false;
        bool requiredToReset = false;
        bool isIDEmpty = true;
        bool stopOperation = false;
        string msp430Version = "";
        string bootloaderVersion = "";
        Random searchIndexRnd = new Random();
        RollingMinimum rollingMinRssi = null;
        RollingMinimum rollingMinDistance = null;
        MovingAverageCalculator movingAverageRssi = new MovingAverageCalculator(20);
        MovingAverageCalculator movingAverageDistance = new MovingAverageCalculator(20);
        int tagCount = 0;
        int timer = 0;
        bool disposed = false;
        System.Threading.Timer timeout = null;

        /// <summary>
        /// MSP430 Version
        /// </summary>
        public string MSP430Version
        {
            get { return msp430Version; }
        }
        /// <summary>
        /// Bootloader Version
        /// </summary>
        public string BootloaderVersion
        {
            get { return bootloaderVersion; }
        }
        /// <summary>
        /// Device Status
        /// </summary>
        public DeviceStatus DeviceStatus
        {
            get { return currentStatus; }
        }
        bool bStopOperation
        {
            get { lock (syncLock) return stopOperation; }
            set { lock (syncLock) stopOperation = value; }
        }
        uint TagStopCount
        {
            get { lock (syncLock) return tagStopCount; }
            set { lock (syncLock) tagStopCount = value; }
        }
        #endregion

        #region Event
        /// <summary>
        /// PowerUpHandler
        /// </summary>
        public delegate void PowerUpHandler();
        /// <summary>
        /// KeepAliveHandler
        /// </summary>
        public delegate void KeepAliveHandler();
        /// <summary>
        /// GetVersionEventHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void GetVersionEventHandler(GetVersionEventArgs e);
        /// <summary>
        /// TagSearchEventHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void TagSearchEventHandler(TagSearchEventArgs e);
        /// <summary>
        /// AdhocBeaconEventHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void AdhocBeaconEventHandler(ErrorCode e);
        /// <summary>
        /// UDControlNotifyEventHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void UDControlNotifyEventHandler(UDControlArgs e);
        /// <summary>
        /// EnterUpgradeModeHandler
        /// </summary>
        /// <param name="e"></param>
        private delegate void EnterUpgradeModeHandler(ErrorCode e);
        /// <summary>
        /// FirmwareUpgradeHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void FirmwareUpgradeHandler(FirmwareUpgradeArgs e);
        /// <summary>
        /// DeviceStatusHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void DeviceStatusHandler(DeviceStatus e);
        /// <summary>
        /// TagPositionHandler
        /// </summary>
        /// <param name="e"></param>
        public delegate void TagPositionHandler(TagPositionNotifyArgs e);
        /// <summary>
        /// PowerUpNotify
        /// </summary>
        public event PowerUpHandler PowerUpNotify;
        /// <summary>
        /// KeepAliveNotify
        /// </summary>
        public event KeepAliveHandler KeepAliveNotify;
        /// <summary>
        /// GetVersionNotify
        /// </summary>
        public event GetVersionEventHandler GetVersionNotify;
        /// <summary>
        /// TagSearchNotify
        /// </summary>
        public event TagSearchEventHandler TagSearchNotify;
        /// <summary>
        /// AdhocBeaconNotify
        /// </summary>
        public event AdhocBeaconEventHandler AdhocBeaconNotify;
        /// <summary>
        /// UDControlNotify
        /// </summary>
        public event UDControlNotifyEventHandler UDControlNotify;
        /// <summary>
        /// EnterUpgradeModeNotify
        /// </summary>
        private event EnterUpgradeModeHandler EnterUpgradeModeNotify;
        /// <summary>
        /// FirmwareUpgradeNotify
        /// </summary>
        public event FirmwareUpgradeHandler FirmwareUpgradeNotify;
        /// <summary>
        /// DeviceStatusNotify
        /// </summary>
        public event DeviceStatusHandler DeviceStatusNotify;
        /// <summary>
        /// TagPositionNotify
        /// </summary>
        public event TagPositionHandler TagPositionNotify;
        #endregion

        #region ctr/dtr
        /// <summary>
        /// Constructor
        /// </summary>
        public RTLSProvider()
            : this("COM3:")
        {
            timeout = new Timer(new TimerCallback(TimeoutProc), this, Timeout.Infinite, 5000);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">comport number</param>
        public RTLSProvider(string port)
        {
#if __NATIVE_COM__
            BasicPortSettings setting = new BasicPortSettings();
            setting.BaudRate = BaudRates.CBR_115200;
            setting.ByteSize = 8;
            setting.Parity = CSLibrary.RTLS.Transport.Parity.none;
            setting.StopBits = CSLibrary.RTLS.Transport.StopBits.two;
            mPort = new Port(port, setting);
            mPort.DataReceived += new Port.CommEvent(mPort_DataReceived);
            mPort.OnError += new Port.CommErrorEvent(mPort_OnError);
            //mPort.PowerEvent += new Port.CommEvent(mPort_PowerEvent);
            //mPort.TxDone += new Port.CommEvent(mPort_TxDone);

            mPort.Open();
#else
            mPort = new SerialPort(port);
            mPort.BaudRate = 115200;
            mPort.Parity = System.IO.Ports.Parity.None;
            mPort.StopBits = StopBits.Two;
            mPort.ReadTimeout = -1;
            mPort.DiscardNull = true;
            mPort.DataBits = 8;
            mPort.DtrEnable = true;
            mPort.Handshake = Handshake.RequestToSend;
            mPort.RtsEnable = true;
            //mPort.ReceivedBytesThreshold = 1;
            //mPort.Encoding = System.Text.ASCIIEncoding.ASCII;
            mPort.ErrorReceived += new SerialErrorReceivedEventHandler(mPort_ErrorReceived);
            mPort.DataReceived += new SerialDataReceivedEventHandler(mPort_DataReceived);
            mPort.Open();
#endif
            //Try to Get Version, if not power yet, please call this after powerup event
            GetVersion();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~RTLSProvider()
        {
            Dispose(false);
        }
        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
                if (mPort != null)
                {
                    mPort.Close();
                    mPort.Dispose();
                    mPort = null;
                }
                if (timeout != null)
                {
                    timeout.Dispose();
                    timeout = null;
                }
            }
        }
        #endregion

        #region event fire
        /// <summary>
        /// Raise PowerUp notify
        /// </summary>
        protected void OnPowerUpNotify()
        {
            if (PowerUpNotify != null)
            {
                PowerUpNotify();
            }
        }
        /// <summary>
        /// Raise KeepAlive notify
        /// </summary>
        protected void OnKeepAliveNotify()
        {
            if (KeepAliveNotify != null)
            {
                KeepAliveNotify();
            }
        }
        /// <summary>
        /// Raise GetVersion notify
        /// </summary>
        protected void OnGetVersionNotify(GetVersionEventArgs e)
        {
            if (GetVersionNotify != null)
            {
                GetVersionNotify(e);
            }
        }
        /// <summary>
        /// Raise TagSearch notify
        /// </summary>
        protected void OnTagSearchNotify(TagSearchEventArgs e)
        {
            if (TagSearchNotify != null)
            {
                TagSearchNotify(e);
            }
        }
        /// <summary>
        /// Raise AdhocBeacon notify
        /// </summary>
        protected void OnAdhocBeaconNotify(ErrorCode err)
        {
            if (AdhocBeaconNotify != null)
            {
                AdhocBeaconNotify(err);
            }
        }
        /// <summary>
        /// Raise UDControl notify
        /// </summary>
        protected void OnUDControlNotify(UDControlArgs e)
        {
            if (UDControlNotify != null)
            {
                UDControlNotify(e);
            }
        }
        /// <summary>
        /// Raise EnterUpgrade notify
        /// </summary>
        protected void OnEnterUpgradeNotify(ErrorCode err)
        {
            if (EnterUpgradeModeNotify != null)
            {
                EnterUpgradeModeNotify(err);
            }
        }
        /// <summary>
        /// Raise FramwareUpgrade notify
        /// </summary>
        protected void OnFramwareUpgradeNotify(FirmwareUpgradeArgs e)
        {
            if (FirmwareUpgradeNotify != null)
            {
                FirmwareUpgradeNotify(e);
            }
        }
        /// <summary>
        /// Raise DeviceStatus notify
        /// </summary>
        /// <param name="status"></param>
        protected void OnDeviceStatusNotify(DeviceStatus status)
        {
            if (status != currentStatus)
            {
                currentStatus = status;
                if (DeviceStatusNotify != null)
                {
                    DeviceStatusNotify(status);
                }
            }
        }
        /// <summary>
        /// Raise TagPosition notify
        /// </summary>
        /// <param name="e"></param>
        protected void OnTagPositionNotify(TagPositionNotifyArgs e)
        {
            if (TagPositionNotify != null)
            {
                TagPositionNotify(e);
            }
        }
        
        #endregion

        #region Comport Event
#if __NATIVE_COM__
        /*
        void mPort_TxDone()
        {
            //Console.WriteLine("TxDone!");
        }

        void mPort_PowerEvent()
        {
            //Console.WriteLine("PowerEvent!");
        }*/

        void mPort_OnError(string Description)
        {
            throw new Exception(Description);
        }

        void mPort_DataReceived()
        {
            try
            {
                if (mPort.InBufferCount > 0)
                {
                    int time = Environment.TickCount;
                    byte[] readBytes = new byte[mPort.InBufferCount];
                    mPort.Read(readBytes);
                    //Console.WriteLine("
#if DEBUG
                foreach (byte b in readBytes)
                {
                    Console.Write(b.ToString("X2"));
                }
#endif
                    sbuffer += System.Text.ASCIIEncoding.ASCII.GetString(readBytes, 0, readBytes.Length);
                    while (sbuffer != null && sbuffer.Length > 0)
                    {
                        //Check header and trail
                        int startIndex = sbuffer.IndexOf("A55A");
                        int endIndex = sbuffer.IndexOf("?k?k");
                        //Check if last packet is not clear
                        if (startIndex == -1 || endIndex == -1)
                        {
                            break;
                        }
                        if (endIndex < startIndex)
                        {
                            sbuffer = sbuffer.Remove(0, endIndex);
                            continue;
                        }
                        else 
                        {
                            //truncate it
                            string cmd = sbuffer.Substring(startIndex, endIndex - startIndex).Replace("A55A", "").Replace("?k?k", "");
                            sbuffer = sbuffer.Remove(0, endIndex + 4);
                            //int time = Environment.TickCount;
                            Frame frame = Frame.Decode(cmd);

                            //Console.WriteLine("time = {0}", Environment.TickCount - time);
                            //Console.WriteLine("MID = {0}", frame.MID);
                            switch (frame.MID)
                            {
                                case MID.ClearAllRegisteredTags:
                                case MID.ConfirmRejectRegistration:
                                    break;
                                case MID.GetVersion:
                                    {
                                        if (isIDEmpty)
                                        {
                                            Array.Copy(frame.ID, id, frame.ID.Length);
                                            isIDEmpty = false;
                                        }
                                        GetVersionEventArgs arg = GetVersionEventArgs.Parse(frame.Data);
                                        if (arg != null)
                                        {
                                            msp430Version = arg.MSP430Version.ToString();
                                            bootloaderVersion = arg.BootloaderVersion.ToString();
                                            if (msp430Version == "0.0.0.0")
                                            {
                                                OnDeviceStatusNotify(DeviceStatus.Bootloader);
                                            }
                                            else
                                            {
                                                OnDeviceStatusNotify(DeviceStatus.Idle);
                                            }
                                            OnGetVersionNotify(arg);
                                        }
                                    }
                                    break;
                                case MID.KeepAlive:
                                    break;
                                case MID.KeepAliveNtf:
                                    {
                                        OnKeepAliveNotify();
                                    }
                                    break;
                                case MID.PowerUpVersionRequestNtf:
                                    {
                                        Array.Copy(frame.ID, id, frame.ID.Length);
                                        GetVersion();
                                        if (requiredToReset)
                                        {
                                            //Stop Power up event
                                            UpdateBlock();
                                            requiredToReset = false;
                                            //startingUpdate = true;
                                        }
                                        else
                                        {
                                            OnPowerUpNotify();
                                        }
                                    }
                                    break;
                                case MID.TagAnchorSearchNtf:
                                case MID.TagAnchorSearch:
                                    {
                                        TagSearchEventArgs arg = TagSearchEventArgs.Parse(frame.Data);
                                        if (arg != null)
                                        {
                                            switch (optParms.Operation)
                                            {
                                                case Operation.Inventory:
                                                    {
                                                        if (!bStopOperation)
                                                        {
                                                            InventoryParms inv = optParms as InventoryParms;
                                                            if (inv.flags == IDFilterFlags.MASK)
                                                            {
                                                                ulong bit = (ulong)(
                                                                    (ulong)arg.tagID[0] << 40 |
                                                                    (ulong)arg.tagID[1] << 32 |
                                                                    (ulong)arg.tagID[2] << 24 |
                                                                    (ulong)arg.tagID[3] << 16 |
                                                                    (ulong)arg.tagID[4] << 8 |
                                                                    (ulong)arg.tagID[5] << 0
                                                                    ) & (ulong)((((ulong)(1 << (int)mask.count) - 1) << (48 - (int)mask.count)) >> (int)mask.offset);

                                                                ulong cmpBit = (ulong)(((
                                                                    (ulong)mask.mask[0] << 40 |
                                                                    (ulong)mask.mask[1] << 32 |
                                                                    (ulong)mask.mask[2] << 24 |
                                                                    (ulong)mask.mask[3] << 16 |
                                                                    (ulong)mask.mask[4] << 8 |
                                                                    (ulong)mask.mask[5] << 0) & (ulong)(~0L << mask.mask.Length * 8 - (int)mask.count)) << 48 - mask.mask.Length * 8) >> (int)mask.offset;
                                                                if (bit == cmpBit)
                                                                {
                                                                    OnTagSearchNotify(arg);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                OnTagSearchNotify(arg);
                                                            }
                                                            if (inv.tagStopCount != 0)
                                                            {
                                                                if (++tagStopCount >= inv.tagStopCount)
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                            if (arg.errorCode == ErrorCode.ERROR_WDP_RECD_REC_NG)
                                                            {
                                                                RTLSThrowException(
                                                                    AnchorSearch(
                                                                    (byte)searchIndexRnd.Next(0xff),
                                                                    1,
                                                                    powerLevel,
                                                                    inv.ledBlinkOnGoodRead,
                                                                    TagSearchRxOptionFlags.AnyTag,
                                                                    TagSearchTxOptionFlags.StartSearch | TagSearchTxOptionFlags.UIDOnly));

                                                            }
                                                        }
                                                        
                                                    } break;
                                                case Operation.Ranging:
                                                    {
                                                        if (!bStopOperation)
                                                        {
                                                            RangingParms rang = optParms as RangingParms;
                                                            if (rang.flags == IDFilterFlags.MASK && arg.errorCode == ErrorCode.NO_ERROR)
                                                            {
                                                                ulong bit = (ulong)(
                                                                    (ulong)arg.tagID[0] << 40 |
                                                                    (ulong)arg.tagID[1] << 32 |
                                                                    (ulong)arg.tagID[2] << 24 |
                                                                    (ulong)arg.tagID[3] << 16 |
                                                                    (ulong)arg.tagID[4] << 8 |
                                                                    (ulong)arg.tagID[5] << 0
                                                                    ) & (ulong)((((ulong)(1 << (int)mask.count) - 1) << (48 - (int)mask.count)) >> (int)mask.offset);

                                                                ulong cmpBit = (ulong)(((
                                                                    (ulong)mask.mask[0] << 40 |
                                                                    (ulong)mask.mask[1] << 32 |
                                                                    (ulong)mask.mask[2] << 24 |
                                                                    (ulong)mask.mask[3] << 16 |
                                                                    (ulong)mask.mask[4] << 8 |
                                                                    (ulong)mask.mask[5] << 0) & (ulong)(~0L << mask.mask.Length * 8 - (int)mask.count)) << 48 - mask.mask.Length * 8) >> (int)mask.offset;
                                                                if (bit == cmpBit)
                                                                {
                                                                    OnTagSearchNotify(arg);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                OnTagSearchNotify(arg);
                                                            }
                                                            switch (arg.errorCode)
                                                            {
                                                                /*case ErrorCode.NO_ERROR:
                                                                    RTLSThrowException(
                                                                        LedControl(
                                                                            arg.tagID,
                                                                            rang.ledBlinkOnGoodRead)
                                                                         );
                                                                    break;*/
                                                                case ErrorCode.ERROR_WDP_RECD_REC_NG:
                                                                    //timeout resent
                                                                    RTLSThrowException(
                                                                        AnchorSearch(
                                                                            (byte)searchIndexRnd.Next(0xff),
                                                                            BackOff,
                                                                            powerLevel,
                                                                            rang.ledBlinkOnGoodRead,
                                                                            TagSearchRxOptionFlags.AnyTag | TagSearchRxOptionFlags.Toggle,
                                                                            TagSearchTxOptionFlags.StartSearch));
                                                                    break;
                                                            }
                                                        }
                                                    } break;
                                                case Operation.Searching:
                                                    {
                                                        Console.WriteLine(string.Format("{0},{1}", arg.errorCode, arg.searchIndex));
                                                        if (!bStopOperation)
                                                        {
                                                            SearchingParms search = optParms as SearchingParms;
                                                            if (arg.errorCode == ErrorCode.NO_ERROR && arg.distance <= 65525)
                                                            {
                                                                rollingMinRssi.Add(arg.rssi);
                                                                arg.rssi = (byte)rollingMinRssi.Minimum;
                                                                rollingMinDistance.Add(arg.distance);
                                                                arg.distance = (ushort)rollingMinDistance.Minimum;
                                                                arg.rssi = (byte)movingAverageRssi.NextValue(arg.rssi);
                                                                arg.distance = (ushort)movingAverageDistance.NextValue(arg.distance);

                                                                if (tagCount > 10 && Environment.TickCount - timer > 1000)
                                                                {
                                                                    avg_rssi += arg.rssi;
                                                                    avg_distance += arg.distance;

                                                                    arg.rssi = (byte)(avg_rssi / tagCount);
                                                                    arg.distance = (ushort)(avg_distance / tagCount);
                                                                    avg_distance = 0;
                                                                    avg_rssi = 0;
                                                                    tagCount = 0;
                                                                    timer = Environment.TickCount;
                                                                    OnTagSearchNotify(arg);
                                                                }
                                                                else
                                                                {
                                                                    avg_rssi += arg.rssi;
                                                                    avg_distance += arg.distance;
                                                                }
                                                                /*avg_rssi = MovingAverage(avg_rssi, arg.rssi);
                                                                arg.rssi = (byte)avg_rssi;
                                                                avg_distance = MovingAverage(avg_distance, arg.distance);
                                                                arg.distance = (ushort)avg_distance;*/
                                                                tagCount++;
                                                            }
                                                            
                                                            switch (arg.errorCode)
                                                            {
                                                                /*case ErrorCode.NO_ERROR:
                                                                    RTLSThrowException(
                                                                        LedControl(
                                                                            arg.tagID,
                                                                            search.ledBlinkOnGoodRead)
                                                                        );
                                                                    break;*/
                                                                case ErrorCode.ERROR_WDP_RECD_REC_NG:
                                                                    int index = searchIndexRnd.Next(0xff);
                                                                    Console.WriteLine("AnchorSearch {0}", index);
                                                                    RTLSThrowException(
                                                                         AnchorSearch(
                                                                         (byte)index,
                                                                         ((SearchingParms)optParms).mask,
                                                                         BackOff,
                                                                         powerLevel,
                                                                         search.ledBlinkOnGoodRead,
                                                                         TagSearchRxOptionFlags.SpecificTag | TagSearchRxOptionFlags.Toggle,
                                                                         TagSearchTxOptionFlags.StartSearch));
                                                                    break;
                                                            }
                                                        }
                                                    } break;
                                                case Operation.Read:
                                                    {
                                                        if (!bStopOperation)
                                                        {
                                                            OnTagSearchNotify(arg);
                                                        }
                                                        //TagStopCount++;
                                                    } break;
                                            }
                                            if (bStopOperation)
                                            {
                                                OnDeviceStatusNotify(DeviceStatus.Idle);
                                            }
                                        }
                                    }
                                    break;
                                case MID.AdhocBeacon:
                                    {
                                        if (frame.Data != null && frame.Data.Length == 1)
                                        {
                                            OnAdhocBeaconNotify((ErrorCode)frame.Data[0]);
                                        }
                                    }
                                    break;
                                case MID.UDControl:
                                    {
                                        if (frame.Data != null && frame.Data.Length == 1)
                                        {
                                            UDControlArgs args = UDControlArgs.Decode(frame.Data[0]);
                                            OnUDControlNotify(args);
                                        }
                                    }
                                    break;
                                case MID.EnterFirmwareUpgrade:
                                    {
                                        if (frame.Data != null && frame.Data.Length == 1)
                                        {
                                            if (((ErrorCode)frame.Data[0]) == ErrorCode.NO_ERROR) // already in bootloader
                                            {
                                                UpdateBlock();
                                            }
                                            else if (((ErrorCode)frame.Data[0]) == ErrorCode.ERROR_FWUG_CHECKSUM2_OK_RESET)
                                            {
                                                //required to reset
                                                if (!requiredToReset)
                                                {
                                                    requiredToReset = true;
                                                }
                                            }
                                            else
                                            {
                                                FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(0, FirmwareUpdateResult.Fail);
                                                OnFramwareUpgradeNotify(arg);
                                            }
                                        }
                                    }
                                    break;
                                case MID.StartFirmwareUpgrade:
                                    {
                                        if (frame.Data != null && frame.Data.Length == 3)
                                        {
                                            FirmwareUpgradeBlock block = FirmwareUpgradeBlock.Decode(frame.Data);
                                            if (block.errorCode == ErrorCode.ERROR_WRITEINFO_OK)
                                            {
                                                if (!updateSuccess)
                                                {
                                                    updateSuccess = true;
                                                    FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(100, FirmwareUpdateResult.Success);
                                                    OnFramwareUpgradeNotify(arg);
                                                }
                                            }
                                            else if (block.errorCode == ErrorCode.NO_ERROR)
                                            {
                                                FirmwareUpdateResult result = FirmwareUpdateResult.InProgress;
                                                startBlockIndex++;
                                                //Move next block
                                                if (startBlockIndex < totalBlocks)
                                                {
                                                    UpdateBlock();
                                                }

                                                //Console.WriteLine("Block Index = {0}", startBlockIndex);
                                                currentUpgradePercent = (int)(100.0 * startBlockIndex / totalBlocks);
                                                FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(currentUpgradePercent, result);
                                                OnFramwareUpgradeNotify(arg);
                                            }
                                            else
                                            {
                                                fwUpgradeRetry++;
                                                if (fwUpgradeRetry < 10)
                                                {
                                                    startBlockIndex /= 8;
                                                    UpdateBlock();
                                                }
                                                else
                                                {
                                                    FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(currentUpgradePercent, FirmwareUpdateResult.Fail);
                                                    OnFramwareUpgradeNotify(arg);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case MID.TagPositionNtf:
                                    {
                                        TagPositionNotifyArgs arg = TagPositionNotifyArgs.Decode(frame.Data);
                                        OnTagPositionNotify(arg);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (RTLSErrorException ex)
            {
                result = ex.GetError();
                OnDeviceStatusNotify(DeviceStatus.Idle);
            }
            catch (Exception)
            {
                result = Result.FAILURE;
                OnDeviceStatusNotify(DeviceStatus.Idle);
            }
        }
#else
        void mPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine(string.Format("com error {0}", e.EventType));
        }

        void mPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Byte[] buffer = new byte[mPort.BytesToRead];
            mPort.Read(buffer, 0, buffer.Length);

            sbuffer += System.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, buffer.Length).Replace("\0", "");
            //sbuffer += mPort.ReadExisting().Replace("\0", "");

            while (sbuffer != null && sbuffer.Length > 0)
            {
                //Check header and trail
                int startIndex = sbuffer.IndexOf("A55A");
                int endIndex = sbuffer.IndexOf("?k?k");
                //Check if last packet is not clear
                if (startIndex == -1 || endIndex == -1)
                {
                    break;
                }
                if (endIndex < startIndex)
                {
                    sbuffer = sbuffer.Remove(0, endIndex);
                    continue;
                }
                else
                {
                    //truncate it
                    string cmd = sbuffer.Substring(startIndex, endIndex - startIndex).Replace("A55A", "").Replace("?k?k", "");
                    sbuffer = sbuffer.Remove(0, endIndex + 4);
                    //int time = Environment.TickCount;
                    Frame frame = Frame.Decode(cmd);

                    //Console.WriteLine("time = {0}", Environment.TickCount - time);
                    //Console.WriteLine("MID = {0}", frame.MID);
                    switch (frame.MID)
                    {
                        case MID.ClearAllRegisteredTags:
                        case MID.ConfirmRejectRegistration:
                            break;
                        case MID.GetVersion:
                            {
                                GetVersionEventArgs arg = GetVersionEventArgs.Parse(frame.Data);
                                if (arg != null)
                                {
                                    msp430Version = arg.MSP430Version.ToString();
                                    bootloaderVersion = arg.BootloaderVersion.ToString();
                                    if (msp430Version == "0.0.0.0")
                                    {
                                        OnDeviceStatusNotify(DeviceStatus.BootloaderMode);
                                        /*if (startingUpdate)
                                        {
                                            UpdateBlock();
                                        }*/
                                    }
                                    else
                                    {
                                        OnDeviceStatusNotify(DeviceStatus.ApplicationMode);
                                    }
                                    OnGetVersionNotify(arg);
                                }
                            }
                            break;
                        case MID.KeepAlive:
                            break;
                        case MID.KeepAliveNtf:
                            {
                                Array.Copy(frame.ID, id, frame.ID.Length);
                                OnKeepAliveNotify();
                            }
                            break;
                        case MID.PowerUpVersionRequestNtf:
                            {
                                Array.Copy(frame.ID, id, frame.ID.Length);
                                GetVersion();
                                if (requiredToReset)
                                {
                                    //Stop Power up event
                                    UpdateBlock();
                                    requiredToReset = false;
                                    //startingUpdate = true;
                                }
                                else
                                {
                                    OnPowerUpNotify();
                                }
                            }
                            break;
                        case MID.TagAnchorSearch:
                            {
                                TagSearchEventArgs arg = TagSearchEventArgs.Parse(frame.Data);
                                if (arg != null)
                                {
                                    OnTagSearchNotify(arg);
                                }
                            }
                            break;
                        case MID.AdhocBeacon:
                            {
                                if (frame.Data != null && frame.Data.Length == 1)
                                {
                                    OnAdhocBeaconNotify((ErrorCode)frame.Data[0]);
                                }
                            }
                            break;
                        case MID.StartRF:
                            {
                                if (frame.Data != null && frame.Data.Length == 1)
                                {
                                    OnStartRFPowerNotify((RFStatus)frame.Data[0]);
                                }
                            }
                            break;
                        case MID.EnterFirmwareUpgrade:
                            {
                                if (frame.Data != null && frame.Data.Length == 1)
                                {
                                    if (((ErrorCode)frame.Data[0]) == ErrorCode.NO_ERROR) // already in bootloader
                                    {
                                        UpdateBlock();
                                    }
                                    else if (((ErrorCode)frame.Data[0]) == ErrorCode.ERROR_FWUG_CHECKSUM2_OK_RESET)
                                    {
                                        //required to reset
                                        if (!requiredToReset)
                                        {
                                            requiredToReset = true;
                                        }
                                    }
                                    else
                                    {
                                        FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(0, FirmwareUpdateResult.Fail);
                                        OnFramwareUpgradeNotify(arg);
                                    }
                                }
                            }
                            break;
                        case MID.StartFirmwareUpgrade:
                            {
                                if (frame.Data != null && frame.Data.Length == 3)
                                {
                                    FirmwareUpgradeBlock block = FirmwareUpgradeBlock.Decode(frame.Data);
                                    if (block.errorCode == ErrorCode.ERROR_WRITEINFO_OK)
                                    {
                                        if (!updateSuccess)
                                        {
                                            updateSuccess = true;
                                            FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(100, FirmwareUpdateResult.Success);
                                            OnFramwareUpgradeNotify(arg);
                                        }
                                    }
                                    else if (block.errorCode == ErrorCode.NO_ERROR)
                                    {
                                        FirmwareUpdateResult result = FirmwareUpdateResult.InProgress;
                                        startBlockIndex++;
                                        //Move next block
                                        if (startBlockIndex < totalBlocks)
                                        {
                                            UpdateBlock();
                                        }
                                        
                                        //Console.WriteLine("Block Index = {0}", startBlockIndex);
                                        currentUpgradePercent = (int)(100.0 * startBlockIndex / totalBlocks);
                                        FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(currentUpgradePercent, result);
                                        OnFramwareUpgradeNotify(arg);
                                    }
                                    else
                                    {
                                        fwUpgradeRetry++;
                                        if (fwUpgradeRetry < 10)
                                        {
                                            startBlockIndex /= 8;
                                            UpdateBlock();
                                        }
                                        else
                                        {
                                            FirmwareUpgradeArgs arg = new FirmwareUpgradeArgs(currentUpgradePercent, FirmwareUpdateResult.Fail);
                                            OnFramwareUpgradeNotify(arg);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
#endif
        #endregion

        #region public function
        /// <summary>
        /// AdhocBeacon
        /// </summary>
        /// <param name="searchIndex"></param>
        /// <param name="txPower"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Result AdhocBeacon(byte searchIndex, Byte txPower, AdhocBeaconFlags flags)
        {
            return AdhocBeacon(searchIndex, new byte[6], txPower, true, flags);
        }
        /// <summary>
        /// AdhocBeacon
        /// </summary>
        /// <param name="searchIndex"></param>
        /// <param name="tagID"></param>
        /// <param name="txPower"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Result AdhocBeacon(byte searchIndex, Byte[] tagID, Byte txPower, AdhocBeaconFlags flags)
        {
            return AdhocBeacon(searchIndex, tagID, txPower, false, flags);
        }
        /// <summary>
        /// AdhocBeacon
        /// </summary>
        /// <param name="searchIndex"></param>
        /// <param name="tagID"></param>
        /// <param name="txPower"></param>
        /// <param name="isSearchAny"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Result AdhocBeacon(byte searchIndex, Byte[] tagID, Byte txPower, bool isSearchAny, AdhocBeaconFlags flags)
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            if (currentStatus != DeviceStatus.Idle && currentStatus != DeviceStatus.Stop)
            {
                return Result.DEVICE_NOT_READY;
            }

            try
            {
                Frame frm = new Frame();
                frm.Delimiter = 0xC;
                frm.IsIncludeTimeStampMiSec = false;
                frm.IsIncludeTimeStampSec = false;
                frm.IsResponse = true;
                frm.IsSend = true;
                frm.ID = id;
                frm.MID = MID.AdhocBeacon;
                frm.Length = 10;
                frm.Data = new byte[] { searchIndex, 
                    tagID[5], 
                    tagID[4], 
                    tagID[3], 
                    tagID[2], 
                    tagID[1], 
                    tagID[0], 
                    (byte)(isSearchAny ? 0x01 : 0x0),
                    txPower,
                    (byte)flags
                };
                byte[] raw = frm.Encode();
#if __NATIVE_COM__
                mPort.Output = raw;
#else
                mPort.Write(raw, 0, raw.Length);
#endif
            }
            catch (Exception e)
            {
                return Result.FAILURE;
            }
            return Result.OK;
        }
        /// <summary>
        /// GetPowerLevel
        /// </summary>
        /// <param name="powerLevel"></param>
        /// <returns></returns>
        public Result GetPowerLevel(out byte powerLevel)
        {
            lock (syncLock)
            {
                powerLevel = this.powerLevel;
            }
            return Result.OK;
        }
        /// <summary>
        /// SetPowerLevel
        /// </summary>
        /// <param name="powerLevel"></param>
        /// <returns></returns>
        public Result SetPowerLevel(byte powerLevel)
        {
            lock (syncLock)
            {
                if (powerLevel > 0x3f)
                    return Result.INVALID_PARAMETER;
                this.powerLevel = powerLevel;
            }
            return Result.OK;
        }
        /// <summary>
        /// Set Back Off value (Number of tags searching)
        /// </summary>
        /// <param name="powerLevel"></param>
        /// <returns></returns>
        public Result SetBackOff (byte BackOff)
        {
            lock (syncLock)
            {
                if (BackOff > 1)
                    this.BackOff = (byte)(BackOff << 2);
                else
                    this.BackOff = 4;
            }

            return Result.OK;
        }

        /// <summary>
        /// GetSelectMask
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public Result GetSelectMask(SelectMask mask)
        {
            mask = this.mask;
            return Result.OK;
        }
        /// <summary>
        /// SetSelectMask
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public Result SetSelectMask(SelectMask mask)
        {
            if (mask == null)
                return Result.INVALID_PARAMETER;
            this.mask.count = mask.count;
            this.mask.offset = mask.offset;
            this.mask.mask = (byte[])mask.mask.Clone();
            return Result.OK;
        }
        /// <summary>
        /// StartOperation
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Result StartOperation(IOperationParms parms)
        {
            Random innerRnd = new Random(Guid.NewGuid().GetHashCode());

            try
            {
                switch(currentStatus)
                {
                    case DeviceStatus.Busy:
                    case DeviceStatus.Stop:
                        RTLSThrowException(Result.RADIO_BUSY);
                        break;
                    case DeviceStatus.Error:
                    case DeviceStatus.Bootloader:
                    case DeviceStatus.Unknown:
                        RTLSThrowException(Result.DEVICE_NOT_READY);
                        break;
                    case DeviceStatus.Idle:
                        break;
                }
                optParms = parms;
                bStopOperation = false;
                tagStopCount = 0;
                switch (optParms.Operation)
                {
                    case Operation.Inventory:
                        {
                            InventoryParms inv = optParms as InventoryParms;

                            if (inv == null)
                            {
                                RTLSThrowException(Result.INVALID_PARAMETER);
                            }
                            result = AnchorSearch((byte)innerRnd.Next(255), BackOff, this.powerLevel, inv.ledBlinkOnGoodRead, TagSearchRxOptionFlags.AnyTag | TagSearchRxOptionFlags.Toggle, TagSearchTxOptionFlags.StartSearch | TagSearchTxOptionFlags.UIDOnly);
                        }
                        break;
                    case Operation.Ranging:
                        {
                            RangingParms rang = optParms as RangingParms;

                            if (rang == null)
                            {
                                RTLSThrowException(Result.INVALID_PARAMETER);
                            }
                            result = AnchorSearch((byte)innerRnd.Next(255), BackOff, this.powerLevel, rang.ledBlinkOnGoodRead, TagSearchRxOptionFlags.AnyTag | TagSearchRxOptionFlags.Toggle, TagSearchTxOptionFlags.StartSearch);
                        }
                        break;
                    case Operation.Searching:
                        {
                            SearchingParms search = optParms as SearchingParms;

                            if (search == null || search.mask == null || search.mask.Length != 6)
                            {
                                RTLSThrowException(Result.INVALID_PARAMETER);
                            }
                            avg_distance = 0;
                            avg_rssi = 0;
                            tagCount = 0;
                            rollingMinRssi = new RollingMinimum(search.threshold);
                            rollingMinDistance = new RollingMinimum(search.threshold);
                            result = AnchorSearch((byte)innerRnd.Next(255), search.mask, BackOff, this.powerLevel, search.ledBlinkOnGoodRead, TagSearchRxOptionFlags.SpecificTag | TagSearchRxOptionFlags.Toggle, TagSearchTxOptionFlags.StartSearch);
                            timer = Environment.TickCount;
                        }
                        break;
                    case Operation.Read:
                        {
                            ReadParms read = optParms as ReadParms;
                            if (read == null)
                            {
                                RTLSThrowException(Result.INVALID_PARAMETER);
                            }
                            result = AnchorSearch((byte)innerRnd.Next(256), BackOff, this.powerLevel, read.ledBlinkOnGoodRead, TagSearchRxOptionFlags.AnyTag | TagSearchRxOptionFlags.Toggle, TagSearchTxOptionFlags.StartSearch);
                        }
                        break;
                }
                OnDeviceStatusNotify(DeviceStatus.Busy);
            }
            catch (RTLSErrorException exception)
            {
                result = exception.GetError();
            }
            catch (Exception exception)
            {
                result = Result.FAILURE;
            }
            return result;
        }
        /// <summary>
        /// StopOperation
        /// </summary>
        /// <returns></returns>
        public Result StopOperation()
        {
            if (currentStatus == DeviceStatus.Busy)
            {
                //TagSearch(0, 0, 0x3f, TagSearchRxOptionFlags.AnyTag | TagSearchRxOptionFlags.Toggle, TagSearchTxOptionFlags.StopSearch | TagSearchTxOptionFlags.UIDOnly);
                bStopOperation = true;
                OnDeviceStatusNotify(DeviceStatus.Stop);
                if (timeout != null)
                {
                    timeout.Change(0, 5000);
                }
            }

            Thread.Sleep(100);
            return Result.OK;
        }
        private void TimeoutProc(object obj)
        {
            RTLSProvider prov = obj as RTLSProvider;
            if (prov == null || prov.disposed)
            {
                return;
            }
            if (timeout != null)
            {
                timeout.Change(Timeout.Infinite, 5000);
            }
            OnDeviceStatusNotify(DeviceStatus.Idle);
        }
        /// <summary>
        /// LedControl
        /// </summary>
        /// <param name="TargetID"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public Result LedControl(Byte[] TargetID, byte times)
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            try
            {
                Frame frm = new Frame();
                frm.Delimiter = 0xC;
                frm.IsIncludeTimeStampMiSec = false;
                frm.IsIncludeTimeStampSec = false;
                frm.IsResponse = true;
                frm.IsSend = true;
                frm.ID = id;
                frm.MID = MID.LedControl;
                frm.Length = 7;
                frm.Data = new byte[] {
                    TargetID[5], 
                    TargetID[4], 
                    TargetID[3], 
                    TargetID[2], 
                    TargetID[1], 
                    TargetID[0], 
                    times
                };
                byte[] raw = frm.Encode();
#if __NATIVE_COM__
                mPort.Output = raw;
#else
                mPort.Write(raw, 0, raw.Length);
#endif
            }
            catch (Exception e)
            {
                return Result.FAILURE;
            }
            return Result.OK;
        }

        Result AnchorSearch(InventoryParms parms, Byte led, TagSearchRxOptionFlags rxFlags, TagSearchTxOptionFlags txflags)
        {
            return AnchorSearch((byte)searchIndexRnd.Next(0xff), new byte[6], BackOff, powerLevel, led, rxFlags, txflags);
        }
        Result AnchorSearch(RangingParms parms, Byte led, TagSearchRxOptionFlags rxFlags, TagSearchTxOptionFlags txflags)
        {
            return AnchorSearch((byte)searchIndexRnd.Next(0xff), new byte[6], BackOff, powerLevel, led, rxFlags, txflags);
        }
        Result AnchorSearch(byte searchIndex, int backOffTime, Byte txPower, Byte led, TagSearchRxOptionFlags rxFlags, TagSearchTxOptionFlags txflags)
        {
            return AnchorSearch(searchIndex, new byte[6], backOffTime, txPower, led, rxFlags, txflags);
        }

        Result AnchorSearch(byte searchIndex, Byte[] tagID, int backOffTime, Byte txPower, Byte led, TagSearchRxOptionFlags rxFlags, TagSearchTxOptionFlags txFlags)
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            try
            {
                Frame frm = new Frame();
                frm.Delimiter = 0xC;
                frm.IsIncludeTimeStampMiSec = false;
                frm.IsIncludeTimeStampSec = false;
                frm.IsResponse = true;
                frm.IsSend = true;
                frm.ID = id;
                frm.MID = MID.TagAnchorSearch;
                frm.Length = 12;
                frm.Data = new byte[] { searchIndex, 
                    tagID[5], 
                    tagID[4], 
                    tagID[3], 
                    tagID[2], 
                    tagID[1], 
                    tagID[0], 
                    (byte)backOffTime,
                    (byte)rxFlags,
                    led,
                    txPower,
                    (byte)txFlags
                };
                byte[] raw = frm.Encode();
#if __NATIVE_COM__
                mPort.Output = raw;
#else
            mPort.Write(raw, 0, raw.Length);
#endif
            }
            catch (Exception e)
            {
                return Result.FAILURE;
            }
            return Result.OK;
        }
        /// <summary>
        /// GetVersion
        /// </summary>
        /// <returns></returns>
        public Result GetVersion()
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            try
            {
                Frame frm = new Frame();
                frm.Delimiter = 0xC;
                frm.IsResponse = true;
                frm.IsSend = true;
                frm.IsIncludeTimeStampMiSec = frm.IsIncludeTimeStampSec = false;
                frm.ID = id;
                frm.MID = MID.GetVersion;
                frm.Length = 0;
                byte[] raw = frm.Encode();

#if __NATIVE_COM__
                mPort.Output = raw;
#else
                mPort.Write(raw, 0, raw.Length);
#endif
            }
            catch (Exception e)
            {
                return Result.FAILURE;
            }
            return Result.OK;
        }
        /// <summary>
        /// UDControl
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Result UDControl(UDControlFlags flags)
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            if (currentStatus != DeviceStatus.Idle)
            {
                return Result.DEVICE_NOT_READY;
            }
            try
            {
                UDControlArgs arg = new UDControlArgs();
                arg.IsAlertON = (flags & UDControlFlags.AlertOn) == UDControlFlags.AlertOn;
                arg.IsApplyChanged = (flags & UDControlFlags.SetStatus) == UDControlFlags.SetStatus;
                arg.IsRangingON = !((flags & UDControlFlags.RangingOFF) == UDControlFlags.RangingOFF);
                arg.IsRangingDataON = (flags & UDControlFlags.RangingDataON) == UDControlFlags.RangingDataON;
                arg.IsRFAssigned = (flags & UDControlFlags.RFAssign) == UDControlFlags.RFAssign;
                arg.IsApplyChanged = (flags & UDControlFlags.SetStatus) == UDControlFlags.SetStatus;
                Frame frm = new Frame();
                frm.Delimiter = 0xC;
                frm.IsResponse = true;
                frm.IsSend = true;
                frm.IsIncludeTimeStampMiSec = frm.IsIncludeTimeStampSec = false;
                frm.ID = id;
                frm.MID = MID.UDControl;
                frm.Length = 1;
                frm.Data = new byte[] { arg.Encode() };
                byte[] raw = frm.Encode();

#if __NATIVE_COM__
                mPort.Output = raw;
#else
                mPort.Write(raw, 0, raw.Length);
#endif
            }
            catch (Exception e)
            {
                return Result.DEVICE_NOT_READY;
            }
            return Result.OK;
        }

        private Result EnterUpgradeMode()
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            try
            {
                currentUpgradePercent = 0;
                Frame frm = new Frame();
                frm.Delimiter = 0xC;
                frm.IsResponse = true;
                frm.IsSend = true;
                frm.IsIncludeTimeStampMiSec = frm.IsIncludeTimeStampSec = false;
                frm.ID = id;
                frm.MID = MID.EnterFirmwareUpgrade;
                frm.Length = 6;
                frm.Data = id;
                byte[] raw = frm.Encode();

#if __NATIVE_COM__
                mPort.Output = raw;
#else
                mPort.Write(raw, 0, raw.Length);
#endif
            }
            catch (Exception e)
            {
                return Result.FAILURE;
            }
            return Result.OK;
        }
        /// <summary>
        /// FirmwareUpgrade
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Result FirmwareUpgrade(string filePath)
        {
            if (mPort == null || !mPort.IsOpen)
            {
                return Result.NOT_INITIALIZED;
            }
            try
            {
                long fileLen = 0;
                fwUpgradeRetry = 0;
                totalBlocks = 0;
                startBlockIndex = 0;
                updateSuccess = false;

                using (FileStream sr = new FileStream(filePath, FileMode.Open))
                {
                    fileLen = sr.Length;
                    fwUpgradeBuffer = new byte[fileLen];
                    sr.Read(fwUpgradeBuffer, 0, (int)fileLen);
                }
                totalBlocks = (uint)Math.Ceiling((double)((double)fileLen / 64.0));

                if (totalBlocks > 1023)
                {
                    return Result.INVALID_PARAMETER;
                }

                return EnterUpgradeMode();
            }
            catch (Exception e)
            {
                return Result.FAILURE;
            }
            return Result.OK;
        }

        private void UpdateBlock()
        {
            Frame frm = new Frame();
            frm.Delimiter = 0xC;
            frm.IsResponse = true;
            frm.IsSend = true;
            frm.IsIncludeTimeStampMiSec = frm.IsIncludeTimeStampSec = false;
            frm.ID = id;
            frm.MID = MID.StartFirmwareUpgrade;
            frm.Length = 77;
            frm.Data = FirmwareUpgradeBlock.Encode(
                id,
                (int)totalBlocks,
                (int)startBlockIndex,
                fwUpgradeBuffer
           );
            byte[] raw = frm.Encode();

#if __NATIVE_COM__
            mPort.Output = raw;
#else
            mPort.Write(raw, 0, raw.Length);
#endif
        }
        private const int AVG_SAMPLES = 16;

        private float MovingAverage(float __avg, int __val)
        {
            return __avg > 0  ?
                (((__avg * (AVG_SAMPLES - 1)) + __val ) /
                (AVG_SAMPLES) ) :
                (__val);
        }
        private float ExpMovingAverage(float __avg, int __val)
        {
            return ((2.0f / (1 + AVG_SAMPLES)) * (__val - __avg)) + __avg;
        }
        void RTLSThrowException(Result result)
        {
            if ((this.result = result) != Result.OK)
            {
                throw new RTLSErrorException(result);
            }
        }
        #endregion
#if NOUSE
        public void Send(byte[] cmd)
        {
            if (cmd != null && cmd.Length > 0)
            {
                /*foreach (byte b in cmd)
                {
                    mPort.Output = new byte[] { b };
                    System.Threading.Thread.Sleep(1);
                }*/
                mPort.Write(cmd, 0, cmd.Length);
            }
        }

        public void Send(string cmd)
        {
            if (cmd != null && cmd.Length > 0)
            {
                byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd);
                /*foreach (byte bb in b)
                {
                    mPort.Output = new byte[] { bb };
                    System.Threading.Thread.Sleep(1);
                }*/
                mPort.Write(b, 0, b.Length);
            }
        }
#endif
#if NOUSE
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        private static extern bool GetExitCodeThread(UInt32 hThread, out uint lpExitCode);

        private bool IsAlive()
        {
            if (mThreadRun != null)
            {
#if WindowsCE
                uint exCode = 0;
                if (!GetExitCodeThread((uint)mThreadRun.ManagedThreadId, out exCode))
                {
                    return false;
                }
                return (exCode == 0x00000103);

#else
                return mThreadRun.IsAlive;
#endif
            }
            return false;
        }
#endif
    }
}
