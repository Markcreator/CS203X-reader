using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSLibrary.Events
{
    using CSLibrary.Structures;
    using CSLibrary.Constants;
   
    /// <summary>
    /// Inventory or tag search callback event argument
    /// </summary>
    public class OnAsyncCallbackEventArgs : EventArgs
    {
        /// <summary>
        /// Callback Tag Information
        /// </summary>
        public readonly TagCallbackInfo info = new TagCallbackInfo();
        /// <summary>
        /// Async callback data type
        /// </summary>
        public readonly CallbackType type = CallbackType.UNKNOWN;
        /// <summary>
        /// Cancel async callback.
        /// </summary>
        public bool Cancel = false;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Tag Information</param>
        /// <param name="type">Callback Type</param>
        public OnAsyncCallbackEventArgs(TagCallbackInfo info, CallbackType type)
        {
            this.info = info;
            this.type = type;
        }
    }
    /// <summary>
    /// Tag Access Completed Argument
    /// </summary>
    public class OnAccessCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Access Result
        /// </summary>     
        public readonly bool success = false;
        /// <summary>
        /// Access bank
        /// </summary>
        public readonly Bank bank = Bank.UNKNOWN;
        /// <summary>
        /// Access Type
        /// </summary>
        public readonly TagAccess access = TagAccess.UNKNOWN;
        /// <summary>
        /// Access Data only use for Tag Read operation
        /// </summary>
        public readonly IBANK data;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">Access Result</param>
        /// <param name="bank">Access bank</param>
        /// <param name="access">Access type</param>
        /// <param name="data">Access Data only use for Tag Read operation</param>
        public OnAccessCompletedEventArgs(bool success, Bank bank, TagAccess access, IBANK data)
        {
            this.access = access;
            this.success = success;
            this.bank = bank;
            this.data = data;
        }
    }

    /// <summary>
    /// Reader Operation changed EventArgs
    /// </summary>
    public class OnStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Current operation state
        /// </summary>
        public readonly RFState state = RFState.IDLE;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        public OnStateChangedEventArgs(RFState state)
        {
            this.state = state;
        }
    }

    /// <summary>
    /// Current position of updating
    /// </summary>
    public class OnFirmwareUpgradeEventArgs : EventArgs
    {
        /// <summary>
        /// Current operation state
        /// </summary>
        public readonly UInt64 CurrentUpdateOffset = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        public OnFirmwareUpgradeEventArgs(UInt64 Offset)
        {
            this.CurrentUpdateOffset = Offset;
        }
    }

    /*
    /// <summary>
    /// Inventory or tag search callback event argument
    /// </summary>
    public class TagInventoryEventArgs : EventArgs
    {
        /// <summary>
        /// Tag inventory record
        /// </summary>
        public readonly TagCallbackInfo record = new TagCallbackInfo();
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="record"></param>
        public TagInventoryEventArgs(TagCallbackInfo record)
        {
            this.record = record;
        }
    }
    
    /// <summary>
    /// ranging tag callback event argument
    /// </summary>
    public class TagSearchAllEventArgs : EventArgs
    {
        /// <summary>
        /// Tag inventory record
        /// </summary>
        public readonly TagCallbackInfo record = new TagCallbackInfo();
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="record"></param>
        public TagSearchAllEventArgs(TagCallbackInfo record)
        {
            this.record = record;
        }
    }

    /// <summary>
    /// ranging tag callback event argument
    /// </summary>
    public class TagSearchOneEventArgs : EventArgs
    {
        /// <summary>
        /// Tag inventory record
        /// </summary>
        public readonly TagCallbackInfo record = new TagCallbackInfo();
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="record"></param>
        public TagSearchOneEventArgs(TagCallbackInfo record)
        {
            this.record = record;
        }
    }*/
    /*
    /// <summary>
    /// Tag write callback event argument
    /// </summary>
    public class TagWriteEventArgs : EventArgs
    {
        private bool success;

        private Bank bank = Bank.UNKNOWN;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">written successful or not</param>
        /// <param name="bank">Input TagAccess Data</param>
        public TagWriteEventArgs(bool success, Bank bank)
        {
            this.success = success;
            this.bank = bank;
        }
        /// <summary>
        /// Write Result
        /// </summary>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
        /// <summary>
        /// which bank currently write
        /// </summary>
        public Bank BankToWrite
        {
            get { return bank; }
        }
    }*/

#if NOUSE
    /// <summary>
    /// Tag lock callback event argument
    /// </summary>
    public class TagLockEventArgs : EventArgs
    {
        private bool success;

        private Bank bank = Bank.UNKNOWN;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">written successful or not</param>
        /// <param name="bank">Input TagAccess Data</param>
        public TagLockEventArgs(bool success, Bank bank)
        {
            this.success = success;
            this.bank = bank;
        }
        /// <summary>
        /// Read Result
        /// </summary>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
        /// <summary>
        /// which bank currently read
        /// </summary>
        public Bank BankToLock
        {
            get { return bank; }
        }

    }

    public class TagKillEventArgs : EventArgs
    {
        private bool success;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">written successful or not</param>
        public TagKillEventArgs(bool success)
        {
            this.success = success;
        }
        /// <summary>
        /// Read Result
        /// </summary>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
    }

    /// <summary>
    /// Tag access callback event argument
    /// </summary>
    public class TagAccessEventArgs : EventArgs
    {
        private TAG_ACCESS_RECORD Data;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Input TagAccess Data</param>
        public TagAccessEventArgs(TAG_ACCESS_RECORD data)
        {
            Data = data;
        }
        /// <summary>
        /// TagAccessInformation
        /// </summary>
        public TAG_ACCESS_RECORD TagAccessInformation
        {
            get { return Data; }
            set { Data = value; }
        }
    }
    public class GetTempEventArgs : EventArgs
    {
        private TemperatureDataStruct Data;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Input TagAccess Data</param>
        public GetTempEventArgs(TemperatureDataStruct data)
        {
            Data = data;
        }
        /// <summary>
        /// TagAccessInformation
        /// </summary>
        public TemperatureDataStruct GetTempInformation
        {
            get { return Data; }
            set { Data = value; }
        }
    }
#endif

#if nouse
    /// <summary>
    /// CRCErrorEventArgs
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        private ErrorType err = ErrorType.UNKNOWN;
        private ErrorCode ErrCode =  ErrorCode.UNKNOWN;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Err">Input Error Type</param>
        public ErrorEventArgs(TAG_ERROR_RECORD Err)
        {
            err = Err.ErrorType;
            ErrCode = Err.ErrorCode;
        }
        /// <summary>
        /// ErrorType
        /// </summary>
        public ErrorType ErrorType
        {
            get { return err; }
            //set { err = value; }
        }
        /// <summary>
        /// Error Code
        /// </summary>
        public ErrorCode ErrorCode
        {
            get { return ErrCode; }
        }
    }
#endif
}
