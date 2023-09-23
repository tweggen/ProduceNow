
using System.Reactive;
using ReactiveUI;
using ProduceNow.Models;

namespace ProduceNow.ViewModels;


class AddChennelViewModel : ViewModelBase
{
    string shortTitle;

    public AddChennelViewModel()
    {
        var okEnabled = this.WhenAnyValue(
            x => x.ShortTitle,
            x => !string.IsNullOrWhiteSpace(x));

        Ok = ReactiveCommand.Create(
            () => new ChannelPresentation() { ShortTitle = ShortTitle },
            okEnabled);
        Cancel = ReactiveCommand.Create(() => { });
    }

    public string ShortTitle
    {
        get => shortTitle;
        set => this.RaiseAndSetIfChanged(ref shortTitle, value);
    }

    public ReactiveCommand<Unit, ChannelPresentation> Ok { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }
}