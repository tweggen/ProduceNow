using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProduceNow.Views;

public partial class ChannelPresentationView : UserControl
{
    public ChannelPresentationView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}