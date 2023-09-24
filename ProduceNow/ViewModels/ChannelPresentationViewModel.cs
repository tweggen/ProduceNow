using System.Reactive;
using ReactiveUI;
using ProduceNow.Models;

namespace ProduceNow.ViewModels;

public class ChannelPresentationViewModel : ViewModelBase
{
    private ChannelPresentation _channelPresentation;

    public string ShortTitle
    {
        get => _channelPresentation.ShortTitle;
    }

    public string StateString
    {
        get => _channelPresentation.StateString;
    }
    
    public bool IsRecording
    {
        get => _channelPresentation.IsRecording;
    }


    public ChannelPresentationViewModel(ChannelPresentation channelPresentation)
    {
        _channelPresentation = channelPresentation;
    }
}