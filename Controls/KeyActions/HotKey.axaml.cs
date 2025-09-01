using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using StreamDeckConfiguration.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace StreamDeckConfiguration;

public partial class HotKey : KeyActionUserControl, INotifyPropertyChanged
{
	// Whether we are currently waiting for the user to press a shortcut
	private bool waitingForShortcut;

	// Guard flag to avoid recursive updates when changing ShortcutDisplay
	private bool setSCFromDisplay = true;

	// Handle for the low-level keyboard hook
	private IntPtr hook = IntPtr.Zero;

	// Delegate reference to prevent GC from collecting the callback
	private HookProc? proc;

	// Set of currently pressed virtual key codes
	private readonly HashSet<uint> downVKs = new();

	public HotKey()
	{
		InitializeComponent();
		DataContext = this;

		// Default display text before a shortcut is set
		setSCFromDisplay = false;
		ShortcutDisplay = "Click to set shortcut";
		setSCFromDisplay = true;
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		var button = this.FindControl<Button>("MyShortcutButton");

		// Clicking the button starts capturing a new shortcut
		button.Click += Button_Click;

		// KeyDown handler is only used outside of capture mode
		button.AddHandler(KeyDownEvent, Button_KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		// Ensure the hook is removed when the control is detached
		StopHook();
		base.OnDetachedFromVisualTree(e);
	}

	// ==========================
	//      Bindable Property
	// ==========================

	private string shortcutDisplay = "";
	public string ShortcutDisplay
	{
		get => shortcutDisplay;
		set
		{
			if (shortcutDisplay != value)
			{
				shortcutDisplay = value;
				OnPropertyChanged();

				// Try to parse the shortcut string into a KeyGesture if we are allowed
				if (setSCFromDisplay)
				{
					try
					{
						if (!string.IsNullOrWhiteSpace(value))
							Shortcut = KeyGesture.Parse(value);
					}
					catch { /* ignore parse errors */ }
				}
			}
		}
	}

	// Stores the final captured shortcut
	public KeyGesture? Shortcut { get; private set; }

	public event PropertyChangedEventHandler? PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	// ==========================
	//      Helper Methods
	// ==========================

	// Determines if a key is only a modifier (Ctrl, Shift, Alt, Win)
	private static bool IsModifierOnly(Key key) =>
		key is Key.LeftCtrl or Key.RightCtrl or
			   Key.LeftShift or Key.RightShift or
			   Key.LeftAlt or Key.RightAlt or
			   Key.LWin or Key.RWin;

	// Format a KeyGesture as a string, e.g. "Ctrl + Shift + S"
	private static string FormatShortcut(KeyModifiers modifiers, Key? key)
	{
		var sb = new StringBuilder();
		if (modifiers.HasFlag(KeyModifiers.Control)) sb.Append("Ctrl + ");
		if (modifiers.HasFlag(KeyModifiers.Shift)) sb.Append("Shift + ");
		if (modifiers.HasFlag(KeyModifiers.Alt)) sb.Append("Alt + ");
		if (modifiers.HasFlag(KeyModifiers.Meta)) sb.Append("Win + ");
		if (key is not null and not Key.None) sb.Append(key.Value.ToString());
		return sb.ToString().TrimEnd(' ', '+');
	}

	// ==========================
	//      Capture Workflow
	// ==========================

	// Called when user clicks the button to set a new shortcut
	private void Button_Click(object? sender, RoutedEventArgs e)
	{
		waitingForShortcut = true;

		// Update display to inform user
		setSCFromDisplay = false;
		ShortcutDisplay = "Waiting for shortcut...";
		setSCFromDisplay = true;

		// Start low-level hook so we can capture system shortcuts
		StartHook();

		// Ensure this control has focus
		Focus();
	}

	// Called once a full shortcut is captured
	private void FinishCapture(KeyModifiers mods, Key key)
	{
		Shortcut = new KeyGesture(key, mods);

		setSCFromDisplay = false;
		ShortcutDisplay = Shortcut.ToString();
		setSCFromDisplay = true;

		waitingForShortcut = false;

		// Remove the keyboard hook again
		StopHook();
	}

	// Fallback key handling (not used during capture mode, since hook intercepts everything)
	private void Button_KeyDown(object? sender, KeyEventArgs e)
	{
		if (waitingForShortcut)
		{
			e.Handled = true;
			return;
		}
	}

	// ==========================
	//     Keyboard Hook Logic
	// ==========================

	private void StartHook()
	{
		if (hook != IntPtr.Zero) return;
		proc = HookCallback; // keep delegate alive
		var hMod = GetModuleHandle(null);
		hook = SetWindowsHookEx(WH_KEYBOARD_LL, proc, hMod, 0);
		if (hook == IntPtr.Zero)
			throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
		downVKs.Clear();
	}

	private void StopHook()
	{
		if (hook == IntPtr.Zero) return;
		UnhookWindowsHookEx(hook);
		hook = IntPtr.Zero;
		proc = null;
		downVKs.Clear();
	}

	// Callback for each keyboard event
	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode < 0)
			return CallNextHookEx(hook, nCode, wParam, lParam);

		var msg = (KeyboardMessage)wParam;
		var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
		uint vk = data.vkCode;

		bool isDown = msg is KeyboardMessage.WM_KEYDOWN or KeyboardMessage.WM_SYSKEYDOWN;
		bool isUp = msg is KeyboardMessage.WM_KEYUP or KeyboardMessage.WM_SYSKEYUP;

		// Maintain the set of currently pressed keys
		if (isDown) downVKs.Add(vk);
		if (isUp) downVKs.Remove(vk);

		if (waitingForShortcut)
		{
			// Build modifiers from pressed keys
			var mods = KeyModifiers.None;
			if (downVKs.Contains(0x10) || downVKs.Contains(0xA0) || downVKs.Contains(0xA1)) mods |= KeyModifiers.Shift;
			if (downVKs.Contains(0x11) || downVKs.Contains(0xA2) || downVKs.Contains(0xA3)) mods |= KeyModifiers.Control;
			if (downVKs.Contains(0x12) || downVKs.Contains(0xA4) || downVKs.Contains(0xA5)) mods |= KeyModifiers.Alt;
			if (downVKs.Contains(0x5B) || downVKs.Contains(0x5C)) mods |= KeyModifiers.Meta;

			// Determine the main non-modifier key
			Key? mainKey = TryMapVKToAvaloniaKey(vk, out var mapped) && !IsModifierOnly(mapped)
				? mapped
				: FirstNonModifierFromSet(downVKs);

			// Update the UI display text live
			Dispatcher.UIThread.Post(() =>
			{
				setSCFromDisplay = false;
				ShortcutDisplay = FormatShortcut(mods, mainKey is { } mk ? mk : null);
				setSCFromDisplay = true;
			});

			// If a non-modifier key was pressed, finalize the capture
			if (isDown && mainKey is { } realKey && !IsModifierOnly(realKey))
			{
				Dispatcher.UIThread.Post(() => FinishCapture(mods, realKey));
			}

			// Swallow all events so the OS does not react (e.g., prevent Snipping Tool on Win+Shift+S)
			return (IntPtr)1;
		}

		// If not in capture mode, just pass the event to the next hook/OS
		return CallNextHookEx(hook, nCode, wParam, lParam);
	}

	// Find the first non-modifier key currently pressed
	private static Key? FirstNonModifierFromSet(HashSet<uint> vks)
	{
		foreach (var v in vks)
		{
			if (TryMapVKToAvaloniaKey(v, out var k) && !IsModifierOnly(k))
				return k;
		}
		return null;
	}

	// Map a Windows Virtual-Key code to Avalonia's Key enum
	private static bool TryMapVKToAvaloniaKey(uint vk, out Key key)
	{
		// Letters A-Z
		if (vk >= 0x41 && vk <= 0x5A)
		{
			key = Key.A + (int)(vk - 0x41);
			return true;
		}
		// Numbers 0-9
		if (vk >= 0x30 && vk <= 0x39)
		{
			key = Key.D0 + (int)(vk - 0x30);
			return true;
		}
		// Function keys F1-F24
		if (vk >= 0x70 && vk <= 0x87)
		{
			key = Key.F1 + (int)(vk - 0x70);
			return true;
		}

		key = vk switch
		{
			0x1B => Key.Escape,
			0x09 => Key.Tab,
			0x0D => Key.Enter,
			0x20 => Key.Space,
			0x25 => Key.Left,
			0x26 => Key.Up,
			0x27 => Key.Right,
			0x28 => Key.Down,
			0x2E => Key.Delete,
			0x2D => Key.Insert,
			0x24 => Key.Home,
			0x23 => Key.End,
			0x21 => Key.PageUp,
			0x22 => Key.PageDown,
			0x6A => Key.Multiply,
			0x6B => Key.Add,
			0x6D => Key.Subtract,
			0x6E => Key.Decimal,
			0x6F => Key.Divide,

			// Modifiers
			0x10 or 0xA0 => Key.LeftShift,
			0xA1 => Key.RightShift,
			0x11 or 0xA2 => Key.LeftCtrl,
			0xA3 => Key.RightCtrl,
			0x12 or 0xA4 => Key.LeftAlt,
			0xA5 => Key.RightAlt,
			0x5B => Key.LWin,
			0x5C => Key.RWin,

			_ => Key.None
		};
		return key != Key.None;
	}

	// ==========================
	//     Win32 Interop
	// ==========================

	private const int WH_KEYBOARD_LL = 13;

	private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll")]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string? lpModuleName);

	private enum KeyboardMessage : int
	{
		WM_KEYDOWN = 0x0100,
		WM_KEYUP = 0x0101,
		WM_SYSKEYDOWN = 0x0104,
		WM_SYSKEYUP = 0x0105
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct KBDLLHOOKSTRUCT
	{
		public uint vkCode;
		public uint scanCode;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}
}
