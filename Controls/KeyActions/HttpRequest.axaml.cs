using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StreamDeckConfiguration;

public partial class HttpRequest : UserControl
{
    public HttpRequest()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string RequestString { get; set; }
    public string RequestBody { get; set; }
}