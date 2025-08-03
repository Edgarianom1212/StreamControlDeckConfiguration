using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StreamDeckConfiguration.Controls;

namespace StreamDeckConfiguration;

public partial class Text : KeyActionUserControl
{
    public Text()
    {
        InitializeComponent();
        DataContext = this;
        TopLevel = TopLevel.GetTopLevel(this);
    }

    public string PasteText { get; set; }
    public TopLevel TopLevel { get; set; }

}