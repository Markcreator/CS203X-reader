////////////////////////////////////////////////////////////////////////////////
//  INTEL CONFIDENTIAL
//  Copyright 2006, 2007 Intel Corporation All Rights Reserved.
//
//  The source code contained or described herein and all documents related to
//  the source code ("Material") are owned by Intel Corporation or its suppliers
//  or licensors. Title to the Material remains with Intel Corporation or its
//  suppliers and licensors. The Material may contain trade secrets and
//  proprietary and confidential information of Intel Corporation and its
//  suppliers and licensors, and is protected by worldwide copyright and trade
//  secret laws and treaty provisions. No part of the Material may be used,
//  copied, reproduced, modified, published, uploaded, posted, transmitted,
//  distributed, or disclosed in any way without Intel's prior express written
//  permission. 
//  
//  No license under any patent, copyright, trade secret or other intellectual
//  property right is granted to or conferred upon you by disclosure or delivery
//  of the Materials, either expressly, by implication, inducement, estoppel or
//  otherwise. Any license under such intellectual property rights must be
//  express and approved by Intel in writing.
//
//  Unless otherwise agreed by Intel in writing, you may not remove or alter
//  this notice or any other notice embedded in Materials by Intel or Intel's
//  suppliers or licensors in any way.
////////////////////////////////////////////////////////////////////////////////


#if NOUSE
using System;
using System.Collections.Generic;
using System.Text;



namespace Reader.OEM
{
    /// <summary>
    /// Reader OEM Operation
    /// </summary>
    public class OEM
        :
        Object
    {

        // Mirrored from oemcfg.h
        /// <summary>
        /// OEMCFG_MFG_NAME_WORD_LEN = 40
        /// </summary>
        public const Int32 OEMCFG_MFG_NAME_WORD_LEN = 40;
        /// <summary>
        /// OEMCFG_PROD_NAME_WORD_LEN = 40
        /// </summary>
        public const Int32 OEMCFG_PROD_NAME_WORD_LEN = 40;
        /// <summary>
        /// OEMCFG_SERIAL_NUM_WORD_LEN = 40
        /// </summary>
        public const Int32 OEMCFG_SERIAL_NUM_WORD_LEN = 40;
        /// <summary>
        /// OEMCFGADDR_PBA_NUM_LEN = 4
        /// </summary>
        public const Int32 OEMCFGADDR_PBA_NUM_LEN = 4;

        // Mirrored from oemcfg.h

        protected enum OEMCFG_ADDRS : int
        {
            OEMCFGADDR_FIRST = 0,
            OEMCFGADDR_WW_YEAR = OEMCFGADDR_FIRST,
            OEMCFGADDR_MON_DAY_MIN_SEC,
            OEMCFGADDR_VEND_SITE_CODE1,
            OEMCFGADDR_VEND_SITE_CODE2,
            OEMCFGADDR_PBA_NUM_BASE,
            OEMCFGADDR_CHECKSUM = (OEMCFGADDR_PBA_NUM_BASE + OEMCFGADDR_PBA_NUM_LEN),
            OEMCFGADDR_SKU_CAP_CODE,
            OEMCFGADDR_OEM_CODE,
            OEMCFGADDR_OEM_VERSION,
            OEMCFGADDR_OEM_CONTENT_ID,
            OEMCFGADDR_USB_VID,
            OEMCFGADDR_USB_PID,
            OEMCFGADDR_MFG_NAME_BASE,
            OEMCFGADDR_PROD_NAME_BASE = (OEMCFGADDR_MFG_NAME_BASE + OEMCFG_MFG_NAME_WORD_LEN),
            OEMCFGADDR_SERIAL_NUM_BASE = (OEMCFGADDR_PROD_NAME_BASE + OEMCFG_PROD_NAME_WORD_LEN),
            OEMCFGADDR_NA_TX_PWR_CAL_LOW = (OEMCFGADDR_SERIAL_NUM_BASE + OEMCFG_SERIAL_NUM_WORD_LEN),
            OEMCFGADDR_NA_TX_PWR_CAL_MID,
            OEMCFGADDR_NA_TX_PWR_CAL_HI,
            OEMCFGADDR_EU_TX_PWR_CAL_LOW,
            OEMCFGADDR_EU_TX_PWR_CAL_MID,
            OEMCFGADDR_EU_TX_PWR_CAL_HI,
            OEMCFGADDR_APAC_TX_PWR_CAL_LOW,
            OEMCFGADDR_APAC_TX_PWR_CAL_MID,
            OEMCFGADDR_APAC_TX_PWR_CAL_HI,
            OEMCFGADDR_ADC_REFERENCE_VOLTAGE,
            OEMCFGADDR_RFCAL_FWDPWR_C0,
            OEMCFGADDR_RFCAL_FWDPWR_C1,
            OEMCFGADDR_RFCAL_FWDPWR_C2,
            OEMCFGADDR_GGAIN_NEG7,
            OEMCFGADDR_GGAIN_NEG5,
            OEMCFGADDR_GGAIN_NEG3,
            OEMCFGADDR_GGAIN_NEG1,
            OEMCFGADDR_GGAIN_PLUS1,
            OEMCFGADDR_GGAIN_PLUS3,
            OEMCFGADDR_GGAIN_PLUS5,
            OEMCFGADDR_GGAIN_PLUS7,
            OEMCFGADDR_RSSI_THRESHOLD,
            OEMCFGADDR_STANDARD_SEL,
            OEMCFGADDR_LAST = OEMCFGADDR_STANDARD_SEL
        };

        // Field name mirrored from oemcfg.h but size differs as we
        // are excluding fields after serial number
        /// <summary>
        /// OEMCFG_AREA_MAP_SIZE_WORDS
        /// </summary>
        public static Int32 OEMCFG_AREA_MAP_SIZE_WORDS
            = (11)
            + (OEMCFGADDR_PBA_NUM_LEN)
            + (OEMCFG_MFG_NAME_WORD_LEN)
            + (OEMCFG_PROD_NAME_WORD_LEN)
            + (OEMCFG_SERIAL_NUM_WORD_LEN);
        /// <summary>
        /// MAXOEMDATASIZE = 0x1F80;
        /// </summary>
        public static Int32 MAXOEMDATASIZE = 0x1F80;

        protected UInt32[] oemDataBuffer;// = new UInt32[MAXOEMDATASIZE/*OEMCFG_AREA_MAP_SIZE_WORDS*/];


        public UInt32[] OemData
        {
            get { return oemDataBuffer; }
        }

        public OEM()
            :
            base()
        {
            // NOP
        }


        /*********************************************************************
         * 
         * Int32 usbStyleStringToUIntArray( ... )
         * 
         * Convert the incoming source string from standard format to usb
         * format and store in the target UInt32 array.
         * 
         * Parameters:
         * 
         *   source - the source string to convert
         * 
         *   target - the target UInt32 array where the converted string will
         *            be stored
         * 
         *   length - the length of the target i.e. count of UInt32 slots
         *            available.
         * 
         * The usb 'format' utilized is:
         * 
         * UInt8  : length
         * UInt8  : type ( hardcode to 3 )
         * UInt16 : unicode characters in little endian format
         * 
         * On return the num of characters stored or -1 if failure.  Failure
         * occurs if conversion of the input source requires more slots than
         * allowed by the specified length.
         * 
         ********************************************************************/

        protected Int32 stringToUInt32Array
        (
            String source,
            UInt32[] target,
            UInt32 offset,
            UInt32 length
        )
        {
            Byte[] encodedChars = Encoding.Unicode.GetBytes(source);

            // Following is clumsy but other option is make entire module run
            // in 'unsafe' mode to allow casting byte to int array via IntPtr

            Array.Clear(target, (int)offset, (int)length);

            target[offset] = (UInt32)((0x02 + encodedChars.Length) | 0x03 << 0x08);

            if (0 != encodedChars.Length)
            {
                target[offset] =
                    target[offset] |
                    ((UInt32)encodedChars[0] | (UInt32)encodedChars[1] << 0x08) << 0x10;
            }

            for (int index = 1; index < encodedChars.Length / 2; ++index)
            {
                target[offset + ((index + 1) / 2)] |=
                    (1 == (index % 2)) ?
                    ((UInt32)encodedChars[index * 2] | (UInt32)encodedChars[index * 2 + 1] << 0x08) :
                    ((UInt32)encodedChars[index * 2] | (UInt32)encodedChars[index * 2 + 1] << 0x08) << 0x10;

            }

            return 0;
        }


        protected String uint32ArrayToString
        (
            UInt32[] source,
            UInt32 offset
        )
        {
            StringBuilder sb = new StringBuilder();

            // Byte at offset is total byte len, 2nd byte is always 3

            for (int index = 2; index < (Int32)(source[(int)offset] & 0x0FF); index += 2)
            {
                sb.Append
                (
                    (Char)(
                        (0 == (index % 4)) ?
                        (source[offset + (index / 4)] & 0x0000FFFF) :
                        (source[offset + (index / 4)] & 0xFFFF0000) >> 0x10
                    )
                );

            }

            return sb.ToString();
        }
        /// <summary>
        /// Load OEM Data
        /// </summary>
        /// <param name="link">Reader Linkage</param>
        /// <param name="readerHandle">Reader Handle</param>
        /// <param name="Address">Start address</param>
        /// <param name="Size">Size</param>
        /// <returns>Return OK if Success</returns>
        public CSLibrary.Constants.Result load
        (
            rfid.Linkage link,
            Int32 readerHandle,
            Int32 Address,
            Int32 Size
        )
        {
            UInt32[] buffer = new UInt32[Size/*OEMCFG_AREA_MAP_SIZE_WORDS*/];
            oemDataBuffer = new UInt32[Size];
            CSLibrary.Constants.Result status = link.MacReadOemData
                (
                    readerHandle,
                    (UInt32)Address,
                    (UInt32)buffer.Length,
                    buffer
                );

            if (CSLibrary.Constants.Result.OK == status)
            {
                buffer.CopyTo(oemDataBuffer, 0);
            }

            return status;
        }


        /// <summary>
        /// Store Data to OEM...(Warning: Don't use this function)
        /// OEM Data is writed by Factory
        /// </summary>
        /// <param name="link">Reader Linkage</param>
        /// <param name="readerHandle">Reader Handle</param>
        /// <returns>Return OK if Success</returns>
        public CSLibrary.Constants.Result store
        (
            rfid.Linkage link,
            Int32 readerHandle
        )
        {
            CSLibrary.Constants.Result status = link.MacWriteOemData
                (
                    readerHandle,
                    0,
                    (UInt32)oemDataBuffer.Length,
                    oemDataBuffer
                );

            return status;
        }


        protected const UInt32 YEAR_MASK = 0x0000FFFF;
        protected const Int32 YEAR_SHFT = 0x00;
        protected const UInt32 WEEK_MASK = 0x00FF0000;
        protected const Int32 WEEK_SHFT = 0x10;

        /// <summary>
        /// Year
        /// </summary>
        public UInt32 Year
        {
            get
            {
                return (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_WW_YEAR] & YEAR_MASK) >> YEAR_SHFT; ;
            }
            set
            {
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_WW_YEAR]
                    = (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_WW_YEAR] & (~YEAR_MASK)) | (value << YEAR_SHFT);
            }
        }
        /// <summary>
        /// WorkWeek
        /// </summary>
        public UInt32 WorkWeek
        {
            get
            {
                return (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_WW_YEAR] & WEEK_MASK) >> WEEK_SHFT;
            }
            set
            {
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_WW_YEAR]
                    = (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_WW_YEAR] & (~WEEK_MASK)) | (value << WEEK_SHFT);
            }
        }

        protected const UInt32 MONTH_MASK = 0x0000000F;
        protected const Int32 MONTH_SHFT = 0x00;
        protected const UInt32 DAY_MASK = 0x000000F0;
        protected const Int32 DAY_SHFT = 0x04;
        protected const UInt32 MIN_MASK = 0x000FFF00;
        protected const Int32 MIN_SHFT = 0x08;
        protected const UInt32 SEC_MASK = 0x0FF00000;
        protected const Int32 SEC_SHFT = 0x14;
        /// <summary>
        /// Month
        /// </summary>
        public UInt32 Month
        {
            get
            {
                return (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & MONTH_MASK) >> MONTH_SHFT; ;
            }
            set
            {
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC]
                    = (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & (~MONTH_MASK)) | (value << MONTH_SHFT);
            }
        }
        /// <summary>
        /// Day
        /// </summary>
        public UInt32 Day
        {
            get
            {
                return (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & DAY_MASK) >> DAY_SHFT; ;
            }
            set
            {
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC]
                    = (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & (~DAY_MASK)) | (value << DAY_SHFT);
            }
        }
        /// <summary>
        /// Minute
        /// </summary>
        public UInt32 Minute
        {
            get
            {
                return (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & MIN_MASK) >> MIN_SHFT; ;
            }
            set
            {
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC]
                    = (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & (~MIN_MASK)) | (value << MIN_SHFT);
            }
        }
        /// <summary>
        /// Second
        /// </summary>
        public UInt32 Second
        {
            get
            {
                return (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & SEC_MASK) >> SEC_SHFT; ;
            }
            set
            {
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC]
                    = (this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_MON_DAY_MIN_SEC] & (~SEC_MASK)) | (value << SEC_SHFT);
            }
        }
        /// <summary>
        /// VendorSiteCode1(Contory Code)
        /// </summary>
        public UInt32 VendorSiteCode1
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_VEND_SITE_CODE1]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_VEND_SITE_CODE1] = value; }
        }
        /// <summary>
        /// VendorSiteCode2
        /// </summary>
        public UInt32 VendorSiteCode2
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_VEND_SITE_CODE2]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_VEND_SITE_CODE2] = value; }
        }
        /// <summary>
        /// AssemblyNumber
        /// </summary>
        public UInt32[] AssemblyNumber
        {
            get
            {
                return new UInt32[OEMCFGADDR_PBA_NUM_LEN]
                { 
                    this.oemDataBuffer[ ( int ) OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 0 ],
                    this.oemDataBuffer[ ( int ) OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 1 ],
                    this.oemDataBuffer[ ( int ) OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 2 ],
                    this.oemDataBuffer[ ( int ) OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 3 ]
                };
            }
            set
            {
                UInt32[] valArray = value as UInt32[];

                if (null == valArray || OEMCFGADDR_PBA_NUM_LEN != valArray.Length)
                {
                    Console.WriteLine("Assembly Number : set value bad format");
                }

                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 0] = valArray[0];
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 1] = valArray[1];
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 2] = valArray[2];
                this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_PBA_NUM_BASE + 3] = valArray[3];
            }
        }
        /// <summary>
        /// Checksum
        /// </summary>
        public UInt32 Checksum
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_CHECKSUM]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_CHECKSUM] = value; }
        }
        /// <summary>
        /// SKUCapabilityCode
        /// </summary>
        public UInt32 SKUCapabilityCode
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_SKU_CAP_CODE]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_SKU_CAP_CODE] = value; }
        }
        /// <summary>
        /// OemCode
        /// </summary>
        public UInt32 OemCode
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_OEM_CODE]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_OEM_CODE] = value; }
        }
        /// <summary>
        /// OemVersion
        /// </summary>
        public UInt32 OemVersion
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_OEM_VERSION]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_OEM_VERSION] = value; }
        }
        /// <summary>
        /// OemContentID
        /// </summary>
        public UInt32 OemContentID
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_OEM_CONTENT_ID]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_OEM_CONTENT_ID] = value; }
        }
        /// <summary>
        /// UsbVendorID
        /// </summary>
        public UInt32 UsbVendorID
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_USB_VID]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_USB_VID] = value; }
        }
        /// <summary>
        /// UsbProductID
        /// </summary>
        public UInt32 UsbProductID
        {
            get { return this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_USB_PID]; }
            set { this.oemDataBuffer[(int)OEMCFG_ADDRS.OEMCFGADDR_USB_PID] = value; }
        }
        /// <summary>
        /// Manufacturer
        /// </summary>
        public String Manufacturer
        {
            get
            {
                return uint32ArrayToString
                    (
                        this.oemDataBuffer,
                        (UInt32)OEMCFG_ADDRS.OEMCFGADDR_MFG_NAME_BASE
                    );
            }
            set
            {
                Int32 result = stringToUInt32Array
                    (
                        value,
                        this.oemDataBuffer,
                        (UInt32)OEMCFG_ADDRS.OEMCFGADDR_MFG_NAME_BASE,
                        OEMCFG_MFG_NAME_WORD_LEN
                    );
            }
        }
        /// <summary>
        /// Product
        /// </summary>
        public String Product
        {
            get
            {
                return uint32ArrayToString
                    (
                        this.oemDataBuffer,
                        (UInt32)OEMCFG_ADDRS.OEMCFGADDR_PROD_NAME_BASE
                    );
            }
            set
            {
                Int32 result = stringToUInt32Array
                (
                    value,
                    this.oemDataBuffer,
                    (UInt32)OEMCFG_ADDRS.OEMCFGADDR_PROD_NAME_BASE,
                    OEMCFG_PROD_NAME_WORD_LEN
                );
            }
        }
        /// <summary>
        /// SerialNumber
        /// </summary>
        public String SerialNumber
        {
            get
            {
                return uint32ArrayToString
                    (
                        this.oemDataBuffer,
                        (UInt32)OEMCFG_ADDRS.OEMCFGADDR_SERIAL_NUM_BASE
                    );
            }
            set
            {
                Int32 result = stringToUInt32Array
                (
                    value,
                    this.oemDataBuffer,
                    (UInt32)OEMCFG_ADDRS.OEMCFGADDR_SERIAL_NUM_BASE,
                    OEMCFG_SERIAL_NUM_WORD_LEN
                );
            }
        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} ({1})", Product, SerialNumber);
        }
    }
}

#endif