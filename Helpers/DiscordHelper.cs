using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StreamDeckConfiguration.Helpers
{
	public static class DiscordHelper
	{
		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		public static bool IsDiscordVisible()
		{
			foreach (var proc in Process.GetProcessesByName("Discord"))
			{
				IntPtr handle = proc.MainWindowHandle;
				if (handle != IntPtr.Zero && IsWindowVisible(handle))
					return true;
			}
			return false;
		}
	}
}
