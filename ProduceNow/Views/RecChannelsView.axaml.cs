using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProduceNow.Views;


public partial class RecChannelsView : UserControl
{
    public RecChannelsView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}