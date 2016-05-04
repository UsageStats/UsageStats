using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UsageStats
{
    // http://blogs.msdn.com/toub/archive/2006/05/03/589468.aspx
    // http://msdn.microsoft.com/en-us/library/ms644959(VS.85).aspx
    // http://msdn.microsoft.com/en-us/library/ms644986(v=VS.85).aspx

    public enum MouseMessages
    {
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_MOUSEWHEEL = 0x020A
    }

    public class InterceptMouse : IDisposable
    {
        #region Delegates

        public delegate void MouseHandler(
            IntPtr wParam,
            IntPtr lParam);

        #endregion

        private const int WH_MOUSE_LL = 14;

        private static readonly LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static MouseHandler _handler;

        public InterceptMouse(MouseHandler handler)
        {
            _handler = handler;
            _hookID = SetHook(_proc);
        }

        #region IDisposable Members

        public void Dispose()
        {
            bool result = UnhookWindowsHookEx(_hookID);
            if (!result)
                Trace.WriteLine("Could not unhook mouse interception.");
        }

        #endregion

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, IntPtr.Zero, 0);
            }
        }

        // If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx.

        // If nCode is greater than or equal to zero, and the hook procedure did not process the message, 
        // it is highly recommended that you call CallNextHookEx and return the value it returns; otherwise, 
        // other applications that have installed WH_MOUSE_LL hooks will not receive hook notifications and 
        // may behave incorrectly as a result. If the hook procedure processed the message, it may return 
        // a nonzero value to prevent the system from passing the message to the rest of the hook chain or 
        // the target window procedure.


        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (_handler != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _handler(wParam, lParam);
                    }));
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static MSLLHOOKSTRUCT GetMouseData(IntPtr lParam)
        {
            return (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (MSLLHOOKSTRUCT));
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
                                                      LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                                    IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #region Nested type: LowLevelMouseProc

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Nested type: MSLLHOOKSTRUCT

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region Nested type: POINT

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        #endregion
    }
}