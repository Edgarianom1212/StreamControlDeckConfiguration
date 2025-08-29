// App.axaml.cs
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace StreamDeckConfiguration;

public partial class App : Application
{
	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			// Async-Startup nutzen, statt synchron zu blockieren
			desktop.Startup += async (_, __) =>
			{
				desktop.MainWindow = new Views.MainWindow();
				desktop.MainWindow.Show();          // explizit anzeigen
			};

			// Optional: beim Beenden speichern
			desktop.Exit += async (_, __) =>
			{
				try { await GlobalData.SaveAsync(); } catch { /* ignore */ }
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}
