using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace UsageStats
{
    public static class Hook
    {
        //This class is based lightly off of the class found at the following website

        //http://blogs.msdn.com/toub/archive/2006/05/03/589423.aspx

        private static class API
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(
                int idHook,
                HookDel lpfn,
                IntPtr hMod,
                uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(
                IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(
                IntPtr hhk,
                int nCode,
                IntPtr
                wParam,
                IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(
                string lpModuleName);
        }

        public enum VK
        {
            //Keycodes recieved from this website:

            //http://delphi.about.com/od/objectpascalide/l/blvkc.htm

            //I've commented out the keys that I've never heard of--feel free to uncomment them if you wish

            LBUTTON = 0X01, //Left mouse
            RBUTTON = 0X02, //Right mouse
            //CANCEL       = 0X03,
            MBUTTON = 0X04,
            Backspace = 0X08, //Backspace
            Tab = 0X09,
            //CLEAR        = 0X0C,
            Enter = 0X0D, //Enter
            Shift = 0X10,
            Control = 0X11, //CTRL
            //MENU         = 0X12,
            Pause = 0X13,
            CapsLock = 0X14, //Caps-Lock
            Escape = 0X1B,
            Space = 0X20,
            PageUp = 0X21, //Page-Up
            PageDown = 0X22, //Page-Down
            End = 0X23,
            Home = 0X24,
            Left = 0X25,
            Up = 0X26,
            Right = 0X27,
            Down = 0X28,
            //SELECT       = 0X29,
            //PRINT        = 0X2A,
            //EXECUTE      = 0X2B,
            PrintScreen = 0X2C, //Print Screen
            Insert = 0X2D,
            Delete = 0X2E,
            //HELP         = 0X2F,

            N0 = 0X30,
            N1 = 0X31,
            N2 = 0X32,
            N3 = 0X33,
            N4 = 0X34,
            N5 = 0X35,
            N6 = 0X36,
            N7 = 0X37,
            N8 = 0X38,
            N9 = 0X39,

            A = 0X41,
            B = 0X42,
            C = 0X43,
            D = 0X44,
            E = 0X45,
            F = 0X46,
            G = 0X47,
            H = 0X48,
            I = 0X49,
            J = 0X4A,
            K = 0X4B,
            L = 0X4C,
            M = 0X4D,
            N = 0X4E,
            O = 0X4F,
            P = 0X50,
            Q = 0X51,
            R = 0X52,
            S = 0X53,
            T = 0X54,
            U = 0X55,
            V = 0X56,
            W = 0X57,
            X = 0X58,
            Y = 0X59,
            Z = 0X5A,

            NUMPAD0 = 0X60,
            NUMPAD1 = 0X61,
            NUMPAD2 = 0X62,
            NUMPAD3 = 0X63,
            NUMPAD4 = 0X64,
            NUMPAD5 = 0X65,
            NUMPAD6 = 0X66,
            NUMPAD7 = 0X67,
            NUMPAD8 = 0X68,
            NUMPAD9 = 0X69,

            Separator = 0X6C, // | (shift + backslash)
            Subtract = 0X6D, // -
            Decimal = 0X6E, // .
            Divide = 0X6F, // /
            Add = 107,
            Multiply = 106,

            F1 = 0X70,
            F2 = 0X71,
            F3 = 0X72,
            F4 = 0X73,
            F5 = 0X74,
            F6 = 0X75,
            F7 = 0X76,
            F8 = 0X77,
            F9 = 0X78,
            F10 = 0X79,
            F11 = 0X7A,
            F12 = 0X7B, //I only went up to F12, because honestly, who the hell has 24 F buttons?
            //and for the 8 people in the world who do, I think they can live without using them

            NumLock = 0X90,
            ScrollLock = 0X91, //Scroll-Lock
            LeftShift = 0XA0,
            RightShift = 0XA1,
            LeftControl = 0XA2,
            RightControl = 0XA3,
            LeftAlt = 164,
            AltGr = 165,
            LeftWindows = 91,
            RightWindows = 92,
            Popup = 93
            //LMENU        = 0XA4,
            //RMENU        = 0XA5,
            //PLAY         = 0XFA,
            //ZOOM         = 0XFB
        } //keycodes

        public delegate IntPtr HookDel(
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        public delegate void KeyHandler(
            IntPtr wParam,
            IntPtr lParam);

        private static IntPtr hhk = IntPtr.Zero;
        private static HookDel hd;
        private static KeyHandler kh;

        public static void CreateKeyboardHook(KeyHandler _kh)
        {
            Process _this = Process.GetCurrentProcess();
            ProcessModule mod = _this.MainModule;
            hd = KeyboardHookFunc;
            kh = _kh;

            hhk = API.SetWindowsHookEx(13, hd, API.GetModuleHandle(mod.ModuleName), 0);
            //13 is the parameter specifying that we're gonna do a low-level keyboard hook

//            MessageBox.Show(Marshal.GetLastWin32Error().ToString()); //for debugging
            //Note that this could be a Console.WriteLine(), as well. I just happened
            //to be debugging this in a Windows Application
            //to get the errors, in VS 2005+ (possibly before) do Tools -&gt; Error Lookup
        }

        public static bool DestroyHook()
        {
            //to be called when we're done with the hook

            return API.UnhookWindowsHookEx(hhk);
        }

        private static IntPtr KeyboardHookFunc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam)
        {
            int iwParam = wParam.ToInt32();
            if (nCode >= 0 &&
                (iwParam == 0x100 ||
                iwParam == 0x104)) //0x100 = WM_KEYDOWN, 0x104 = WM_SYSKEYDOWN
                kh(wParam, lParam);
 
            return API.CallNextHookEx(hhk, nCode, wParam, lParam);
        }
    }
}
