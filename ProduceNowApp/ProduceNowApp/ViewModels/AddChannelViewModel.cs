
using System;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using ProduceNowApp.Models;

namespace ProduceNowApp.ViewModels;


class AddChannelViewModel : ViewModelBase
{
    private ComboBoxItem feedItem;
    private string shortTitle;

    
    public AddChannelViewModel()
    {
        var okEnabled = this.WhenAnyValue(
            x => x.ShortTitle,
            x => !string.IsNullOrWhiteSpace(x));

        Ok = ReactiveCommand.Create(
            () => new ChannelPresentation()
            {
                Uuid = Guid.NewGuid().ToString(),
                ShortTitle = ShortTitle,
                Feed = FeedValue.Tag as string
            },
            okEnabled);
        Cancel = ReactiveCommand.Create(() => { });
    }
    

    public string ShortTitle
    {
        get => shortTitle;
        set => this.RaiseAndSetIfChanged(ref shortTitle, value);
    }


    public ComboBoxItem FeedValue
    {
        get => feedItem;
        set => this.RaiseAndSetIfChanged(ref feedItem, value);
    }
    
    
    public ReactiveCommand<Unit, ChannelPresentation> Ok { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }
}