using Avalonia.Controls;
using StreamDeckConfiguration.ViewModels;
using System.Threading;
using System.IO.Ports;
using System.Management;
using System;
using System.Diagnostics;
using Avalonia.Controls.Primitives;

namespace StreamDeckConfiguration.Views
{
    public partial class MainWindow : Window
    {

		public MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();
            vm = new MainWindowViewModel();
            DataContext = vm;
        }

        private bool handleUnchecking = true;

		private void SDButtonChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
            for (int i = 0; i < vm.SDButtons.Count; i++)
            {
                if (sender is ToggleButton tb)
                {
                    if (tb.DataContext == vm.SDButtons[i])
                    {
                        vm.ActivateSDButtonConfig(i);
                    }
                }
                if (vm.SDButtons[i].IsActive)
                {
                    handleUnchecking = false;
                    vm.SDButtons[i].IsActive = false;
                    handleUnchecking = true;
                }
            }
		}

		private void SDButtonUnchecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
            if (handleUnchecking)
            {
                vm.KeyAction = new Models.KeyAction("", "", vm.InitLabel);
            }
		}
	}
}