using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace StreamDeckConfiguration;

public partial class OpenApplication : UserControl, INotifyPropertyChanged
{
    public OpenApplication()
    {
        InitializeComponent();
		DataContext = this;
    }

	private string filePath;
	public string FilePath
	{
		get => filePath;
		set
		{
			if (filePath != value)
			{
				filePath = value;
				OnPropertyChanged(nameof(FilePath));
			}
		}
	}

	private string fileName;
	public string FileName
	{
		get => fileName;
		set
		{
			if (fileName != value)
			{
				fileName = value;
				OnPropertyChanged(nameof(FileName));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged(string name) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
	{
		var topLevel = TopLevel.GetTopLevel(this);

		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Select application to open",
			AllowMultiple = false,
			FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType("exeFiles") { Patterns = new List<string> { "*.exe" } } },
		});

		if (files.Count == 1)
		{
			FileName = files[0].Name;
			FilePath = files[0].Path.AbsolutePath;
		}
	}
}