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
using Avalonia.VisualTree;
using Avalonia.Controls.Presenters;
using System.Collections;
using System.Collections.Generic;
using StreamDeckConfiguration.Controls;
using StreamDeckConfiguration.Helpers;

namespace StreamDeckConfiguration.Views
{
	public partial class MainWindow : Window
	{

		public MainWindowViewModel vm;
		private Point dragStartPoint;
		private Popup? dragPopup;
		private Border preview;
		private Point oldPointerPosition;
		private bool handleUnchecking = true;

		public MainWindow()
		{
			InitializeComponent();
			vm = new MainWindowViewModel();
			DataContext = vm;

			Closing += (s, e) =>
			{
				e.Cancel = true;
				Hide();
			};
		}


		private void SDButtonChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			for (int i = 0; i < GlobalData.Instance.SDButtons.Count; i++)
			{
				if (sender is ToggleButton tb)
				{
					if (tb.DataContext == GlobalData.Instance.SDButtons[i])
					{
						vm.ActivateSDButtonConfig(i);
					}
				}
				if (GlobalData.Instance.SDButtons[i].IsActive)
				{
					handleUnchecking = false;
					GlobalData.Instance.SDButtons[i].IsActive = false;
					handleUnchecking = true;
				}
			}
		}

		private void SDButtonUnchecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (handleUnchecking)
			{
				vm.ActiveKeyAction = new Models.KeyAction("", "", vm.InitLabel, new("none", ""));
				vm.ActiveKeyActionIndex = -1;
			}
		}

		private void DragSource_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
				dragStartPoint = e.GetPosition(this);
		}

		private async void DragSource_PointerMoved(object? sender, PointerEventArgs e)
		{
			if (sender is Control dragSource && dragSource.DataContext is KeyAction action)
			{
				Point current = e.GetPosition(this);

				//slight treshhold for dragging
				if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && (Math.Abs(current.X - dragStartPoint.X) > 3 || Math.Abs(current.Y - dragStartPoint.Y) > 3))
				{
					preview = new Border
					{
						Width = 200,
						Height = 40,
						Background = Application.Current.Resources["ControlBackground"] as SolidColorBrush,
						IsHitTestVisible = false,
						Child = new StackPanel
						{
							Orientation = Avalonia.Layout.Orientation.Horizontal,
						},
					};

					((StackPanel)preview.Child).Children.Add(new Projektanker.Icons.Avalonia.Icon { Value = action.IconName, FontSize = 15, Margin = new Thickness(5), Foreground = Brushes.White, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
					((StackPanel)preview.Child).Children.Add(new Label { Content = action.ActionName, Margin = new Thickness(5), Foreground = Brushes.White, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

					dragPopup = new Popup
					{
						Child = preview,
						Placement = PlacementMode.Pointer,
						PlacementTarget = dragSource,
						IsLightDismissEnabled = false,
						IsHitTestVisible = false,
					};

					dragPopup.HorizontalOffset = 10;
					dragPopup.VerticalOffset = 10;

					oldPointerPosition = e.GetPosition(TopWindow);
					dragPopup.Open();

					//data for drop operation
					DataObject data = new DataObject();
					data.Set("KeyAction", action);

					//start the dragOver loop
					await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);

					dragPopup.IsOpen = false;
					dragPopup = null;
					preview = null;
				}
			}
		}

		private void Window_DragOver(object? sender, DragEventArgs e)
		{
			if (dragPopup == null)
				return;

			Point p = e.GetPosition(TopWindow);
			Visual? visual = TopWindow.InputHitTest(p) as Visual;
			
			IEnumerable<Visual> parents = visual.GetVisualAncestors();
			IEnumerator<Visual> enumerator = parents.GetEnumerator();

			for (int i = 0; i < 3; i++)
			{

				if (enumerator.Current is ToggleButton)
				{
					e.DragEffects = DragDropEffects.Copy;
					break;
				}
				else
				{
					e.DragEffects = DragDropEffects.None;
				}
				if (!enumerator.MoveNext())
				{
					break;
				}
			}

			double x = oldPointerPosition.X - p.X;
			double y = oldPointerPosition.Y - p.Y;

			dragPopup.HorizontalOffset = x + 10;
			dragPopup.VerticalOffset = y + 10;

			oldPointerPosition = p;
		}

		private async void OnToggleDrop(object? sender, DragEventArgs e)
		{
			if (sender is ToggleButton toggleButton && toggleButton.DataContext is SDButton sdButton)
			{
				if (e.Data.Contains("KeyAction"))
				{
					if (e.Data.Get("KeyAction") is KeyAction keyAction)
					{
						sdButton.KeyAction = new KeyAction(keyAction);

						if (toggleButton.IsChecked == true)
						{
							vm.ActivateSDButtonConfig(sdButton.Index-1);
						}
						else
						{
							toggleButton.IsChecked = true;
						}
						GlobalData.Instance.DiscordNeeded = vm.CheckIfDiscordNeeded();
						await GlobalData.SaveAsync();
					}
				}
			}
		}

		private void ToggleButtonExecuteAction(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem && menuItem.DataContext is SDButton sdButton)
			{
				if (sdButton.KeyAction != null && sdButton.KeyAction.Config is Control)
				{
					GlobalData.Instance.ExecuteAction(sdButton.KeyAction.Config);
				}
			}
		}
	}
}