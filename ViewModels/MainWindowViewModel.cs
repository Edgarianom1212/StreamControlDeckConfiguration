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
using StreamDeckConfiguration.Helpers;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private int SDButtonCount;
		public Label InitLabel = new Label() { Content = "Select a key to configure its action", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
		public Label NoActionLabel = new Label() { Content = "Drag an action from the right and drop it on an empty key above", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
		public bool IsStreamControlDeckConnected = false;

		public MainWindowViewModel()
		{
			if (GlobalData.Instance == null)
			{
				new	GlobalData();
			}

			if (!AutoStartManager.IsEnabled("--hidden"))
			{
				AutoStartManager.Enable("--hidden");
			}

			SDButtonCount = 12;
			GlobalData.Instance.SDButtons = new ObservableCollection<SDButton>();
			ActiveKeyAction = new KeyAction("", "", InitLabel, new("none", ""));
			ActiveKeyActionIndex = -1;

			_ = Task.Run(async () =>
			{
				IsStreamControlDeckConnected = CheckPortsForStreamDeck();
				if (IsStreamControlDeckConnected)
				{
					Logger.Log("Successfully connected");
				}
				else
				{
					Logger.Log("Not fun");
				}

				using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));

				while (await timer.WaitForNextTickAsync())
				{
					if (PortManager.StreamDeckPort != null && PortManager.StreamDeckPort.IsOpen)
					{
						try
						{
							PortManager.StreamDeckPort.WriteLine("MYSTREAMDECK:HELLO");
						}
						catch (Exception e) { Logger.Log(e.ToString()); }
					}
					else
					{
						IsStreamControlDeckConnected = CheckPortsForStreamDeck();
						Logger.Log(IsStreamControlDeckConnected ? "Successfully connected" : "Not fun");
					}
				}
			});

			for (int i = 0; i < SDButtonCount; i++)
			{
				SDButton sDButton = new SDButton(i + 1);
				GlobalData.Instance.SDButtons.Add(sDButton);
			}

			GlobalData.InitializeAsync();

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

		private bool CheckPortsForStreamDeck()
		{
			string[] portNames = SerialPort.GetPortNames();

			foreach (string portName in portNames)
			{
				SerialPort? candidate = null;
				try
				{
					candidate = new SerialPort(portName, 115200)
					{
						ReadTimeout = 1500,
						WriteTimeout = 1000,
						NewLine = "\n",
						DtrEnable = false,
						RtsEnable = false
					};

					//only open candidate port if the current streamDeckPort is null or not open
					if (PortManager.StreamDeckPort == null || (PortManager.StreamDeckPort == candidate && !(PortManager.StreamDeckPort?.IsOpen ?? false)))
					{
						candidate.Open();
					}

					Thread.Sleep(1200);

					candidate.DiscardInBuffer();
					candidate.DiscardOutBuffer();

					candidate.WriteLine("MYSTREAMDECK:HELLO");
					string? reply = candidate.ReadLine()?.Trim();

					if (reply == "MYSTREAMDECK:WAZZUP")
					{
						PortManager.StreamDeckPort = candidate;
						PortManager.StreamDeckPort.DataReceived += (s, e) =>
						{
							ProcessIncomingMessage(PortManager.StreamDeckPort.ReadLine().Trim());
						};
						return true;
					}
				}
				catch (Exception ex)
				{
					Logger.Log($"Probe {portName} failed: {ex.Message}");
				}
				finally
				{
					// Wenn nicht übernommen, sauber schließen
					if (candidate != null && !ReferenceEquals(candidate, PortManager.StreamDeckPort))
					{
						PortManager.StreamDeckPort = null;
						try { candidate.Close(); } catch { }
						try { candidate.Dispose(); } catch { }
					}
				}
			}
			return false;
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
