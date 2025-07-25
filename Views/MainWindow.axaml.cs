using Avalonia.Controls;
using StreamDeckConfiguration.ViewModels;
using System.Threading;
using System.IO.Ports;
using System.Management;
using System;
using System.Diagnostics;

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

		private void SendWIFIDataToESP(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (vm.StreamDeckPort != null)
                if (vm.StreamDeckPort.IsOpen)
                    vm.StreamDeckPort.WriteLine(vm.WIFIName + ";" + vm.WIFIPassword);
		}

        private void SendWIFIResetToESP(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
			if (vm.StreamDeckPort != null)
				if (vm.StreamDeckPort.IsOpen)
					vm.StreamDeckPort.WriteLine("RESET;WIFI");
		}
	}
}