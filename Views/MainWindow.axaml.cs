using Avalonia.Controls;
using StreamDeckConfiguration.ViewModels;
using System.Threading;
using System.IO.Ports;
using System.Management;
using System;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia;
using StreamDeckConfiguration.Models;
using Avalonia.Media;
using Avalonia.Interactivity;

namespace StreamDeckConfiguration.Views
{
	public partial class MainWindow : Window
	{

		public MainWindowViewModel vm;
		private Point DragStart;
		private Popup? DragPopup;
		private StackPanel preview;
		private Point OldPointerPosition;

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
				vm.ActiveKeyAction = new Models.KeyAction("", "", vm.InitLabel);
			}
		}

		private void DragSource_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
				DragStart = e.GetPosition(this);
		}

		private async void DragSource_PointerMoved(object? sender, PointerEventArgs e)
		{
			if (sender is Border dragSource &&
				dragSource.DataContext is KeyAction action)
			{
				var current = e.GetPosition(this);

				// 1) Threshold prüfen
				if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
					(Math.Abs(current.X - DragStart.X) > 3 ||
					 Math.Abs(current.Y - DragStart.Y) > 3))
				{
					// 2) Vorschau-Adorner anlegen
					preview = new StackPanel
					{
						Width = 200,
						Height = 40,
						Orientation = Avalonia.Layout.Orientation.Horizontal,
						Background = Brushes.Black,
						Opacity = 0.5,
						IsHitTestVisible = false,             // lässt DragOver durch
					};
					preview.Children.Add(new Projektanker.Icons.Avalonia.Icon { Value = action.IconName, FontSize = 15, Margin = new Thickness(5) });
					preview.Children.Add(new Label { Content = action.ActionName, Margin = new Thickness(5) });

					DragPopup = new Popup
					{
						Child = preview,
						Placement = PlacementMode.Pointer,
						PlacementTarget = dragSource,
						IsLightDismissEnabled = false,
						// Start-Position grob in der Mitte des Borders
						IsHitTestVisible = false,
						RenderTransform = new TranslateTransform(),
					};

					DragPopup.HorizontalOffset = 10;
					DragPopup.VerticalOffset = 10;

					OldPointerPosition = e.GetPosition(TopWindow);
					DragPopup.Open();

					// 3) Daten für Drop
					var data = new DataObject();
					data.Set("myFormat", action);

					// 4) Starte die eingebaute Drag-Schleife
					await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);

					DragPopup.IsOpen = false;
					DragPopup = null;
					preview = null;
				}
			}
		}

		private DateTime lastMove = DateTime.MinValue;	
		private void Window_DragOver(object? sender, DragEventArgs e)
		{
			if (DragPopup == null)
				return;

			if ((DateTime.Now - lastMove).TotalMilliseconds < 8)
				return;
			lastMove = DateTime.Now;

			Point p = e.GetPosition(TopWindow);

			double x = OldPointerPosition.X - p.X;
			double y = OldPointerPosition.Y - p.Y;

			DragPopup.HorizontalOffset = x + 10;
			DragPopup.VerticalOffset = y + 10;

			OldPointerPosition = p;
		}


		private void OnToggleDrop(object? sender, DragEventArgs e)
		{

		}
	}
}