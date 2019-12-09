using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;


namespace cf_pad.CLS
{
    public class BardCodeHooK
    {
        public delegate void BardCodeDeletegate(BarCodes barCode);
        public event BardCodeDeletegate BarCodeEvent;

        //定义成静态，这样不会抛出回收异常
        private static HookProc hookproc;




        public struct BarCodes
        {
            public int VirtKey;//虚拟吗
            public int ScanCode;//扫描码
            public string KeyName;//键名
            public uint Ascll;//Ascll
            public char Chr;//字符


            public string OriginalChrs; //原始 字符
            public string OriginalAsciis;//原始 ASCII


            public string OriginalBarCode;  //原始数据条码


            public bool IsValid;//条码是否有效
            public DateTime Time;//扫描时间,


            public string BarCode;//条码信息   保存最终的条码
        }


        private struct EventMsg
        {
            public int message;
            public int paramL;
            public int paramH;
            public int Time;
            public int hwnd;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);


        [DllImport("user32", EntryPoint = "GetKeyNameText")]
        private static extern int GetKeyNameText(int IParam, StringBuilder lpBuffer, int nSize);


        [DllImport("user32", EntryPoint = "GetKeyboardState")]
        private static extern int GetKeyboardState(byte[] pbKeyState);


        [DllImport("user32", EntryPoint = "ToAscii")]
        private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeySate, ref uint lpChar, int uFlags);


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);




        delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        BarCodes barCode = new BarCodes();
        int hKeyboardHook = 0;
        // string strBarCode = "";
        StringBuilder sbBarCode = new StringBuilder();
        public string strBarCode = "";

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            barCode.IsValid = false;
            bool notChar = false;
            if (nCode == 0)
            {
                EventMsg msg = (EventMsg)Marshal.PtrToStructure(lParam, typeof(EventMsg));

                if (wParam == 0x100)   //WM_KEYDOWN = 0x100
                {
                    barCode.VirtKey = msg.message & 0xff;  //虚拟码
                    barCode.ScanCode = msg.paramL & 0xff;  //扫描码

                    StringBuilder strKeyName = new StringBuilder(255);
                    if (GetKeyNameText(barCode.ScanCode * 65536, strKeyName, 255) > 0)
                    {
                        barCode.KeyName = strKeyName.ToString().Trim(new char[] { ' ', '\0' });
                    }
                    else
                    {
                        barCode.KeyName = "";
                    }

                    byte[] kbArray = new byte[256];
                    uint uKey = 0;
                    GetKeyboardState(kbArray);
                    if (ToAscii(barCode.VirtKey, barCode.ScanCode, kbArray, ref uKey, 0))
                    {
                        barCode.Ascll = uKey;
                        barCode.Chr = Convert.ToChar(uKey);
                    }
                    else
                    {
                        notChar = true;   //转到ascii字符失败，这不是一个正常字符，要去掉
                    }

                    
                            if (DateTime.Now.Subtract(barCode.Time).TotalMilliseconds > 30)     //30ms可以过滤掉连续按住一个键时的情况
                            {
                                if (notChar == false)
                                    strBarCode = barCode.Chr.ToString();
                                else
                                    strBarCode = "";
                                barCode.IsValid = false;
                            }
                            else
                            {
                                if (strBarCode.Length >= 1)  
                                {
                                    barCode.IsValid = true;      //isValid为true表明这是个条码
                                }
                                if (notChar == false)
                                {
                                    strBarCode += barCode.Chr.ToString();
                                }
                                barCode.BarCode = strBarCode;
                            }


                    


                    barCode.Time = DateTime.Now;
                    if (BarCodeEvent != null && barCode.IsValid) BarCodeEvent(barCode);    //触发事件
                    
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }


        //安装钩子
        public bool Start()
        {
            if (hKeyboardHook == 0)
            {
                hookproc = new HookProc(KeyboardHookProc);




                //GetModuleHandle 函数 替代 Marshal.GetHINSTANCE
                //防止在 framework4.0中 注册钩子不成功
                IntPtr modulePtr = GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName);


                //WH_KEYBOARD_LL=13
                //全局钩子 WH_KEYBOARD_LL
                //  hKeyboardHook = SetWindowsHookEx(13, hookproc, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);


                hKeyboardHook = SetWindowsHookEx(13, hookproc, modulePtr, 0);
            }
            return (hKeyboardHook != 0);
        }


        //卸载钩子
        public bool Stop()
        {
            if (hKeyboardHook != 0)
            {
                return UnhookWindowsHookEx(hKeyboardHook);
            }
            return true;
        }
    }
}
