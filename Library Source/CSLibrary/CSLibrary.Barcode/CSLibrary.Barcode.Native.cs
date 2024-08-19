using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Barcode.Constants;
using CSLibrary.Barcode.Structures;

namespace CSLibrary.Barcode
{
#if NOSUE //comment : not hardware support at this moment
    internal class OEM
    {
        private const string DLL = "commDrv.dll";
        ///<summary>
        /// Function prototypes
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern void SetHardwareTrigger(bool bEnable);

        [DllImport(DLL, SetLastError = true)]
        public static extern bool ImagerPoweredDown();

        [DllImport(DLL, SetLastError = true)]
        public static extern void WakeUpImager();

        [DllImport(DLL, SetLastError = true)]
        public static extern bool ConfigureCommPort([In, Out] SerialPortConfig cfg);

        [DllImport(DLL, SetLastError = true)]
        public static extern void PreOpenConfigPort(int nComPort);

        [DllImport(DLL, SetLastError = true)]
        public static extern void PostCloseConfigPort(int nComPort);

    }
#endif
    ///<summary>
    /// Callback prototype
    ///</summary>
    public delegate int EventCallback
    (
        [In] uint eventType,
        [In] uint dwBytes
    );

    sealed class Native
    {
        private const string DLL = "hhpImgrSdk.dll";
        

        ///<summary>
        /// Connect/Disconnect functions.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpConnect
        (
            [In] ComPort connectType,
            [In] IntPtr pStruct
        );

        /*[DllImport(DLL, SetLastError = true)]
        public static extern Result hhpNamedConnect(ref char ptcConnectName, IntPtr pStruct);*/

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpDisconnect();

        [DllImport(DLL, SetLastError = true)]
        public static extern bool hhpEngineConnected();

        ///<summary>
        /// Miscellaneous Error Code Message function.
        ///</summary>
        [DllImport(DLL, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Result hhpGetErrorMessage
        (
            [In] Result nErrorCode,
            [In, Out] StringBuilder ptcErrorMsg,
            [In] int nMaxChars
        );

        ///<summary>
        /// Asynchronous event functions.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSetAsyncMethods
        (
            [In] IntPtr hEventHandle,
            [In] IntPtr hWndHandle,
            [In] EventCallback hEventCallback
        );

        /*///<summary>
        /// Retrieves the data from the last signal event (image/barcode capture).  This function can be called with pResultStruct set to NULL 
        /// to obtain the event type.  This is useful when the notification method is a Windows event.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpGetAsyncResult
        (
            [In, Out] ref EVENT_TYPE pEventType,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] CFG_DECODE_MSG pResultStruct
        );

        ///<summary>
        /// Retrieves the data from the last signal event (image/barcode capture).  This function can be called with pResultStruct set to NULL 
        /// to obtain the event type.  This is useful when the notification method is a Windows event.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpGetAsyncResult
        (
            [In, Out] ref EVENT_TYPE pEventType,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] CFG_RAW_DECODE_MSG pResultStruct
        );*/

        ///<summary>
        /// Retrieves the data from the last signal event (image/barcode capture).  This function can be called with pResultStruct set to NULL 
        /// to obtain the event type.  This is useful when the notification method is a Windows event.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpGetAsyncResult
        (
            [In, Out] ref EventType pEventType,
            [In, Out] IntPtr pResultStruct
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpCancelIo();

        ///<summary>
        /// Whole config functions
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpReadConfigStream
        (
            //[In, Out, MarshalAs(UnmanagedType.ByValArray)] ref Byte[] puchCfgStream, 
            int nMaxLen,
            ref int pnBytesReturned
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpWriteConfigStream
        (
            //[In, Out, MarshalAs(UnmanagedType.ByValArray)] ref Byte[] puchCfgStream,
            int nLen
        );
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfgType">Use CURRENT for the current settings, or DEFAULT for the  
        /// customer default settings.</param>
        /// <param name="item">One of the members of the enumerated type CFG_ITEMS. </param>
        /// <param name="pStruct">structure based on parameter "item:"</param>
        /// <returns></returns>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpReadConfigItem/*OK*/
        (
            [In]        SetupType cfgType,
            [In]        ConfigItems item,
            [In, Out]   IntPtr pStruct 
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpWriteConfigItem/*OK*/
        (
            [In]        ConfigItems item,
            [In]        IntPtr pStruct 
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSetConfigItemToDefaults
        (
            [In]        ConfigItems item
        );
        ///<summary>
        /// General config functions
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpReadImagerCapabilities/*OK*/
        (
            [In] IntPtr pImgrCaps
        );

        ///<summary>
        /// Symbology specific functions.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpReadSymbologyConfig/*OK*/
        (
            [In]        SetupType cfgType,
            [In]        Symbol nSymbol, 
            [In, Out]   IntPtr pvSymStruct
         );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpWriteSymbologyConfig/*OK*/
        (
            [In]        Symbol nSymId,
            [In]        IntPtr pvSymStruct
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSetSymbologyDefaults/*OK*/
        (
            [In] Symbol nSymId
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpEnableDisableSymbology/*OK*/
        (
            [In] Symbol nSymId,
            [In] bool bEnable
        );

        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpReadSymbologyRangeMaxMin/*OK*/
        (
            [In] Symbol nSymId,
            [In, Out] Int32 [] nMinVals,
            [In, Out] Int32 [] nMaxVals
       );

        // Barcode capture support
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpCaptureBarcode/*OK*/
        (
            [In, Out]   IntPtr pDecodeMsg,
            [In]        UInt32 dwTimeout,
            [In]        bool bWait
        );  // Timeout in milliseconds
        
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpCaptureRawBarcode
        (
            [In, Out]   IntPtr pDecodeMsg,
            [In]        UInt32 dwTimeout,
            [In]        bool bWait
        );  // Timeout in milliseconds
        ///<summary>
        /// Barcode capture support
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSetBarcodeDataCodePage
        (
            [In] CodePage dwCodePage
        );


        // Image capture/transfer
#if ONLY_SUPPORT_DOT_NET_2
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpAcquireImage(
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageMessage pImg,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageTransferParms pImgTrans,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageAcquisitionParms pImgAcqu, 
            [In] bool bWait);
#else
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpAcquireImage(
            [In, Out] IntPtr pImg,
            [In, Out] IntPtr pImgTrans,
            [In, Out] IntPtr pImgAcqu,
            [In, MarshalAs(UnmanagedType.Bool)] bool bWait);
#endif
#if ONLY_SUPPORT_DOT_NET_2
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpGetLastImage(
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageMessage pImg,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageTransferParms pImgTrans, 
            [In] bool bWait);
#else
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpGetLastImage(
            [In, Out] IntPtr pImg,
            [In, Out] IntPtr pImgTrans,
            [In] bool bWait);
#endif
#if ONLY_SUPPORT_DOT_NET_2
        // Intelligent Image Capture
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpAcquireIntelligentImage(
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] IntelligentImage pIntelImg,
            [In, Out] IntPtr pDecodeMsg,
            [In] UInt32 dwTimeout,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageMessage pImg, 
            [In] bool bWait);
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpRawAcquireIntelligentImage(
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] IntelligentImage pIntelImg,
            [In, Out] IntPtr pRawDecodeMsg,
            [In] UInt32 dwTimeout,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] ImageMessage pImg,
            [In] bool bWait);
#else
        // Intelligent Image Capture
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpAcquireIntelligentImage(
            [In, Out] IntPtr pIntelImg,
            [In, Out] IntPtr pDecodeMsg,
            [In] UInt32 dwTimeout,
            [In, Out] IntPtr pImg,
            [In] bool bWait);
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpRawAcquireIntelligentImage(
            [In, Out] IntPtr pIntelImg,
            [In, Out] IntPtr pRawDecodeMsg,
            [In] UInt32 dwTimeout,
            [In, Out] IntPtr pImg,
            [In] bool bWait);
#endif
        ///<summary>
        /// Send Command interface
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSendMessage
        (
            [In, Out] ref byte puchMsg,
            [In] int nLen, 
            [In,  MarshalAs(UnmanagedType.Bool)] bool bSendRaw,
            [In, Out] ref byte puchReply,
            [In] int nLenToRead,
            [In, Out] ref int pnRetLen
        );

        ///<summary>
        /// Command only interface
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSendActionCommand
        (
            [In] Action actionCmd,
            [In] int nVal
        );

        ///<summary>
        /// Firmware upgrade function.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpUpgradeFirmware
        (

            [In, MarshalAs(UnmanagedType.LPTStr)] String ptcFirmwareFilename,
            [In, Out] IntPtr pdwTransferPercent,
            [In, Out] IntPtr hTransferNotifyHwnd
        );

        ///<summary>
        /// Specify a hardware line (sleep detection, trigger etc) dll file name
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSetHardwareLineDllFileName
        (
            [In, MarshalAs(UnmanagedType.LPTStr)] String ptcHwrFilename
        );

        ///<summary>
        /// Start/Stop imager imaging hardware scanning.
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpStartImageStream
        (
            [In, MarshalAs(UnmanagedType.Bool)] bool bStartStream
        );

        ///<summary>
        /// Get Image Engine Information (5000 engine with a PSOC only)
        /// Send Plug and Play commands (set specific configuration by single command)
        ///</summary>
        [DllImport(DLL, SetLastError = true)]
        public static extern Result hhpSendPlugAndPlayCommand
        (
            [In] PlugAndPlay pnpCmd
        );

    }
}
