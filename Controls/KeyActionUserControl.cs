using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Controls
{
	public class KeyActionUserControl : UserControl, INotifyPropertyChanged
	{
		public KeyActionUserControl()
		{
			Width = 430;
			Height = 200;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
