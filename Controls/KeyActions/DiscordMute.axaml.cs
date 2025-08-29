using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HarfBuzzSharp;
using StreamDeckConfiguration.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StreamDeckConfiguration;

public partial class DiscordMute : KeyActionUserControl
{
	public DiscordMute()
    {
        InitializeComponent();
        DataContext = this;
		DiscordStatus = GlobalData.Instance.IsDiscordOpen ? "Discord is open" : "Please open Discord";
		GlobalData.Instance.PropertyChanged += GlobalData_PropertyChanged;
	}

	private void GlobalData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(GlobalData.IsDiscordOpen))
		{
			DiscordStatus = GlobalData.Instance.IsDiscordOpen ? "Discord is open" : "Please open Discord";
		}
	}

	private string discordStatus = "Please open Discord";
	public string DiscordStatus
	{
		get => discordStatus;
		set
		{
			if (discordStatus != value)
			{
				discordStatus = value;
				OnPropertyChanged();
			}
		}
	}
}