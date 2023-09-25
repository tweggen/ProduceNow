using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProduceNowApp.ViewModels;

namespace ProduceNowApp.Views;

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