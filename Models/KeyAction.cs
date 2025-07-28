using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckConfiguration.Models
{
	public class KeyAction : ReactiveObject
	{

		public KeyAction(string ActionName, string IconName, Control Config)
		{
			this.ActionName = ActionName;
			this.IconName = IconName;
			this.Config = Config;
		}

		public KeyAction(KeyAction keyAction)
		{
			ActionName = keyAction.ActionName;
			IconName = keyAction.IconName;
			Config = Activator.CreateInstance(keyAction.Config.GetType()) as Control;
		}

		public void ExecuteAction()
		{

		}

		private string actionName;
		public string ActionName
		{
			get => actionName;
			set => this.RaiseAndSetIfChanged(ref actionName, value);
		}

		private string iconName;
		public string IconName
		{
			get => iconName;
			set => this.RaiseAndSetIfChanged(ref iconName, value);
		}

		private Control config;
		public Control Config
		{
			get => config;
			set => this.RaiseAndSetIfChanged(ref config, value);
		}
	}
}
