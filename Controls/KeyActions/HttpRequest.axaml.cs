using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StreamDeckConfiguration.Controls;

namespace StreamDeckConfiguration;

public partial class HttpRequest : KeyActionUserControl
{
    public HttpRequest()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string RequestString { get; set; }
    public string RequestBody { get; set; }
}