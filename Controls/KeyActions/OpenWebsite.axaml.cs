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

    public string URL { get; set; }
}