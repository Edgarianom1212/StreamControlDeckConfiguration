using System;
using System.Runtime.InteropServices;
using System.Text;

namespace StreamDeckConfiguration;
public static class NativeClipboard
{
	private const uint CF_UNICODETEXT = 13;

	[DllImport("user32.dll")]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("user32.dll")]
	private static extern bool CloseClipboard();

	[DllImport("user32.dll")]
	private static extern bool EmptyClipboard();

	[DllImport("user32.dll")]
	private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GlobalUnlock(IntPtr hMem);

	private const uint GMEM_MOVEABLE = 0x0002;

	public static void SetText(string text)
	{
		if (!OpenClipboard(IntPtr.Zero)) return;

		try
		{
			EmptyClipboard();

			// Text als UTF-16-Bytes
			var bytes = Encoding.Unicode.GetBytes(text + '\0');
			var size = (UIntPtr)bytes.Length;

			IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, size);
			if (hGlobal == IntPtr.Zero) return;

			IntPtr target = GlobalLock(hGlobal);
			if (target == IntPtr.Zero) return;

			Marshal.Copy(bytes, 0, target, bytes.Length);
			GlobalUnlock(hGlobal);

			SetClipboardData(CF_UNICODETEXT, hGlobal);
		}
		finally
		{
			CloseClipboard();
		}
	}
}
