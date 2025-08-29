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
using ReactiveUI;
using Avalonia.Threading;
using StreamDeckConfiguration.Helpers;
using System.Runtime.CompilerServices;
using System.Threading;
using StreamDeckConfiguration.ViewModels;
using System.Collections.ObjectModel;

namespace StreamDeckConfiguration;

public class GlobalData : ReactiveObject
{
	public static GlobalData Instance { get; private set; }

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
	private const int KEYEVENTF_KEYDOWN = 0x0000;
	private const int KEYEVENTF_KEYUP = 0x0002;
	private const byte VK_CONTROL = 0x11;

	public ObservableCollection<SDButton> SDButtons { get; set; }

	public List<KeyAction> KeyActionList { get; set; }
	public Window MainWindow { get; set; }

	private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };

	public static AppConfig Config { get; private set; } = new();

	public static ConfigStore<AppConfig> Store { get; } = new();

	private static int _initialized = 0;

	public static async Task InitializeAsync()
	{
		if (System.Threading.Interlocked.Exchange(ref _initialized, 1) == 1)
			return;

		Config = await Store.LoadAsync(() => AppConfig.CreateDefault());

		for (int i = 0; i < Instance.SDButtons.Count; i++)
		{
			if (Config.Keys.Count >= i)
			{
				if (Config.Keys[i].Action != null)
				{
					Control control = ActionMapper.ToControl(Config.Keys[i].Action);

					foreach(KeyAction action in Instance.KeyActionList)
					{
						if (action.Config.GetType() == control.GetType())
						{
							KeyAction ka = new KeyAction(action);
							ka.Config = control;
							Instance.SDButtons[i].KeyAction = ka;
						}
					}
				}
			}
		}
	}

	public static async Task SaveAsync()
	{
		for (int i = 0; i < Instance.SDButtons.Count; i++)
		{
			List<KeyConfig> list = GlobalData.Config.Keys;

			if (list.Count >= i)
			{
				KeyConfig currentConfig = list[i];
				KeyAction keyAction = Instance.SDButtons[i].KeyAction;
				if (keyAction != null)
				{
					currentConfig.IconName = keyAction.IconName;
					currentConfig.Action = ActionMapper.ToData(keyAction.Config);
				}
			}
		}

		await Store.SaveAsync(Config);
	}

	private bool isDiscordOpen = false;
	public bool IsDiscordOpen
	{
		get => isDiscordOpen;
		set => this.RaiseAndSetIfChanged(ref isDiscordOpen, value);
	}

	private bool discordNeeded = false;
	public bool DiscordNeeded
	{
		get => discordNeeded;
		set
		{
			if (value)
			{
				timer.Tick -= CheckDiscordRunning;
				timer.Tick += CheckDiscordRunning;
				timer.Start();
			}
			else
			{
				timer.Tick -= CheckDiscordRunning;
				timer.Stop();
			}
		}
	}

	public KeyActionGroup GeneralGroup = new KeyActionGroup("General", "mdi-keyboard-outline");
	public KeyActionGroup DiscordGroup = new KeyActionGroup("Discord", "fa-discord");


	public GlobalData()
	{
		Instance = this;

		GlobalData.InitializeAsync(); // <-- asynchron laden

		KeyActionList = new List<KeyAction>()
		{
			new KeyAction("HTTP Request", "mdi-web", new HttpRequest(), GeneralGroup),
			new KeyAction("Open Website", "mdi-web", new OpenWebsite(), GeneralGroup),
			new KeyAction("Open Application", "mdi-application-brackets-outline", new OpenApplication(), GeneralGroup),
			new KeyAction("Close Application", "mdi-close-box-outline", new CloseApplication(), GeneralGroup),
			new KeyAction("Text", "mdi-text-recognition", new Text(), GeneralGroup),
			new KeyAction("HotKey", "mdi-view-grid-plus-outline", new HotKey(), GeneralGroup),
			//new KeyAction("Mute", "mdi-microphone-off", new DiscordMute(), DiscordGroup),
		};
	}

	public async void ExecuteAction(Control keyActionUserControl)
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
				PasteText(text.PasteText);
				break;
			case HotKey hotKey:
				PerformHotKey(hotKey.Shortcut);
				break;
			case DiscordMute discordMute:
				DiscordMute();
				break;
		}
	}

	private async void SendHttpRequest(string url, string body)
	{
		if (body == null)
			body = "";
		var client = new HttpClient();
		var content = new StringContent(body, Encoding.UTF8, "application/json");

		var response = await client.PutAsync(url, content);
		string result = await response.Content.ReadAsStringAsync();
		Debug.WriteLine(result);
	}

	private async void OpenWebsite(string url)
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

	private async void OpenApplication(string filePath)
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

	private async void PasteText(string text)
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

	private void CloseApplication(string exePath)
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

	private void PerformHotKey(KeyGesture keyGesture)
	{
		if (keyGesture == null) return;
		if (!TryMapKey(keyGesture.Key, out var mainKey)) return;

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

	private bool TryMapKey(Key key, out VirtualKeyCode vk)
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

	private void CheckDiscordRunning(object? sender, EventArgs? e)
	{
		Instance.IsDiscordOpen = DiscordHelper.IsDiscordVisible();
		Debug.WriteLine("tick");
	}

	private async void DiscordMute()
	{

	}
}
