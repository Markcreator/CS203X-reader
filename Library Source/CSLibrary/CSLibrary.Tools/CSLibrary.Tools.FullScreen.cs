#if WindowsCE
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CSLibrary.Tools
{
    /// <summary>
    /// Fullscreen API
    /// </summary>
    public class FullScreen
    {
        private const int SPI_SETWORKAREA = 47;

        private const int SPI_GETWORKAREA = 48;

        private const int SPIF_UPDATEINIFILE = 0x01;

        private static IntPtr hWndInputPanel;

        private static IntPtr hWndSipButton;

        private static IntPtr hWndTaskBar;

        private static RECT rtDesktop;

        private static RECT rtNewDesktop;

        private static RECT rtInputPanel;

        private static RECT rtSipButton;

        private static RECT rtTaskBar;

        private static int m_width = 0;

        private static int m_height = 0;

        private static bool bInitial = false;

        [DllImport("coredll.dll")]
        extern private static IntPtr FindWindowW(string lpClassName, string lpWindowName);

        [DllImport("coredll.dll")]
        extern private static int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, int bRepaint);

        [DllImport("coredll.dll")]
        extern private static int SetRect(ref RECT lprc, int xLeft, int yTop, int xRight, int yBottom);

        [DllImport("coredll.dll")]
        extern private static int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("coredll.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern private static bool SystemParametersInfo(int uiAction, int uiParam, ref RECT pvParam, int fWinIni);
        /// <summary>
        /// Construction
        /// </summary>
        private static void Init()
        {
            // Declare & Instatiate local variable
            try
            {
                m_width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                m_height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

                if (SystemParametersInfo(SPI_GETWORKAREA, 0, ref rtDesktop, 0))
                {
                    // // Successful obtain the system working area (Desktop)
                    SetRect(ref rtNewDesktop, 0, 0, m_width, m_height);
                }
                // // Find the Input panel window handle
                hWndInputPanel = FindWindowW("SipWndClass", null);
                // // Checking...
                if ((hWndInputPanel.ToInt64() != 0))
                {
                    // // Get the original Input panel window size
                    GetWindowRect(hWndInputPanel, ref rtInputPanel);
                }
                // // Find the SIP Button window handle
                hWndSipButton = FindWindowW("MS_SIPBUTTON", null);
                // // Checking...
                if ((hWndSipButton.ToInt64() != 0))
                {
                    // // Get the original Input panel window size
                    GetWindowRect(hWndSipButton, ref rtSipButton);
                }
                // // Find the Taskbar window handle
                hWndTaskBar = FindWindowW("HHTaskBar", null);
                // // Checking...
                if ((hWndTaskBar.ToInt64() != 0))
                {
                    // // Get the original Input panel window size
                    GetWindowRect(hWndTaskBar, ref rtTaskBar);
                }

                bInitial = true;
            }
            catch (System.Exception ex)
            {
                // // PUT YOUR ERROR LOG CODING HERE
                // // Set return value
                //Result = 1;
#if DEBUG
                    CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("FullScreen.Init()", ex);
#endif
            }
            // // Return result code
            //return Result;
        }

        /// <summary>
        /// Start Fullscreen
        /// </summary>
        /// <returns></returns>
        public static int Start()
        {
            // // Declare & Instatiate local variable
            int Result = 0;

            if (!bInitial)
            {
                Init();
            }

            try
            {
                // // Update window working area size
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref rtNewDesktop, SPIF_UPDATEINIFILE);
                if ((hWndTaskBar.ToInt64() != 0))
                {
                    // // Hide the TaskBar
                    MoveWindow(hWndTaskBar, 0, rtNewDesktop.bottom, (rtTaskBar.right - rtTaskBar.left), (rtTaskBar.bottom - rtTaskBar.top), 0);
                }
                if ((hWndInputPanel.ToInt64() != 0))
                {
                    // // Reposition the input panel
                    MoveWindow(hWndInputPanel, 0, (rtNewDesktop.bottom
                    - (rtInputPanel.bottom - rtInputPanel.top)), (rtInputPanel.right - rtInputPanel.left), (rtInputPanel.bottom - rtInputPanel.top), 0);
                }
                if ((hWndSipButton.ToInt64() != 0))
                {
                    // // Hide the SIP button
                    MoveWindow(hWndSipButton, 0, rtNewDesktop.bottom, (rtSipButton.right - rtSipButton.left), (rtSipButton.bottom - rtSipButton.top), 0);
                }

            }
            catch (System.Exception ex)
            {
                // // PUT YOUR ERROR LOG CODING HERE
                // // Set return value
                Result = 1;
#if DEBUG
                CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("FullScreen.Start()", ex);
#endif
            }
            // // Return result code
            return Result;
        }
        /// <summary>
        /// Stop fullScreen
        /// </summary>
        /// <returns></returns>
        public static int Stop()
        {
            // // Update window working area size
            SystemParametersInfo(SPI_SETWORKAREA, 0, ref rtDesktop, SPIF_UPDATEINIFILE);
            // // Restore the TaskBar
            if ((hWndTaskBar.ToInt64() != 0))
            {
                MoveWindow(hWndTaskBar, rtTaskBar.left, rtNewDesktop.bottom - (rtTaskBar.bottom - rtTaskBar.top), (rtTaskBar.right - rtTaskBar.left), (rtTaskBar.bottom - rtTaskBar.top), 0);
            }
            // // Restore the input panel
            if ((hWndInputPanel.ToInt64() != 0))
            {
                MoveWindow(hWndInputPanel, rtInputPanel.left, (rtDesktop.bottom
                - ((rtInputPanel.bottom - rtInputPanel.top)
                - (rtTaskBar.bottom - rtTaskBar.top))), (rtInputPanel.right - rtInputPanel.left), (rtInputPanel.bottom - rtInputPanel.top), 0);
            }
            if ((hWndSipButton.ToInt64() != 0))
            {
                // // Restore the SIP button
                MoveWindow(hWndSipButton, rtSipButton.left, rtSipButton.top, (rtSipButton.right - rtSipButton.left), (rtSipButton.bottom - rtSipButton.top), 0);
            }
            return 0;
        }

        /// <summary>
        /// RECT
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            /// <summary>
            /// Left position
            /// </summary>
            public int left;
            /// <summary>
            /// Top position
            /// </summary>
            public int top;
            /// <summary>
            /// Right position
            /// </summary>
            public int right;
            /// <summary>
            /// Bottom position
            /// </summary>
            public int bottom;
        }
    }
}
#endif