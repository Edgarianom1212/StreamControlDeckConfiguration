using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StreamDeckConfiguration;

public partial class Text : UserControl
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