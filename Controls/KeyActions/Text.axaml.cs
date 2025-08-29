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
    }

    private string pasteText;
    public string PasteText
    {
        get => pasteText;
        set
        {
            if (pasteText != value)
            {
                pasteText = value;
                OnPropertyChanged();
            }
        }
    }
}