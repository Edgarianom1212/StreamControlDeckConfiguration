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

    private string requestString;
    public string RequestString
    {
        get => requestString;
        set
        {
            requestString = value;
            OnPropertyChanged();
        }
    }

    private string requestBody;
    public string RequestBody
    {
        get => requestBody;
        set
        {
            requestBody = value;
            OnPropertyChanged();
        }
    }
}