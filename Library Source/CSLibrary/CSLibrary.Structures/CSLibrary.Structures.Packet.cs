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
using System.Xml;
using CSLibrary.Constants;

namespace CSLibrary.Structures
{
    /******************************************************************************
     * Name:  CONTEXT_PARMS - Custom Type for access
     ******************************************************************************/
    [StructLayout(LayoutKind.Explicit)]
    struct CONTEXT_PARMS
    {
        [FieldOffset(0)]
        public bool  operationSucceeded;
        [FieldOffset(4)]
        public byte [] pReadData;
    };

    /// <summary>
    /// The common packet preamble that contains fields that are common to all packets.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct RFID_PACKET_COMMON
    {
        /// <summary>
        /// Packet specific version number 
        /// </summary>
        [FieldOffset(0)]
        public byte pkt_ver;
        /// <summary>
        /// Packet specific flags
        /// </summary>
        [FieldOffset(1)]
        public byte flags;
        /// <summary>
        /// Packet type identifier
        /// </summary>
        [FieldOffset(2)]
        public RFID_PACKET_TYPE pkt_type;
        /// <summary>
        /// <para>Packet length indicator - number of 32-bit words that follow the common</para>
        /// <para>packet preamble (i.e., this struct) </para>
        /// </summary>
        [FieldOffset(4)]
        public UInt16 pkt_len;
        /// <summary>
        /// Reserved for future use
        /// </summary>
        [FieldOffset(6)]
        public UInt16 res0;
    }

    /******************************************************************************
     * Name:  RFID_PACKET_COMMAND_BEGIN - The command-begin packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_COMMAND_BEGIN
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* The command for which the packet sequence is in response to              */
        public UInt32 command;
        /* Current millisecond timer/counter                                        */
        public UInt32 ms_ctr;
    }


    /******************************************************************************
     * Name:  RFID_PACKET_COMMAND_END - The command-end packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_COMMAND_END
    {
        /// Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        public UInt32 ms_ctr;
        /* Command status indicator                                                 */
        public UInt32 status;
    }

    /******************************************************************************
     * Name:  RFID_PACKET_ANTENNA_CYCLE_BEGIN - The antenna-cycle-begin packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_ANTENNA_CYCLE_BEGIN
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* No other packet specific fields                                          */
    }

    /******************************************************************************
     * Name:  RFID_PACKET_ANTENNA_CYCLE_END - The antenna cycle-end packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_ANTENNA_CYCLE_END
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* No other packet specific fields                                          */
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_ANTENNA_BEGIN - The antenna-begin packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_ANTENNA_BEGIN
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* The logical antenna ID                                                   */
        public UInt32 antenna;
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_ANTENNA_END - The antenna-end packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_ANTENNA_END
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* No other packet specific fields                                          */
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_INVENTORY_CYCLE_BEGIN - The inventory-cycle-begin packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_INVENTORY_CYCLE_BEGIN
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        public UInt32 ms_ctr;
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_INVENTORY_CYCLE_END - The inventory-cycle-end packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_INVENTORY_CYCLE_END
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        public UInt32 ms_ctr;
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_INVENTORY_CYCLE_END_DIAGS - The inventory-cycle-end
     *        diagnostics packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_INVENTORY_CYCLE_END_DIAGS
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Number of query's issued */
        public UInt32 querys;
        /* Number of RN16's received */
        public UInt32 rn16rcv;
        /* Number of RN16 timeouts (i.e., no detected response to ISO 18000-6C      */
        /* Query or QueryRep)                                                       */
        public UInt32 rn16to;
        /* Number of EPC timeouts (i.e., no detected response to ISO 18000-6C RN16  */
        /* ACK)                                                                     */
        public UInt32 epcto;
        /* Number of good EPC reads */
        public UInt32 good_reads;
        /* Number of CRC failures                                                   */
        public UInt32 crc_failures;
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_INVENTORY_ROUND_BEGIN - The ISO 18000-6C inventory-
     *        round-begin packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_18K6C_INVENTORY_ROUND_BEGIN
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* No packet specific fields                                                */
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_INVENTORY_ROUND_END - The ISO 18000-6C inventory-
     *        round-end packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_18K6C_INVENTORY_ROUND_END
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* No packet specific fields                                                */
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_INVENTORY_ROUND_BEGIN_DIAGS - The ISO 18000-6C
     *        inventory-round-begin diagnostics packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_18K6C_INVENTORY_ROUND_BEGIN_DIAGS
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        public UInt32 ms_ctr;
        /* Starting singulation parameters                                          */
        public UInt32 sing_params;
    } ;


    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_INVENTORY_ROUND_END_DIAGS - The ISO 18000-6C
     *        inventory-round-end diagnostics packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_18K6C_INVENTORY_ROUND_END_DIAGS
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        public UInt32 ms_ctr;
        /* Number of query's issued */
        public UInt32 querys;
        /* Number of RN16's received */
        public UInt32 rn16rcv;
        /* Number of RN16 timeouts (i.e., no detected response to ISO 18000-6C      */
        /* Query or QueryRep)                                                       */
        public UInt32 rn16to;
        /* Number of EPC timeouts (i.e., no detected response to ISO 18000-6C RN16  */
        /* ACK)                                                                     */
        public UInt32 epcto;
        /* Number of good EPC reads */
        public UInt32 good_reads;
        /* Number of CRC failures                                                   */
        public UInt32 crc_failures;
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_INVENTORY - The ISO 18000-6C inventory packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Explicit)]
    struct RFID_PACKET_18K6C_INVENTORY
    {
        /* Common preamble - part of every packet!                                  */
        [FieldOffset(0)]
        public RFID_PACKET_COMMON cmn;
        /* current millisecond timer/counter                                        */
        [FieldOffset(8)]
        public UInt32 ms_ctr;
        /* Receive Signal Strength Indicator - backscattered tab signal */
        /* amplitude.                                                               */
        [FieldOffset(12)]
        public Byte wb_rssi;
        [FieldOffset(13)]
        public Byte nb_rssi;
        [FieldOffset(14)]
        public UInt16 ana_ctrl1;
        /* Reserved                                                                 */
        [FieldOffset(16)]
        public UInt16 truncate;
        [FieldOffset(18)]
        public UInt16 ana_port;
        /* Variable length inventory data (i.e., PC, EPC, and CRC)                  */
        [FieldOffset(20)]
        public UInt16 inv_data;
    }


    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_INVENTORY_DIAGS - The ISO 18000-6C inventory
     *        diagnostics packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_18K6C_INVENTORY_DIAGS
    {
        /* Common preamble - part of every packet!                                  */
        public RFID_PACKET_COMMON cmn;
        /* Protocol parameters                                                      */
        public UInt32 prot_parms;
    } ;



    /******************************************************************************
     * Name:  RFID_PACKET_18K6C_TAG_ACCESS - The ISO 18000-6C tag-access packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Explicit)]
    struct RFID_PACKET_18K6C_TAG_ACCESS
    {
        /* Common preamble - part of every packet!                                  */
        [FieldOffset(0)]
        public RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        [FieldOffset(8)]
        public UInt32 ms_ctr;
        /* ISO 18000-6C access command                                              */
        [FieldOffset(12)]
        public RFID_18K6C command;
        /* Error code from tag access                                               */
        [FieldOffset(13)]
        public TAG_BACKSCATTERED_ERROR error_code;
        /* Reserved                                                                 */
        [FieldOffset(14)]
        public UInt16 res0;
        [FieldOffset(16)]
        public UInt32 res1;
        /* Variable length access data                                              */
        [FieldOffset(20)]
        public UInt16 data;
    } ;





    /******************************************************************************
     * Name:  RFID_PACKET_NONCRITICAL_FAULT - The non-critical-fault packet.
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_NONCRITICAL_FAULT
    {
        /* Common preamble - part of every packet!                                  */
        RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        UInt32 ms_ctr;
        /* Fault type                                                               */
        UInt16 fault_type;
        /* Fault subtype                                                            */
        UInt16 fault_subtype;
        /* Context specific data for fault                                          */
        UInt32 context;
    } ;

    /******************************************************************************
     * Name:  RFID_PACKET_CARRIER_INFO (type = RFID_PACKET_TYPE_CARRIER_INFO)
     *        Contains info related to tranmit carrier
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_CARRIER_INFO
    {
        /* Common preamble - part of every packet!                                  */
        RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        UInt32 ms_ctr;
        /* current plldivmult setting                                               */
        UInt32 plldivmult;
        /* channel                                                                  */
        UInt16 chan;
        /* carrier flags                                                            */
        UInt16 cw_flags;
    } ;



    /******************************************************************************
     * Name:  RFID_PACKET_CYCCFG_EVT (type = RFID_PACKET_TYPE_CYCCFG_EVT)
     *        Contains info to cycle configuration events
     ******************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct RFID_PACKET_CYCCFG_EVT
    {
        /* Common preamble - part of every packet!                                  */
        RFID_PACKET_COMMON cmn;
        /* Current millisecond timer/counter                                        */
        UInt32 ms_ctr;
        /* current cycle configuration descriptor index                             */
        UInt32 descidx;
    } ;


}
