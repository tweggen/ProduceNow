using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProduceNow.Views;

public partial class AddChannelView : UserControl
{
    public AddChannelView()
    {
        this.InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}