using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using StreamDeckConfiguration.Helpers;
using StreamDeckConfiguration.ViewModels;
using StreamDeckConfiguration.Views;
using System;
using System.Linq;

namespace StreamDeckConfiguration;

public partial class App : Application
{
	private IClassicDesktopStyleApplicationLifetime? life;
	private MainWindow? window;
	private TrayIcon? tray;

	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{

			life = desktop;

			desktop.Startup += async (_, __) =>
			{

				window = new MainWindow();

				if (!desktop.Args.Any(a => string.Equals(a, "--hidden", StringComparison.OrdinalIgnoreCase)))
				{
					desktop.MainWindow = window;
					desktop.MainWindow.Show();
				}
				CreateTrayIcon();
			};

			desktop.Exit += async (_, __) =>
			{
				PortManager.ClosePort();
				try { await GlobalData.SaveAsync(); } catch { /* ignore */ }
				DestroyTrayIcon();
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	public void ShowMainWindow()
	{
		if (life is null) return;

		if (window != null)
		{
			if (!window.IsVisible) window.Show();
		}
	}

	private void CreateTrayIcon()
	{
		var icon = new WindowIcon(AssetLoader.Open(
			new Uri("avares://StreamDeckConfiguration/Assets/StreamControlDeckIcon.ico")));

		var menu = new NativeMenu();

		var openItem = new NativeMenuItem("Open Configuration");
		openItem.Click += (_, __) => ShowMainWindow();
		menu.Items.Add(openItem);

		menu.Items.Add(new NativeMenuItemSeparator());

		var exitItem = new NativeMenuItem("Exit");
		exitItem.Click += (_, __) => life?.Shutdown();
		menu.Items.Add(exitItem);

		tray = new TrayIcon
		{
			Icon = icon,
			ToolTipText = "StreamDeckConfiguration is running",
			Menu = menu,
			IsVisible = true
		};

		// Left-click also opens the window
		tray.Clicked += (_, __) => ShowMainWindow();
	}

	private void DestroyTrayIcon()
	{
		if (tray is null) return;
		tray.IsVisible = false;
		tray.Clicked -= (_, __) => ShowMainWindow(); // no-op; just to show intent
		tray = null;
	}
}
