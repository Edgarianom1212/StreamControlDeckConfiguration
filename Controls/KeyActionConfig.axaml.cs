using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StreamDeckConfiguration.Models;
using StreamDeckConfiguration.ViewModels;

namespace StreamDeckConfiguration.Controls;

public partial class KeyActionConfig : UserControl
{
    public KeyActionConfig()
    {
        InitializeComponent();
    }

	private void RemoveKeyAction(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
        if (DataContext is MainWindowViewModel vm)
        {
            if (vm.ActiveKeyActionIndex != -1)
            {
                GlobalData.Instance.SDButtons[vm.ActiveKeyActionIndex].KeyAction = null;
				vm.ActiveKeyAction = new KeyAction("", "", vm.NoActionLabel, new("none", ""));
			}
        }
	}
}