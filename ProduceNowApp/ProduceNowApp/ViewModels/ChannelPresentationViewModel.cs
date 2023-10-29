using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Org.BouncyCastle.Tls;
using ReactiveUI;
using ProduceNowApp.Models;
using ProduceNowApp.Services;
using Splat;

namespace ProduceNowApp.ViewModels;

public class ChannelPresentationViewModel : ViewModelBase, IDisposable
{
    private Database _database = Locator.Current.GetService<Database>();
    
    private ChannelPresentation _channelPresentation;

    private Services.RTCWebSocketServer _rtcWebSocketServer;

    public string ShortTitle
    {
        get => _channelPresentation.ShortTitle;
    }

    public string StateString
    {
        get => _channelPresentation.StateString;
    }

    public string ChannelUuid
    {
        get => _channelPresentation.Uuid;
    }
    
    public bool IsRecording
    {
        get => _channelPresentation.IsRecording;
        private set
        {
            var isRecording = _channelPresentation.IsRecording;
            if (value != isRecording)
            {
                _channelPresentation.IsRecording = value;
                // TXWTODO: We are using raise if changed but changing it.
                this.RaisePropertyChanged("IsRecording");
                this.RaisePropertyChanged("RecordImage");
                this.RaisePropertyChanged("StateColor");
                if (value)
                {
                    _channelPresentation.StateString = "recording";
                }
                else
                {
                    _channelPresentation.StateString = "monitoring";
                }
                this.RaisePropertyChanged("StateString");
            }
        }
    }

    private Bitmap _miniPicture = null;
    public Bitmap? MiniPicture
    {
        get => _miniPicture;
        set
        {
            _miniPicture = value;
            this.RaisePropertyChanged("MiniPicture");
        }
    }


    public string StandbyColor { get; } = "#aaaaaa";
    public string RecordingColor { get;  } = "#dd8822";
    public string StateColor => IsRecording ? RecordingColor : StandbyColor;

    private Bitmap NoRecord; 
    private Bitmap Record;
    

    public Bitmap RecordImage
    {
        get => IsRecording ? Record : NoRecord;
    }
    

    public async void RecordClicked()
    {
        if (_channelPresentation.IsRecording)
        {
            _channelPresentation.StateString = "stopping...";
            this.RaisePropertyChanged("StateString");
            await Task.Delay(500);
            IsRecording = false;
        }
        else
        {
            _channelPresentation.StateString = "starting...";
            this.RaisePropertyChanged("StateString");
            await Task.Delay(200);
            IsRecording = true;
        }
    }
    

    public void Dispose()
    {
        _rtcWebSocketServer.Close();
    }
    

    public ChannelPresentationViewModel(
        ChannelPresentation channelPresentation)
    {
        _channelPresentation = channelPresentation;
        MiniPicture = new Bitmap(AssetLoader.Open(new Uri(_channelPresentation.Uri)));
        NoRecord = new Bitmap(AssetLoader.Open(new Uri("avares://ProduceNowApp/Assets/NoRecord.png")));
        Record = new Bitmap(AssetLoader.Open(new Uri("avares://ProduceNowApp/Assets/Record.png")));
        _rtcWebSocketServer = new();
        _rtcWebSocketServer.Setup(_database.ClientConfig.Settings.ConfigUrl);
        _rtcWebSocketServer.OnNewBitmap += (sender, bitmap) =>
        {
            MiniPicture = bitmap;
        };
        _rtcWebSocketServer.Start();
    }
}