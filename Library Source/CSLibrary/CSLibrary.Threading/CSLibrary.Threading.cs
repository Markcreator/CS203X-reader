using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace CSLibrary.Threading
{
    /// <summary>
    /// Enhanced alternative to the <see cref="T:System.Threading.Monitor"/> class.
    /// Provides a mechanism that synchronizes access to objects.
    /// </summary>
    /// <seealso cref="T:System.Threading.Monitor"/>
    internal sealed class MonitorEx
    {
        public const uint INFINITE = 0xffffffff;
        public const uint WAIT_TIMEOUT = 258;
        public const uint WAIT_FAILED = 0xffffffff;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent
        (
            IntPtr lpEventAttributes,
            [In, MarshalAs(UnmanagedType.Bool)] bool bManualReset,
            [In, MarshalAs(UnmanagedType.Bool)] bool bIntialState,
            [In, MarshalAs(UnmanagedType.BStr)] string lpName
        );

        [DllImport("coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hEvent);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern Int32 WaitForSingleObject(IntPtr hEvent, Int32 Wait);

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern uint WaitForMultipleObjects(uint cObjects, IntPtr[] hEvent, bool fWaitAll, uint dwTimeout);

        [DllImport("coredll.dll", SetLastError = true, EntryPoint = "EventModify")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetEvent(IntPtr hEvent, [In] EventFlags dEvent);
        public enum EventFlags
        {
            PULSE = 1,
            RESET = 2,
            SET = 3
        }
    }
}
