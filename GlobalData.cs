using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using HarfBuzzSharp;
using StreamDeckConfiguration.Controls;
using StreamDeckConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;
using WindowsInput;

namespace StreamDeckConfiguration;

public static class GlobalData
{

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
	private const int KEYEVENTF_KEYDOWN = 0x0000;
	private const int KEYEVENTF_KEYUP = 0x0002;
	private const byte VK_CONTROL = 0x11;

	public static List<KeyAction> KeyActionList { get; set; }
	public static Window MainWindow { get; set; }

	static GlobalData()
	{
		KeyActionList = new List<KeyAction>()
		{
			new KeyAction("HTTP Request", "mdi-web", new HttpRequest()),
			new KeyAction("Open Website", "mdi-web", new OpenWebsite()),
			new KeyAction("Open Application", "mdi-application-brackets-outline", new OpenApplication()),
			new KeyAction("Close Application", "mdi-close-box-outline", new CloseApplication()),
			new KeyAction("Text", "mdi-text-recognition", new Text()),
			new KeyAction("HotKey", "mdi-view-grid-plus-outline", new HotKey()),
		};
	}

	public static async void ExecuteAction(object keyActionUserControl)
	{
		switch (keyActionUserControl)
		{
			case HttpRequest httpRequest:
				SendHttpRequest(httpRequest.RequestString, httpRequest.RequestBody);
				break;
			case OpenWebsite openWebsite:
				OpenWebsite(openWebsite.URL);
					break;
			case OpenApplication openApplication:
				OpenApplication(openApplication.FilePath);
				break;
			case CloseApplication closeApplication:
				CloseApplication(closeApplication.FilePath);
				break;
			case Text text:
				PasteText(text.PasteText, text.TopLevel);
				break;
			case HotKey hotKey:
				PerformHotKey(hotKey.Shortcut);
				break;
		}
	}

	private static async void SendHttpRequest(string url, string body)
	{
		var client = new HttpClient();
		var content = new StringContent(body, Encoding.UTF8, "application/json");

		var response = await client.PutAsync(url, content);
		string result = await response.Content.ReadAsStringAsync();
		Debug.WriteLine(result);
	}

	private static async void OpenWebsite(string url)
	{
		try
		{
			using var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				}
			};
			process.Start();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"error while opening website: {ex.Message}");
		}
	}

	private static async void OpenApplication(string filePath)
	{
		try
		{
			using var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = filePath,
					UseShellExecute = true
				}
			};
			process.Start();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"error while starting application: {ex.Message}");
		}
	}

	private static async void PasteText(string text, TopLevel topLevel)
	{
		try
		{
			NativeClipboard.SetText(text);

			await Task.Delay(100);

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
				keybd_event((byte)'V', 0, KEYEVENTF_KEYDOWN, 0);
				keybd_event((byte)'V', 0, KEYEVENTF_KEYUP, 0);
				keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
			}
			else
			{
				Debug.WriteLine("pasting text is only supported on windows");
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"error while pasting text: {ex.Message}");
		}
	}

	private static void CloseApplication(string exePath)
	{
		try
		{
			string fullPath = Path.GetFullPath(exePath).ToLowerInvariant();

			Process[] processes = Process.GetProcesses();

			foreach (var process in processes)
			{
				try
				{
					string processPath = process.MainModule?.FileName?.ToLowerInvariant();
					if (processPath == fullPath)
					{
						process.Kill(true); // true = close whole process family
						Debug.WriteLine($"Closed: {process.ProcessName}");
					}
				}
				catch (Exception ex)
				{
					//access to mainModule failed (e.g. system protected processes)
					Debug.WriteLine($"error on process: {process.ProcessName}: {ex.Message}");
				}
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"error while closing application: {ex.Message}");
		}
	}

	private static void PerformHotKey(KeyGesture keyGesture)
	{
		if (!TryMapKey(keyGesture.Key, out var mainKey))
			return;

		var modifiers = new List<VirtualKeyCode>();
		if (keyGesture.KeyModifiers.HasFlag(KeyModifiers.Control))
			modifiers.Add(VirtualKeyCode.CONTROL);
		if (keyGesture.KeyModifiers.HasFlag(KeyModifiers.Shift))
			modifiers.Add(VirtualKeyCode.SHIFT);
		if (keyGesture.KeyModifiers.HasFlag(KeyModifiers.Alt))
			modifiers.Add(VirtualKeyCode.MENU);
		if (keyGesture.KeyModifiers.HasFlag(KeyModifiers.Meta))
			modifiers.Add(VirtualKeyCode.LWIN); // oder RWIN bei Bedarf

		var sim = new InputSimulator();
		sim.Keyboard.ModifiedKeyStroke(modifiers, mainKey);
	}

	private static bool TryMapKey(Key key, out VirtualKeyCode vk)
	{
		vk = key switch
		{
			Key.A => VirtualKeyCode.VK_A,
			Key.B => VirtualKeyCode.VK_B,
			Key.C => VirtualKeyCode.VK_C,
			Key.D => VirtualKeyCode.VK_D,
			Key.E => VirtualKeyCode.VK_E,
			Key.F => VirtualKeyCode.VK_F,
			Key.G => VirtualKeyCode.VK_G,
			Key.H => VirtualKeyCode.VK_H,
			Key.I => VirtualKeyCode.VK_I,
			Key.J => VirtualKeyCode.VK_J,
			Key.K => VirtualKeyCode.VK_K,
			Key.L => VirtualKeyCode.VK_L,
			Key.M => VirtualKeyCode.VK_M,
			Key.N => VirtualKeyCode.VK_N,
			Key.O => VirtualKeyCode.VK_O,
			Key.P => VirtualKeyCode.VK_P,
			Key.Q => VirtualKeyCode.VK_Q,
			Key.R => VirtualKeyCode.VK_R,
			Key.S => VirtualKeyCode.VK_S,
			Key.T => VirtualKeyCode.VK_T,
			Key.U => VirtualKeyCode.VK_U,
			Key.V => VirtualKeyCode.VK_V,
			Key.W => VirtualKeyCode.VK_W,
			Key.X => VirtualKeyCode.VK_X,
			Key.Y => VirtualKeyCode.VK_Y,
			Key.Z => VirtualKeyCode.VK_Z,

			Key.D0 => VirtualKeyCode.VK_0,
			Key.D1 => VirtualKeyCode.VK_1,
			Key.D2 => VirtualKeyCode.VK_2,
			Key.D3 => VirtualKeyCode.VK_3,
			Key.D4 => VirtualKeyCode.VK_4,
			Key.D5 => VirtualKeyCode.VK_5,
			Key.D6 => VirtualKeyCode.VK_6,
			Key.D7 => VirtualKeyCode.VK_7,
			Key.D8 => VirtualKeyCode.VK_8,
			Key.D9 => VirtualKeyCode.VK_9,

			Key.F1 => VirtualKeyCode.F1,
			Key.F2 => VirtualKeyCode.F2,
			Key.F3 => VirtualKeyCode.F3,
			Key.F4 => VirtualKeyCode.F4,
			Key.F5 => VirtualKeyCode.F5,
			Key.F6 => VirtualKeyCode.F6,
			Key.F7 => VirtualKeyCode.F7,
			Key.F8 => VirtualKeyCode.F8,
			Key.F9 => VirtualKeyCode.F9,
			Key.F10 => VirtualKeyCode.F10,
			Key.F11 => VirtualKeyCode.F11,
			Key.F12 => VirtualKeyCode.F12,

			Key.Enter => VirtualKeyCode.RETURN,
			Key.Escape => VirtualKeyCode.ESCAPE,
			Key.Back => VirtualKeyCode.BACK,
			Key.Tab => VirtualKeyCode.TAB,
			Key.Space => VirtualKeyCode.SPACE,

			Key.Insert => VirtualKeyCode.INSERT,
			Key.Delete => VirtualKeyCode.DELETE,
			Key.Home => VirtualKeyCode.HOME,
			Key.End => VirtualKeyCode.END,
			Key.PageUp => VirtualKeyCode.PRIOR,
			Key.PageDown => VirtualKeyCode.NEXT,

			Key.Up => VirtualKeyCode.UP,
			Key.Down => VirtualKeyCode.DOWN,
			Key.Left => VirtualKeyCode.LEFT,
			Key.Right => VirtualKeyCode.RIGHT,

			_ => 0
		};

		return vk != 0;
	}
}
