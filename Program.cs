using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace StreamDeckConfiguration
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
			bool startHidden = args.Any(a => string.Equals(a, "--hidden", StringComparison.OrdinalIgnoreCase));

			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				Helpers.Logger.Log($"[APPDOMAIN] {e.ExceptionObject}");
			};

			TaskScheduler.UnobservedTaskException += (s, e) =>
			{
				Helpers.Logger.Log($"[TASK] {e.Exception}");
				e.SetObserved(); // verhindert Prozessabbruch
			};
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
        {
			IconProvider.Current.Register<MaterialDesignIconProvider>().Register<FontAwesomeIconProvider>();

			return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
        }
    }
}
