using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProduceNow.ViewModels;

namespace ProduceNow.Views;

public partial class ChannelPresentationsView : UserControl
{
    public ChannelPresentationsView()
    {
        InitializeComponent();
        this.DataContext = new ChannelPresentationsViewModel();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}