using System;
using System.Runtime.InteropServices;

namespace SkinText {

    internal static class NativeMethods {

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }
}