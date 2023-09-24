using System;
using System.Reactive;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

    public Bitmap? MiniPicture { get; }


    public string StandbyColor { get; } = "#aaaaaa";
    public string RecordingColor { get;  } = "#dd8822";
    public string StateColor => IsRecording ? RecordingColor : StandbyColor;

    private Bitmap NoRecord; 
    private Bitmap Record;

    public Bitmap RecordImage
    {
        get => IsRecording ? Record : NoRecord;
    }

    public ChannelPresentationViewModel(ChannelPresentation channelPresentation)
    {
        _channelPresentation = channelPresentation;
        MiniPicture = new Bitmap(AssetLoader.Open(new Uri(_channelPresentation.Uri)));
        NoRecord = new Bitmap(AssetLoader.Open(new Uri("avares://ProduceNow/Assets/NoRecord.png")));
        Record = new Bitmap(AssetLoader.Open(new Uri("avares://ProduceNow/Assets/Record.png")));
    }
}