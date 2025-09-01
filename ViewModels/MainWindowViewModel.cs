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
using System.Collections.Generic;
using Avalonia.Markup.Xaml;

namespace StreamDeckConfiguration.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private int SDButtonCount;
		public Label InitLabel = new Label() { Content = "Select a key to configure its action", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
		public Label NoActionLabel = new Label() { Content = "Drag an action from the right and drop it on an empty key above", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

		public MainWindowViewModel()
		{
			new GlobalData();

			SDButtonCount = 12;
			GlobalData.Instance.SDButtons = new ObservableCollection<SDButton>();
			ActiveKeyAction = new KeyAction("", "", InitLabel, new("none", ""));
			ActiveKeyActionIndex = -1;


			CheckPortsForStreamDeck();

			for (int i = 0; i < SDButtonCount; i++)
			{
				SDButton sDButton = new SDButton(i + 1);
				GlobalData.Instance.SDButtons.Add(sDButton);
			}

			LoadSDConfig();

			GlobalData.Instance.DiscordNeeded = CheckIfDiscordNeeded();
		}

		public bool CheckIfDiscordNeeded()
		{
			for (int i = 0; i < GlobalData.Instance.SDButtons.Count; i++)
			{
				if (GlobalData.Instance.SDButtons[i].KeyAction != null)
				{
					if (GlobalData.Instance.SDButtons[i].KeyAction.Group == GlobalData.Instance.DiscordGroup)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void LoadSDConfig()
		{
			//TODO: Load states of buttons
		}

		public void ActivateSDButtonConfig(int Index)
		{
			if (GlobalData.Instance.SDButtons.ElementAt(Index).KeyAction == null)
			{
				ActiveKeyAction = new KeyAction("", "", NoActionLabel, new("none", ""));
				ActiveKeyActionIndex = -1;
			}
			else
			{
				ActiveKeyAction = GlobalData.Instance.SDButtons.ElementAt(Index).KeyAction;
				ActiveKeyActionIndex = Index;
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
			string buttonMessage = "MYSTREAMDECK;BUTTON";
			int buttonMessageLength = buttonMessage.Length;
			Debug.WriteLine(msg);
			if (msg.StartsWith(buttonMessage))
			{
				string indexString = msg.Substring(buttonMessageLength);
				int index = int.Parse(indexString);

				for (int i = 0; i < GlobalData.Instance.SDButtons.Count; i++)
				{
					if (GlobalData.Instance.SDButtons[i].Index == index)
					{ 
						if (GlobalData.Instance.SDButtons[i].KeyAction != null)
						{
							if (GlobalData.Instance.SDButtons[i].KeyAction.Config is UserControl)
							{
								GlobalData.Instance.ExecuteAction(GlobalData.Instance.SDButtons[i].KeyAction.Config);
							}
						}
					}
				}
			}
		}

		public ObservableCollection<SDButton> SDButtons => GlobalData.Instance.SDButtons;
		public SerialPort StreamDeckPort { get; set; }

		public List<KeyAction> KeyActionList => GlobalData.Instance.KeyActionList;
		public IEnumerable<IGrouping<KeyActionGroup, KeyAction>> GroupedKeyActions => KeyActionList.GroupBy(a => a.Group);

		private KeyAction activeKeyAction;
		public KeyAction ActiveKeyAction
		{
			get => activeKeyAction;
			set => this.RaiseAndSetIfChanged(ref activeKeyAction, value);
		}

		private int activeKeyActionIndex;
		public int ActiveKeyActionIndex
		{
			get => activeKeyActionIndex;
			set => this.RaiseAndSetIfChanged(ref activeKeyActionIndex, value);
		}
	}
}
