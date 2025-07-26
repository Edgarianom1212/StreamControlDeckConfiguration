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

		public SDButton(int ID, KeyAction KeyAction = null)
		{
			this.ID = ID;
			this.KeyAction = KeyAction;
			IsActive = false;
		}

		private int iD;
		public int ID
		{
			get => iD;
			set => this.RaiseAndSetIfChanged(ref iD, value);
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
			set => this.RaiseAndSetIfChanged(ref keyAction, value);
		}
	}
}
