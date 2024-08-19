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

namespace CSLibrary.Net
{
    /// <summary>
    /// Netfinder information return from device
    /// </summary>
    public class DeviceInfomation
    {
        /// <summary>
        /// Reserved for future use
        /// </summary>
        public Mode Mode = Mode.Unknown;
        /// <summary>
        /// Total time on network
        /// </summary>
        public TimeEvent TimeElapsedNetwork = new TimeEvent();
        /// <summary>
        /// Total Power on time
        /// </summary>
        public TimeEvent TimeElapsedPowerOn = new TimeEvent();
        /// <summary>
        /// MAC address
        /// </summary>
        public MAC MACAddress = new MAC();//[6];
        /// <summary>
        /// IP address
        /// </summary>
        public IP IPAddress = new IP();
        /// <summary>
        /// Subnet Mask
        /// </summary>
        public IP SubnetMask = new IP();
        /// <summary>
        /// Gateway
        /// </summary>
        public IP Gateway = new IP();
        /// <summary>
        /// Trusted hist IP
        /// </summary>
        public IP TrustedServer = new IP();
        /// <summary>
        /// Inducated trusted server enable or not.
        /// </summary>
        public Boolean TrustedServerEnabled = false;
        /// <summary>
        /// UDP Port
        /// </summary>
        public ushort Port; // Get port from UDP header
        /*/// <summary>
        /// Reserved for future use, Server mode ip
        /// </summary>
        public byte[] serverip = new byte[4];*/
        /// <summary>
        /// enable or disable DHCP
        /// </summary>
        public bool DHCPEnabled;
        /*/// <summary>
        /// Reserved for future use, Server mode port
        /// </summary>
        public ushort serverport;*/
        /// <summary>
        /// DHCP retry
        /// </summary>
        public byte DHCPRetry;
        /// <summary>
        /// Device name, user can change it.
        /// </summary>
        public string DeviceName;
        /// <summary>
        /// Mode discription
        /// </summary>
        public string Description;
        /// <summary>
        /// Connect Mode
        /// </summary>        
        public byte ConnectMode;
        /// <summary>
        /// Gateway check reset mode
        /// </summary>
        public int GatewayCheckResetMode;
    }

    class AssignInfo
    {
        public IP IPAddress = new IP();
        public MAC MACAddress = new MAC();
        public string DeviceName;
        public bool DHCPEnabled;
        public byte DHCPRetry;
        public IP TrustedAddress = new IP();
        public bool TrustedEnabled;
        public IP SubnetMask = new IP();
        public IP Gateway = new IP();
        public int GatewayCheckResetMode = -1;
    }

    /// <summary>
    /// IP Structure
    /// </summary>
    public class IP
    {
        /// <summary>
        /// IP Address in 4 bytes format
        /// </summary>
        public byte[] Address = new byte[4];
        /// <summary>
        /// IP Address in String format, i.e. 127.0.0.1
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Address == null || Address.Length == 0) ? "Invalid IPAddress Formate" : 
                string.Format("{0}.{1}.{2}.{3}", Address[0], Address[1], Address[2], Address[3]);
        }
        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static implicit operator String(IP ip)
        {
            return ip.ToString();
        }
        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static implicit operator long(IP ip)
        {
            return (long)BitConverter.ToInt32(ip.Address, 0);
        }
    }
    /// <summary>
    /// MAC Structure
    /// </summary>
    public class MAC
    {
        /// <summary>
        /// Mac Address in 6 bytes format
        /// </summary>
        public byte[] Address = new byte[6];
        /// <summary>
        /// MAC Address in String format, i.e. 00:19:BB:44:7C:AA
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Address == null || Address.Length == 0) ? "Invalid IPAddress Formate" :
                string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", Address[0], Address[1], Address[2], Address[3], Address[4], Address[5]);
        }
        /// <summary>
        /// Check equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is MAC)
            {
                return Win32.memcmp(((MAC)obj).Address, Address, 6) == 0;
            }
            else
            {
                Byte[] array = (Byte[])obj;
                if (array != null)
                {
                    return Win32.memcmp((Byte[])obj, Address, 6) == 0;
                }
            }
            return false;
        }
        /// <summary>
        /// Convert to string
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public static implicit operator String(MAC mac)
        {
            return mac.ToString();
        }
        /// <summary>
        /// Parse String to Byte array format
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] Parse(String address)
        {
            if (String.IsNullOrEmpty(address))
                return null;
            String[] split = address.Split(':');
            if (split == null || split.Length != 6)
                return null;
            return new Byte[]{
                byte.Parse(split[0], System.Globalization.NumberStyles.HexNumber), 
                byte.Parse(split[1], System.Globalization.NumberStyles.HexNumber), 
                byte.Parse(split[2], System.Globalization.NumberStyles.HexNumber), 
                byte.Parse(split[3], System.Globalization.NumberStyles.HexNumber), 
                byte.Parse(split[4], System.Globalization.NumberStyles.HexNumber), 
                byte.Parse(split[5], System.Globalization.NumberStyles.HexNumber)};
        }
    }

    /// <summary>
    /// Time Event
    /// </summary>
    public struct TimeEvent
    {
        /// <summary>
        /// Event name
        /// </summary>
        public string name;
        /// <summary>
        /// Days
        /// </summary>
        public uint days;
        /// <summary>
        /// Hours
        /// </summary>
        public uint hours;
        /// <summary>
        /// Minutes
        /// </summary>
        public uint minutes;
        /// <summary>
        /// Seconds
        /// </summary>
        public uint seconds;
    }
    /// <summary>
    /// Netfinder Mode
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Application mode (Network device)
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Bootloader mode (Network device)
        /// </summary>
        Bootloader,
        /// <summary>
        /// Application mode (USB Device)
        /// </summary>
        NormalUsb,
        /// <summary>
        /// Application mode (Serial Device)
        /// </summary>
        NormalSerial,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    class DeviceCollection : List<DeviceInfomation>
    {
        public DeviceInfomation GetDeviceInfo(int index)
        {
            return index >= 0 && index < Count ? this[index] : null;
        }

        public DeviceInfomation GetDeviceInfo(MAC MACAddress)
        {
            int index = this.FindIndex(delegate(DeviceInfomation item) { return (item.MACAddress.Equals(MACAddress)); });
            if (index < 0)
                return null;
            return this[index];
        }
    }
}
