using Avalonia;
using Avalonia.Controls;
using StreamDeckConfiguration.Controls;
using StreamDeckConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration
{
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
				new KeyAction("Text", "mdi-text-recognition", new Text()),
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
				case Text text:
					PasteText(text.PasteText, text.TopLevel);
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
				Console.WriteLine($"error while opening website: {ex.Message}");
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
				Console.WriteLine($"error while starting application: {ex.Message}");
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
					Console.WriteLine("pasting text is only supported on windows");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"error while pasting text: {ex.Message}");
			}
		}
	}
}
