using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StreamDeckConfiguration;

public partial class OpenWebsite : UserControl
{
    public OpenWebsite()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string URL { get; set; }
}