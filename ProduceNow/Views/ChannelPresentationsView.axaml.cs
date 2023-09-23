using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProduceNow.Views;

public partial class ChannelPresentationsView : UserControl
{
    public ChannelPresentationsView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}