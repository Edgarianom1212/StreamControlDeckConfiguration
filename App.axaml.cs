using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using StreamDeckConfiguration.Helpers;
using StreamDeckConfiguration.ViewModels;
using StreamDeckConfiguration.Views;
using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration;

public partial class App : Application
{
	private IClassicDesktopStyleApplicationLifetime? life;
	private MainWindow? window;
	private TrayIcon? tray;
	private const string PipeName = "StreamDeckConfiguration.ShowPipe";

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

			_ = StartIpcServerAsync(desktop);
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
		tray.Clicked -= (_, __) => ShowMainWindow();
		tray = null;
	}

	private async Task StartIpcServerAsync(IClassicDesktopStyleApplicationLifetime desktop)
	{
		while (true)
		{
			try
			{
				using var server = new NamedPipeServerStream(
					PipeName,
					PipeDirection.In,
					maxNumberOfServerInstances: 1,
					PipeTransmissionMode.Message,
					PipeOptions.Asynchronous);

				await server.WaitForConnectionAsync().ConfigureAwait(false);

				//read message (expected: "SHOW")
				var buffer = new byte[64];
				int read = await server.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
				var text = Encoding.UTF8.GetString(buffer, 0, read).Trim();

				if (text.Equals("SHOW", StringComparison.OrdinalIgnoreCase))
				{
					await Dispatcher.UIThread.InvokeAsync(() =>
					{
						if (window != null)
						{
							window.Show();
						}
					});
				}
			}
			catch
			{
				await Task.Delay(200);
			}
		}
	}
}
