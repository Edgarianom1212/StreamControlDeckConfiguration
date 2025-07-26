using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Avalonia.Layout;
using StreamDeckConfiguration.Models;
using System.Linq;

namespace StreamDeckConfiguration.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private int SDButtonCount;
		public Label InitLabel = new Label() { Content = "Select a key to configure its action", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
		public Label NoActionLabel = new Label() { Content = "Drag an action from the right and drop it on an empty key above", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

		public MainWindowViewModel()
		{
			SDButtonCount = 12;
			SDButtons = new ObservableCollection<SDButton>();
			KeyAction = new KeyAction("", "", InitLabel);

			CheckPortsForStreamDeck();

			for (int i = 0; i < SDButtonCount; i++)
			{
				SDButton sDButton = new SDButton(i + 1);
				SDButtons.Add(sDButton);
			}
		}

		public void ActivateSDButtonConfig(int Index)
		{
			if (SDButtons.ElementAt(Index).KeyAction == null)
			{
				KeyAction = new KeyAction("", "", NoActionLabel);
			}
		}

		private void CheckPortsForStreamDeck()
		{
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

					StreamDeckPort.WriteLine("MYSTREAMDECK:HELLO");

					string reply = StreamDeckPort.ReadLine()?.Trim();

					if (reply == "MYSTREAMDECK:WAZZUP") //then its our streamdeck
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
					}
					else
					{
						StreamDeckPort.Close();
						StreamDeckPort = null;
					}
				}
				catch { }
			}
		}

		public void ProcessIncomingMessage(string msg)
		{
			Debug.WriteLine(msg);
		}

		public ObservableCollection<SDButton> SDButtons { get; set; }
		public SerialPort StreamDeckPort { get; set; }

		private KeyAction keyAction;
		public KeyAction KeyAction
		{
			get => keyAction;
			set => this.RaiseAndSetIfChanged(ref keyAction, value);
		}
	}
}
