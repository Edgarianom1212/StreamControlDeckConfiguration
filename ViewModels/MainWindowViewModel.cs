using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace StreamDeckConfiguration.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			SDButtonCount = 14;
			SDButtons = new ObservableCollection<SDButton>();
			StatusTuples = new ObservableCollection<Tuple<string, string>>();
			StreamDeckStatus = "Initial Status";
			WIFIName = "";
			WIFIPassword = "";

			StatusTuples.Add(new Tuple<string, string>("MYSTREAMDECK:CONNECTION_PING", "needs wifi info"));
			StatusTuples.Add(new Tuple<string, string>("MYSTREAMDECK:CONNECTING_TO_WIFI", "connecting to wifi"));
			StatusTuples.Add(new Tuple<string, string>("MYSTREAMDECK:WIFI_CONNECTION_SUCCESS", "connection success"));
			StatusTuples.Add(new Tuple<string, string>("MYSTREAMDECK:INVALID_WIFI_DATA", "invalid wifi date"));
			StatusTuples.Add(new Tuple<string, string>("MYSTREAMDECK:WIFI_TIMED_OUT", "wrong wifi data or timeout"));
			StatusTuples.Add(new Tuple<string, string>("MYSTREAMDECK:WIFI_FUNCTIONAL", "streamdeck connected"));

			string[] portNames = SerialPort.GetPortNames();

			//go through all ports, look if they are our streamdeck, if yes then add all incoming messages to the message history
			foreach (string portName in portNames)
			{
				try
				{
					StreamDeckPort = new SerialPort(portName, 115200)
					{
						ReadTimeout = 750,
						WriteTimeout = 500
					};
					StreamDeckPort.Open();
					Thread.Sleep(500); // time to boot

					string initCheck = StreamDeckPort.ReadLine();

					if (initCheck.Trim().StartsWith("MYSTREAMDECK:")) //then its our streamdeck
					{
						StreamDeckPort.DataReceived += (s, e) =>
						{
							try
							{
								string line = StreamDeckPort.ReadLine();
								ProcessIncomingMessage(line.Trim());
							}
							catch { }
						};

						DispatcherTimer connectionTimer = new()
						{
							Interval = TimeSpan.FromMilliseconds(1000)
						};

						connectionTimer.Tick += (_, _) =>
						{
							var elapsed = DateTime.Now - _lastMessageTime;

							if (elapsed.TotalMilliseconds > 1000)
								StreamDeckStatus = "no streamdeck found"; // TODO: retry after connecting or every 3 sec or so
						};

						connectionTimer.Start();

						break;
					}
					else
					{
						StreamDeckPort.Close();
						StreamDeckPort = null;
					}
				}
				catch { }
				StreamDeckStatus = "no streamdeck found";
			}

			for (int i = 0; i < SDButtonCount; i++)
			{
				SDButton sDButton = new SDButton(i + 1);
				SDButtons.Add(sDButton);
			}
		}

		public void ProcessIncomingMessage(string msg)
		{
			foreach (Tuple<string, string> tuple in StatusTuples)
			{
				if (tuple.Item1 == msg)
					StreamDeckStatus = tuple.Item2;
			}

			Debug.WriteLine(msg);

			Dispatcher.UIThread?.Post(() =>
			{
				_lastMessageTime = DateTime.Now;
			});
		}

		public ObservableCollection<SDButton> SDButtons { get; set; }
		public ObservableCollection<Tuple<string, string>> StatusTuples { get; set; }

        private string streamDeckStatus;
        public string StreamDeckStatus
		{
            get => streamDeckStatus;
            set => this.RaiseAndSetIfChanged(ref streamDeckStatus, value);
        }

        private string wIFIName;
        public string WIFIName
        {
            get => wIFIName;
            set => this.RaiseAndSetIfChanged(ref wIFIName, value);
        }

		private string wIFIPassword;
		public string WIFIPassword
		{
			get => wIFIPassword;
			set => this.RaiseAndSetIfChanged(ref wIFIPassword, value);
		}

		private int SDButtonCount;
		private DateTime _lastMessageTime = DateTime.MinValue;
		public SerialPort StreamDeckPort { get; set; }
    }
}
