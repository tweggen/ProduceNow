
using System.Reactive;
using ReactiveUI;
using ProduceNowApp.Models;

namespace ProduceNowApp.ViewModels;


class AddChannelViewModel : ViewModelBase
{
    string shortTitle;

    public AddChannelViewModel()
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