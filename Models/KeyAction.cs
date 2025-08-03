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

		public KeyAction(string ActionName, string IconName, Control Config, KeyActionGroup Group)
		{
			this.ActionName = ActionName;
			this.IconName = IconName;
			this.Config = Config;
			this.Group = Group;
		}

		public KeyAction(KeyAction keyAction)
		{
			ActionName = keyAction.ActionName;
			IconName = keyAction.IconName;
			Config = Activator.CreateInstance(keyAction.Config.GetType()) as Control;
			Group = keyAction.Group;
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

		private KeyActionGroup group;
		public KeyActionGroup Group
		{
			get => group;
			set => this.RaiseAndSetIfChanged(ref group, value);
		}
	}

	public class KeyActionGroup : ReactiveObject
	{
		public KeyActionGroup(string GroupName, string IconName)
		{
			this.GroupName = GroupName;
			this.IconName = IconName;
		}

		private string groupName;
		public string GroupName
		{
			get => groupName;
			set => this.RaiseAndSetIfChanged(ref groupName, value);
		}

		private string iconName;
		public string IconName
		{
			get => iconName;
			set => this.RaiseAndSetIfChanged(ref iconName, value);
		}

		public override bool Equals(object? obj)
		{
			return obj is KeyActionGroup other &&
				   GroupName == other.GroupName &&
				   IconName == other.IconName;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(GroupName, IconName);
		}

	}
}
