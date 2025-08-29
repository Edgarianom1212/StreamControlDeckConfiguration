using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StreamDeckConfiguration.Controls;

namespace StreamDeckConfiguration;

public partial class OpenWebsite : KeyActionUserControl
{
    public OpenWebsite()
    {
        InitializeComponent();
        DataContext = this;
    }

    private string url;
    public string URL
    {
        get => url;
        set
        {
            if (url != value)
            {
                url = value;
                OnPropertyChanged();
            }
        }
    }
}