using ReactiveUI;
using StreamDeckConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.ViewModels
{
	public class SDButton : ViewModelBase
	{

		public SDButton(int Index, KeyAction KeyAction = null)
		{
			this.Index = Index;
			this.KeyAction = KeyAction;
			IsActive = false;
		}

		private int index;
		public int Index
		{
			get => index;
			set => this.RaiseAndSetIfChanged(ref index, value);
		}

		private bool isActive;
		public bool IsActive
		{
			get => isActive;
			set => this.RaiseAndSetIfChanged(ref isActive, value);
		}

		private KeyAction keyAction;
		public KeyAction KeyAction
		{
			get => keyAction;
			set
			{
				this.RaiseAndSetIfChanged(ref keyAction, value);
				if (keyAction != null)
				{
					keyAction.PropertyChanged += KeyAction_PropertyChanged;
				}
			}
		}

		private void KeyAction_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.RaisePropertyChanged(nameof(KeyAction));
		}
	}
}
