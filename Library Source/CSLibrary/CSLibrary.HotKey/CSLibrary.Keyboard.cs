using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CSLibrary.Keyboard
{
    public class KeyEventArg : EventArgs
    {
        public System.Windows.Forms.Keys KeyCode;

        public KeyEventArg(System.Windows.Forms.Keys key)
        {
            this.KeyCode = key;
        }
    }
    public class Keyboard
    {

        public static event EventHandler<KeyEventArg> OnKeyUp;
        public static event EventHandler<KeyEventArg> OnKeyDown;


        private delegate Int32 KeyUpCallback
            (
            Int32 vkCode,
            Int32 scanCode,
            Int32 flags
            );

        private delegate Int32 KeyDownCallback
            (
            Int32 vkCode,
            Int32 scanCode,
            Int32 flags
            );

        private const string DLL_NAME = "Keyboard.dll";

        [DllImport(DLL_NAME, SetLastError = true)]
        private static extern bool Install(KeyUpCallback keyup, KeyDownCallback keydown);

        [DllImport(DLL_NAME, SetLastError = true)]
        public static extern bool UnInstall();

        public static bool Install()
        {
            return Install(KeyUp, KeyDown);
        }
        private static Int32 KeyUp(
            Int32 vkCode,
            Int32 scanCode,
            Int32 flags
            )
        {
            Debug.WriteLine(string.Format("Keyup code = {0}, scanCode = {1}", vkCode, scanCode));

            if (OnKeyUp != null)
            {
                OnKeyUp(null, new KeyEventArg((System.Windows.Forms.Keys)vkCode));
            }

            return 0;
        }
        private static Int32 KeyDown(
            Int32 vkCode,
            Int32 scanCode,
            Int32 flags
            )
        {
            Debug.WriteLine(string.Format("KeyDwon code = {0}, scanCode = {1}", vkCode, scanCode));

            if (OnKeyDown != null)
            {
                OnKeyDown(null, new KeyEventArg((System.Windows.Forms.Keys)vkCode));
            }

            return 0;
        }

    }
}
