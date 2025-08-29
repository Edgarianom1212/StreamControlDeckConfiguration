using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StreamDeckConfiguration.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace StreamDeckConfiguration;

public partial class HotKey : KeyActionUserControl
{
	private bool waitingForShortcut;
	private bool setSCFromDisplay = true;

	public HotKey()
	{
		InitializeComponent();
		DataContext = this;
		setSCFromDisplay = false;
		ShortcutDisplay = "Click to set shortcut";
		setSCFromDisplay = true;
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();

		var button = this.FindControl<Button>("MyShortcutButton");
		button.AddHandler(KeyDownEvent, Button_KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
	}

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

				if (setSCFromDisplay)
				{
					try
					{
						if (value != null)
						{
							Shortcut = KeyGesture.Parse(value);
						}
					}
					catch { };
				}
			}
		}
	}

	public KeyGesture? Shortcut { get; set; }

	private bool IsModifierOnly(Key key) =>
		key is Key.LeftCtrl or Key.RightCtrl or
			   Key.LeftShift or Key.RightShift or
			   Key.LeftAlt or Key.RightAlt or
			   Key.LWin or Key.RWin;

	private string FormatShortcut(KeyModifiers modifiers, Key? key)
	{
		var sb = new StringBuilder();
		if (modifiers.HasFlag(KeyModifiers.Control)) sb.Append("Ctrl + ");
		if (modifiers.HasFlag(KeyModifiers.Shift)) sb.Append("Shift + ");
		if (modifiers.HasFlag(KeyModifiers.Alt)) sb.Append("Alt + ");
		if (modifiers.HasFlag(KeyModifiers.Meta)) sb.Append("Win + ");
		if (key is not null and not Key.None)
			sb.Append(key.Value.ToString());
		return sb.ToString().TrimEnd(' ', '+');
	}

	private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		waitingForShortcut = true;
		setSCFromDisplay = false;
		ShortcutDisplay = "Waiting for shortcut...";
		setSCFromDisplay = true;
		Focus();
	}

	private void Button_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
	{
		e.Handled = true;
		if (!waitingForShortcut)
			return;

		var modifiers = e.KeyModifiers;
		var key = e.Key;

		if (key == Key.None || IsModifierOnly(key))
		{
			setSCFromDisplay = false;
			ShortcutDisplay = FormatShortcut(modifiers, null);
			setSCFromDisplay = true;
			return;
		}

		Shortcut = new KeyGesture(key, modifiers);
		setSCFromDisplay = false;
		ShortcutDisplay = Shortcut.ToString();
		setSCFromDisplay = true;
		waitingForShortcut = false;
	}
}
