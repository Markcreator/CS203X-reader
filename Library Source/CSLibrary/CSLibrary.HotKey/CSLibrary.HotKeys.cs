#if WindowsCE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.WindowsCE.Forms;
namespace CSLibrary.HotKeys
{
    [Flags]
    enum eidHotKey : int
    {
        NONE = 0,
        IDHOT_SNAPDESKTOP = -2,
        IDHOT_SNAPWINDOW = -1,
    }
    [Flags]
    enum efsModifiers : uint
    {
        NONE = 0x0000,
        MOD_ALT = 0x0001,
        MOD_CONTROL = 0x0002,
        MOD_KEYUP = 0x1000, //Both key up & down events generate a WM_HOTKEY message. 
        MOD_SHIFT = 0x0004,
        MOD_WIN = 0x0008
    }
    /// <summary>
    /// Current Supported Virtual Key
    /// </summary>
    public enum Key : uint
    {
        /*LBUTTON = 0x01,
        RBUTTON = 0x02,
        CANCEL = 0x03,
        MBUTTON = 0x04,    // NOT contiguous with L & RBUTTON
        XBUTTON1 = 0x05,    // NOT contiguous with L & RBUTTON
        XBUTTON2 = 0x06,    // NOT contiguous with L & RBUTTON

        BACK = 0x08,
        TAB = 0x09,

        CLEAR = 0x0C,
        RETURN = 0x0D,

        SHIFT = 0x10,
        CONTROL = 0x11,
        MENU = 0x12,
        PAUSE = 0x13,
        CAPITAL = 0x14,

        KANA = 0x15,
        HANGEUL = 0x15,  // old name - should be here for compatibility
        HANGUL = 0x15,
        JUNJA = 0x17,
        FINAL = 0x18,
        HANJA = 0x19,
        KANJI = 0x19,

        ESCAPE = 0x1B,

        CONVERT = 0x1c,
        NOCONVERT = 0x1d,

        SPACE = 0x20,
        PRIOR = 0x21,
        NEXT = 0x22,
        END = 0x23,
        HOME = 0x24,
        LEFT = 0x25,
        UP = 0x26,
        RIGHT = 0x27,
        DOWN = 0x28,
        SELECT = 0x29,
        PRINT = 0x2A,
        EXECUTE = 0x2B,
        SNAPSHOT = 0x2C,
        INSERT = 0x2D,
        DELETE = 0x2E,
        HELP = 0x2F,
        // 0 thru 9 are the same as ASCII '0' thru '9' (= 0x30 - = 0x39)
        // A thru Z are the same as ASCII 'A' thru 'Z' (= 0x41 - = 0x5A)
        LWIN = 0x5B,
        RWIN = 0x5C,
        APPS = 0x5D,

        SLEEP = 0x5F,

        NUMPAD0 = 0x60,
        NUMPAD1 = 0x61,
        NUMPAD2 = 0x62,
        NUMPAD3 = 0x63,
        NUMPAD4 = 0x64,
        NUMPAD5 = 0x65,
        NUMPAD6 = 0x66,
        NUMPAD7 = 0x67,
        NUMPAD8 = 0x68,
        NUMPAD9 = 0x69,
        MULTIPLY = 0x6A,
        ADD = 0x6B,
        SEPARATOR = 0x6C,
        SUBTRACT = 0x6D,
        DECIMAL = 0x6E,
        DIVIDE = 0x6F,*/
        /// <summary>
        /// F1 Key
        /// </summary>
        F1 = 0x70,
        /// <summary>
        /// F2 Key
        /// </summary>
        F2 = 0x71,
        /// <summary>
        /// F3 Key
        /// </summary>
        F3 = 0x72,
        /// <summary>
        /// F4 Key
        /// </summary>
        F4 = 0x73,
        /// <summary>
        /// F5 Key
        /// </summary>
        F5 = 0x74,
        /*F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,*/
        /// <summary>
        /// F11 Key
        /// </summary>
        F11 = 0x7A,
        /*F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,

        NUMLOCK = 0x90,
        SCROLL = 0x91,

        // L* & R* - left and right Alt, Ctrl and Shift virtual keys.
        // Used only as parameters to GetAsyncKeyState() and GetKeyState().
        // No other API or message will distinguish left and right keys in this way.
        LSHIFT = 0xA0,
        RSHIFT = 0xA1,
        LCONTROL = 0xA2,
        RCONTROL = 0xA3,
        LMENU = 0xA4,
        RMENU = 0xA5,

        EXTEND_BSLASH = 0xE2,
        OEM_102 = 0xE2,

        PROCESSKEY = 0xE5,

        ATTN = 0xF6,
        CRSEL = 0xF7,
        EXSEL = 0xF8,
        EREOF = 0xF9,
        PLAY = 0xFA,
        ZOOM = 0xFB,
        NONAME = 0xFC,
        PA1 = 0xFD,
        OEM_CLEAR = 0xFE,


        SEMICOLON = 0xBA,
        EQUAL = 0xBB,
        COMMA = 0xBC,
        HYPHEN = 0xBD,
        PERIOD = 0xBE,
        SLASH = 0xBF,
        BACKQUOTE = 0xC0,

        BROWSER_BACK = 0xA6,
        BROWSER_FORWARD = 0xA7,
        BROWSER_REFRESH = 0xA8,
        BROWSER_STOP = 0xA9,
        BROWSER_SEARCH = 0xAA,
        BROWSER_FAVORITES = 0xAB,
        BROWSER_HOME = 0xAC,
        VOLUME_MUTE = 0xAD,
        VOLUME_DOWN = 0xAE,
        VOLUME_UP = 0xAF,
        MEDIA_NEXT_TRACK = 0xB0,
        MEDIA_PREV_TRACK = 0xB1,
        MEDIA_STOP = 0xB2,
        MEDIA_PLAY_PAUSE = 0xB3,
        LAUNCH_MAIL = 0xB4,
        LAUNCH_MEDIA_SELECT = 0xB5,
        LAUNCH_APP1 = 0xB6,
        LAUNCH_APP2 = 0xB7,

        LBRACKET = 0xDB,
        BACKSLASH = 0xDC,
        RBRACKET = 0xDD,
        APOSTROPHE = 0xDE,
        OFF = 0xDF,

        DBE_ALPHANUMERIC = 0x0f0,
        DBE_KATAKANA = 0x0f1,
        DBE_HIRAGANA = 0x0f2,
        DBE_SBCSCHAR = 0x0f3,
        DBE_DBCSCHAR = 0x0f4,
        DBE_ROMAN = 0x0f5,
        DBE_NOROMAN = 0x0f6,
        DBE_ENTERWORDREGISTERMODE = 0x0f7,
        DBE_ENTERIMECONFIGMODE = 0x0f8,
        DBE_FLUSHSTRING = 0x0f9,
        DBE_CODEINPUT = 0x0fa,
        DBE_NOCODEINPUT = 0x0fb,
        DBE_DETERMINESTRING = 0x0fc,
        DBE_ENTERDLGCONVERSIONMODE = 0x0fd*/
    }
    /// <summary>
    /// HotKey Class for Windows CE
    /// </summary>
    public class HotKeys
    {
        private const int WM_HOTKEY = 0x0312;
        private static int F11RegId = 0; // obtained from GlobalAtomTable
        private static int F01RegId = 0;
        private static int F02RegId = 0;
        private static int F03RegId = 0;
        private static int F04RegId = 0;
        private static int F05RegId = 0;
        private static int repeated = 0;
        /// <summary>
        /// Set or get repeated key event 
        /// </summary>
        public static bool RepeatedKey = false;
        /// <summary>
        /// HotKeyEventArgs
        /// </summary>
        /// <param name="KeyCode">Virtual Key Code</param>
        /// <param name="KeyDown">Key Down is press, otherwise, it is key up</param>
        public delegate void HotKeyEventArgs(Key KeyCode, bool KeyDown);
        /// <summary>
        /// Key Press Event
        /// </summary>
        public static event HotKeyEventArgs OnKeyEvent
        {
            add 
            {
                if (HKMsgWnd == null)
                {
                    HKMsgWnd = new MsgWindow();
                }
                HKMsgWnd.OnKeyEvent += value;
            }
            remove
            {
                if (HKMsgWnd != null)
                {
                    HKMsgWnd.OnKeyEvent -= value;
                    HKMsgWnd.Dispose();
                    HKMsgWnd = null;
                }
            }
        }

        private static MsgWindow HKMsgWnd = null;

        [DllImport("Coredll.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("Coredll.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        /*  In the WM_HOTKEY Message
            WM_HOTKEY idHotKey = (int) wParam; 
            fuModifiers = (UINT) LOWORD(lParam); 
            uVirtKey    = (UINT) HIWORD(lParam);
         */

        [DllImport("Coredll.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int GlobalAddAtom(String atomStr);
        [DllImport("Coredll.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern int GlobalDeleteAtom(int atom);

        // nested class inside  
        class MsgWindow : MessageWindow
        {
            //public HandHeldHotKeyNotify HotKeyNotify;
            public event HotKeyEventArgs OnKeyEvent;
            bool disposed = false;

            public MsgWindow()
            {
                // Register F11 Key
                HotKeys.F11RegId = GlobalAddAtom("CS101F11");
                if (HotKeys.F11RegId == 0)
                    throw new ApplicationException("GlobalAddAtom CS101F11 failed");
                HotKeys.RegisterHotKey(this.Hwnd, HotKeys.F11RegId,
                    (uint)efsModifiers.MOD_KEYUP, (uint)Key.F11);
                // Register F4 Key
                HotKeys.F04RegId = GlobalAddAtom("CS101F04");
                if (HotKeys.F04RegId == 0)
                    throw new ApplicationException("GlobalAddAtom CS101F04 failed");
                if (HotKeys.RegisterHotKey(this.Hwnd, HotKeys.F04RegId,
                    (uint)efsModifiers.MOD_KEYUP, (uint)Key.F4) == false)
                    MessageBox.Show("F4 hotkey registration failed");
                // Register F5 Key
                HotKeys.F05RegId = GlobalAddAtom("CS101F05");
                if (HotKeys.F05RegId == 0)
                    throw new ApplicationException("GlobalAddAtom CS101F05 failed");
                HotKeys.RegisterHotKey(this.Hwnd, HotKeys.F05RegId,
                    (uint)efsModifiers.MOD_KEYUP, (uint)Key.F5);
                // Register F1 Key
                HotKeys.F01RegId = GlobalAddAtom("CS101F01");
                if (HotKeys.F01RegId == 0)
                    throw new ApplicationException("GlobalAddAtom CS101F01 failed");
                if (!HotKeys.RegisterHotKey(this.Hwnd, HotKeys.F01RegId,
                    (uint)efsModifiers.MOD_KEYUP, (uint)Key.F1))
                    MessageBox.Show("F1 hotkey registration failed");
                // Register F2 Key
                HotKeys.F02RegId = GlobalAddAtom("CS101F02");
                if (HotKeys.F02RegId == 0)
                    throw new ApplicationException("GlobalAddAtom CS101F02 failed");
                if (!HotKeys.RegisterHotKey(this.Hwnd, HotKeys.F02RegId,
                    (uint)efsModifiers.MOD_KEYUP, (uint)Key.F2))
                    MessageBox.Show("F2 hotkey registration failed");
                // Register F3 Key
                HotKeys.F03RegId = GlobalAddAtom("CS101F03");
                if (HotKeys.F03RegId == 0)
                    throw new ApplicationException("GlobalAddAtom CS101F03 failed");
                if (!HotKeys.RegisterHotKey(this.Hwnd, HotKeys.F03RegId,
                    (uint)efsModifiers.MOD_KEYUP, (uint)Key.F3))
                    MessageBox.Show("F3 hotkey registration failed");
            }

            private void FreeUnmanaged()
            {
                GlobalDeleteAtom(HotKeys.F11RegId);
                HotKeys.UnregisterHotKey(this.Hwnd, HotKeys.F11RegId);
                GlobalDeleteAtom(HotKeys.F04RegId);
                HotKeys.UnregisterHotKey(this.Hwnd, HotKeys.F04RegId);
                GlobalDeleteAtom(HotKeys.F05RegId);
                HotKeys.UnregisterHotKey(this.Hwnd, HotKeys.F05RegId);
                GlobalDeleteAtom(HotKeys.F01RegId);
                HotKeys.UnregisterHotKey(this.Hwnd, HotKeys.F01RegId);
                GlobalDeleteAtom(HotKeys.F02RegId);
                HotKeys.UnregisterHotKey(this.Hwnd, HotKeys.F02RegId);
                GlobalDeleteAtom(HotKeys.F03RegId);
                HotKeys.UnregisterHotKey(this.Hwnd, HotKeys.F03RegId);
            }

            ~MsgWindow()
            {
                Dispose(false);
            }

            public void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    FreeUnmanaged();
                    disposed = true;
                }
                if (disposing)
                    base.Dispose();
            }

            protected override void WndProc(ref Message msg)
            {
                int IntLParam = (int)msg.LParam;

                switch (msg.Msg)
                {
                    case HotKeys.WM_HOTKEY:
                        bool KeyIsUp = ((IntLParam & 0x0000FFFF) & (int)efsModifiers.MOD_KEYUP) != 0;
                        if (!KeyIsUp)
                        {
                            System.Threading.Interlocked.Increment(ref HotKeys.repeated);
                        }
                        else
                        {
                            System.Threading.Interlocked.Exchange(ref HotKeys.repeated, 0);
                        }
                        if (HotKeys.RepeatedKey)
                        {
                            if (OnKeyEvent != null)
                                OnKeyEvent((Key)((IntLParam >> 16) & 0x0000FFFF), !KeyIsUp);
                        }
                        else
                        {
                            if (HotKeys.repeated <= 1 || KeyIsUp)
                            {
                                if (OnKeyEvent != null)
                                    OnKeyEvent((Key)((IntLParam >> 16) & 0x0000FFFF), !KeyIsUp);
                            }
                        }
                        break;
                    default:
                        break;
                }
                base.WndProc(ref msg);

            }
        }// class MsgWindow
    }// class ClsHotkey
}
#endif
