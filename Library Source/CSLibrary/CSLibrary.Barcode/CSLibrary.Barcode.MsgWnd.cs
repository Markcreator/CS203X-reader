#if CS101 && !BizTalk
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using CSLibrary.Barcode;
using CSLibrary.Barcode.Constants;
using CSLibrary.Barcode.Structures;

namespace CSLibrary.Barcode
{
    internal class MessageWindows : MessageWindow
    {
        /// <summary>
        /// BarcodeEventHandler : Capture completed event trigger
        /// </summary>
        public event Barcode.BarcodeEventHandler m_captureCompleted;

        /// <summary>
        /// BarcodeStateEventHandler : report current operation
        /// </summary>
        public event Barcode.BarcodeStateEventHandler m_stateChanged;
        public MessageWindows()
        {

        }

        /// <summary>
        /// Override the default WndProc behavior to examine messages.
        /// </summary>
        /// <param name="msg"></param>
        protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message msg)
        {
            try
            {
                switch ((WM_ID)msg.Msg)
                {
                    // If message is of interest, invoke the method on the form that
                    // functions as a callback to perform actions in response to the message.
                    case WM_ID.WM_SDK_EVENT_HWND_MSG:

                        Debug.WriteLine("WM_SDK_EVENT_HWND_MSG");
                        EventType eventType = (EventType)msg.WParam;   // Event type
                        string tcErrMsg = "";                       // Error message buffer.
                        Result nResult = Result.INITIALIZE;         // Return code.

                        try
                        {

                            if (eventType == EventType.BARCODE_EVENT)          // Verify the event type is barcode
                            {
                                #region Barcode Message

                                Debug.WriteLine("BARCODE_EVENT");
                                DecodeMessage decodeInfo = new DecodeMessage();       // Decode message structure.
                                RawDecodeMessage rawInfo = new RawDecodeMessage();

                                if (Barcode.bCaptureDecoded)
                                {
                                    if ((nResult = Barcode.GetAsyncResult(ref eventType, decodeInfo)) != Result.SUCCESS)
                                    {
                                        Barcode.GetErrorMessage(nResult, ref tcErrMsg);
                                        throw new System.Exception(tcErrMsg);
                                    }
                                    if (m_captureCompleted != null)
                                        m_captureCompleted(new BarcodeEventArgs(MessageType.DEC_MSG, decodeInfo));
                                }
                                else
                                {
                                    if ((nResult = Barcode.GetAsyncResult(ref eventType, rawInfo)) != Result.SUCCESS)
                                    {
                                        Barcode.GetErrorMessage(nResult, ref tcErrMsg);
                                        throw new System.Exception(tcErrMsg);
                                    }
                                    if (m_captureCompleted != null)
                                        m_captureCompleted(new BarcodeEventArgs(MessageType.RAW_MSG, decodeInfo));

                                }

                                if (!Barcode.bStop)
                                {
                                    if (Barcode.bCaptureDecoded)
                                    {
                                        if ((nResult = Barcode.CaptureBarcode(null, 0, false)) != Result.SUCCESS)
                                        {
                                            Barcode.GetErrorMessage(nResult, ref tcErrMsg);
                                            throw new System.Exception(tcErrMsg);
                                        }
                                    }
                                    else
                                    {
                                        if ((nResult = Barcode.CaptureRawBarcode(null, 0, false)) != Result.SUCCESS)
                                        {
                                            Barcode.GetErrorMessage(nResult, ref tcErrMsg);
                                            throw new System.Exception(tcErrMsg);
                                        }
                                    }
                                }
                                else
                                {
                                    if (m_stateChanged != null)
                                        m_stateChanged(new BarcodeStateEventArgs(BarcodeState.IDLE));
                                    Barcode.bStop = false;
                                }
                                #endregion
                            }
                        }
                        catch
                        {
                            if (m_stateChanged != null)
                                m_stateChanged(new BarcodeStateEventArgs(BarcodeState.IDLE));
                            Barcode.bStop = false;
                        }
                        /*else if (eventType == EVENT_TYPE.IMAGE_EVENT)       // Verify the event type is image
                        {
                            #region Image
                            Debug.WriteLine("IMAGE_EVENT");

                            IMAGER_CAPS imgcap = new IMAGER_CAPS();

                            if ((nResult = Barcode.ReadImagerCapabilities(imgcap)) != RESULT.SUCCESS)
                            {
                                Barcode.GetErrorMessage(nResult, ref tcErrMsg);                         //Display error message
                                throw new System.Exception(tcErrMsg);
                            }

                            int imageSize = imgcap.fullImgSize.width * imgcap.fullImgSize.height * 2;
                            IMAGE image = new IMAGE();                                                  //image structure.
                            // Set the IMAGE structure size and allocate a buffer for the data, 
                            // set the buffer size and how we want to receive the data in the buffer.
                            image.puchBuffer = Marshal.AllocHGlobal(imageSize);    // Allocate a buffer big enough to hold 640x480x8 plus header (if BMP)
                            image.nBufferSize = imageSize;                         // SDK wants to know how big the buffer is so there's no overflow
                            image.imageFormat = IMG_FORMATS.FF_BMP_GRAY;           // 8 bit bmp file format data
                            if ((nResult = Barcode.GetAsyncResult(ref eventType, image)) == RESULT.SUCCESS)
                            {
                                // save image data to a bmp file and/or display it
                                if (m_captureCompleted != null)
                                {
                                    m_captureCompleted(new BarcodeEventArgs(MSG_TYPE.IMG_MSG, image));
                                }
                            }
                            else
                            {
                                Barcode.GetErrorMessage(nResult, ref tcErrMsg);                         //Display error message
                                throw new System.Exception(tcErrMsg);
                            }
                            //Free allocated memory
                            if (image.puchBuffer != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(image.puchBuffer);
                            }
                            #endregion
                        }
                        */
                        break;
                    case WM_ID.WM_SDK_IMAGER_FLASHING:
                        Debug.WriteLine("WM_SDK_IMAGER_FLASHING");
                        break;
                    case WM_ID.WM_SDK_POWER_EVENT:
                        Debug.WriteLine("WM_SDK_POWER_EVENT");
                        break;
                    case WM_ID.WM_SDK_PROGRESS_HWND_MSG:
                        Debug.WriteLine("WM_SDK_PROGRESS_HWND_MSG");
                        break;
                    case WM_ID.WM_SDK_SEQ_BARCODE_READ:
                        Debug.WriteLine("WM_SDK_SEQ_BARCODE_READ");
                        break;
                         
                }
            }
            catch (System.Exception ex)
            {
                if (m_captureCompleted != null)
                {
                    m_captureCompleted(new BarcodeEventArgs(MessageType.ERR_MSG, ex.Message));
                }
                if (m_stateChanged != null)
                {
                    m_stateChanged(new BarcodeStateEventArgs(BarcodeState.IDLE));
                }
                Barcode.bStop = false;
                CSLibrary.SysLogger.LogError(ex);
            }
            // Call the base WndProc method
            // to process any messages not handled.
            base.WndProc(ref msg);
        }
    }
}
#endif