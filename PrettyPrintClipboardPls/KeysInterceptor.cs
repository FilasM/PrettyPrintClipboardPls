using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrettyPrintClipboardPls
{
	public class KeysInterceptor : IDisposable
	{
		#region Constants

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;

		#endregion

		#region Fields

		private static LowLevelKeyboardProc m_proc = HookCallback;
		private static IntPtr m_hookId;

		public delegate void KeyboardEventCallback(Keys key);
		public static event KeyboardEventCallback KeyboardKeyDown;

		public static event KeyboardEventCallback KeyboardKeyUp;
		#endregion

		public KeysInterceptor()
		{
			m_hookId = SetHook(m_proc);
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				int vkCode = Marshal.ReadInt32(lParam);

				if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    KeyboardKeyDown?.Invoke((Keys)vkCode);
                }
				else if (wParam == (IntPtr)WM_KEYUP)
                {
                    KeyboardKeyUp?.Invoke((Keys)vkCode);
                }
			}

			return CallNextHookEx(m_hookId, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

        public void Dispose()
        {
            UnhookWindowsHookEx(m_hookId);
        }
    }
}
