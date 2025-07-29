using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StreamDeckConfiguration.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace StreamDeckConfiguration;

public partial class HotKey : KeyActionUserControl, INotifyPropertyChanged
{
	private bool _waitingForShortcut;

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	public HotKey()
	{
		InitializeComponent();
		DataContext = this;
		ShortcutDisplay = "Click to set shortcut";
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();

		var button = this.FindControl<Button>("MyShortcutButton");
		button.AddHandler(KeyDownEvent, Button_KeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
	}

	private string _shortcutDisplay = "";
	public string ShortcutDisplay
	{
		get => _shortcutDisplay;
		set
		{
			if (_shortcutDisplay != value)
			{
				_shortcutDisplay = value;
				OnPropertyChanged();
			}
		}
	}

	public KeyGesture? Shortcut { get; private set; }

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
		_waitingForShortcut = true;
		ShortcutDisplay = "Waiting for shortcut...";
		Focus();
	}

	private void Button_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
	{
		e.Handled = true;
		if (!_waitingForShortcut)
			return;

		var modifiers = e.KeyModifiers;
		var key = e.Key;

		if (key == Key.None || IsModifierOnly(key))
		{
			ShortcutDisplay = FormatShortcut(modifiers, null);
			return;
		}

		Shortcut = new KeyGesture(key, modifiers);
		ShortcutDisplay = Shortcut.ToString();
		_waitingForShortcut = false;
	}
}
