using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
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

		private const string AppId = "StreamDeckConfiguration.SingleInstance";
		private const string PipeName = "StreamDeckConfiguration.ShowPipe";

		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		[STAThread]
        public static void Main(string[] args)
        {

			using var mutex = new System.Threading.Mutex(initiallyOwned: true, name: $"Global\\{AppId}", out bool isNew);


			if (isNew)
			{
				//first instance -> start new application
				BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
			}
			else
			{
				//second instance -> send "SHOW" to the first instance and immediately close second instance
				TrySignalExistingInstanceToShow();
			}

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

		private static void TrySignalExistingInstanceToShow()
		{
			try
			{
				using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
				// kurzer Timeout, falls die Server-Pipe gerade noch nicht lauscht
				client.Connect(300);
				var msg = Encoding.UTF8.GetBytes("SHOW");
				client.Write(msg, 0, msg.Length);
				client.Flush();
			}
			catch
			{
				// Nichts tun: wenn keine Instanz lauscht, einfach leise beenden
			}
		}
	}
}
