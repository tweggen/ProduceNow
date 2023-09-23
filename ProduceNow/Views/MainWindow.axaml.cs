using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProduceNow.Views;

public partial class MainWindow : Window
{
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
}