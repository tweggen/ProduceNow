using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorcery.Sys;

namespace ProduceNow.DemoContent;

public class WebRTCPeer : IDisposable
{
    public IPAddress MyIpAddress { get; set; } = null;
    public string MyHostName { get; set; }
    public string UrlSignalingServer { get; set; }= "http://192.168.178.21:5245/api/WebRTCSignal"; // "http://localhost:5245/api/WebRTCSignal"; // "https://sipsorcery.cloud/api/webrtcsignal";
    public string MyName { get; set; } = "uni";
    public string TargetName { get; set; } = "bro";
    public ushort PortPairBegin { get; set; } = 0;

    private static ILogger logger;

    private bool _isClosed = false;
    
    private WebRTCRestSignalingPeer _webrtcRestSignaling;
    public Video.IVideoEndPoint  VideoEncoderEndPoint { get; }
    
    private CancellationTokenSource _cts;

    public delegate void OnCloseHandler(string reason);
    public event OnCloseHandler OnClose;
    
    public RTCCertificate2 RtcCertificate2;
    
    
    private Task<SIPSorcery.Net.RTCPeerConnection> CreatePeerConnection()
    {
        logger.LogInformation($"Binding to IP address {MyIpAddress}");
        var pc = new SIPSorcery.Net.RTCPeerConnection(new RTCConfiguration()
            {
                X_BindAddress = MyIpAddress,
                certificates2 = new List<RTCCertificate2>() { this.RtcCertificate2 }
            }, 
            0, 
            (PortPairBegin!=0)?new PortRange(PortPairBegin, PortPairBegin+2):null
            );

        // Set up sources and hook up send events to peer connection.
        //AudioExtrasSource audioSrc = new AudioExtrasSource(new AudioEncoder(), new AudioSourceOptions { AudioSource = AudioSourcesEnum.None });
        //audioSrc.OnAudioSourceEncodedSample += pc.SendAudio;

        var testPatternSource = new VideoTestPatternSource();
            
        testPatternSource.SetMaxFrameRate(true);
        testPatternSource.OnVideoSourceRawSample += VideoEncoderEndPoint.ExternalVideoSourceRawSample;
        
        VideoEncoderEndPoint.OnVideoSourceEncodedSample += pc.SendVideo;

        // Add tracks.
        //var audioTrack = new SIPSorcery.Net.MediaStreamTrack(audioSrc.GetAudioSourceFormats(), SIPSorcery.Net.MediaStreamStatusEnum.SendOnly);
        //pc.addTrack(audioTrack);
        var videoTrack = new SIPSorcery.Net.MediaStreamTrack(VideoEncoderEndPoint.GetVideoSourceFormats(), SIPSorcery.Net.MediaStreamStatusEnum.SendOnly);
        pc.addTrack(videoTrack);

        // Handlers to set the codecs to use on the sources once the SDP negotiation is complete.
        pc.OnVideoFormatsNegotiated += (sdpFormat) => VideoEncoderEndPoint.SetVideoSourceFormat(sdpFormat.First());
        //pc.OnAudioFormatsNegotiated += (sdpFormat) => audioSrc.SetAudioSourceFormat(sdpFormat.First());

        pc.OnTimeout += (mediaType) => logger.LogDebug($"Timeout on media {mediaType}.");
        pc.oniceconnectionstatechange += (state) => logger.LogDebug($"ICE connection state changed to {state}.");
        pc.onconnectionstatechange += async (state) =>
        {
            logger.LogDebug($"Peer connection connected changed to {state}.");
            if (state == SIPSorcery.Net.RTCPeerConnectionState.connected)
            {
                //await audioSrc.StartAudio();
                await testPatternSource.StartVideo();
            }
            else if (state == SIPSorcery.Net.RTCPeerConnectionState.closed || state == SIPSorcery.Net.RTCPeerConnectionState.failed)
            {
                //await audioSrc.CloseAudio();
                await testPatternSource.CloseVideo();
                this.Close("Was Disconnected");
            }
        };

        //pc.GetRtpChannel().OnStunMessageReceived += (msg, ep, isRelay) =>
        //{
        //    bool hasUseCandidate = msg.Attributes.Any(x => x.AttributeType == SIPSorcery.Net.STUNAttributeTypesEnum.UseCandidate);
        //    logger.LogDebug($"STUN {msg.Header.MessageType} received from {ep}, use candidate {hasUseCandidate}.");
        //};

        return Task.FromResult(pc);
    }


    private void _computeIP()
    {
        if (null == MyIpAddress)
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        var ipProperties = ni.GetIPProperties();
                        if (ipProperties.GatewayAddresses.Count > 0)
                        {
                            logger.LogInformation($"Using network interface {ni.Name}");
                            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    MyIpAddress = ip.Address;
                                    logger.LogInformation($"Using IP address {MyIpAddress}");
                                    break;
                                }
                            }

                            if (MyIpAddress != null) break;
                        }
                    }
                }
            }
        }

        if (null == MyIpAddress)
        {
            MyIpAddress = IPAddress.Parse("192.168.178.21");
        }
    }
    
    
    public Task Start()
    {
        _computeIP();
        logger.LogInformation($"Listening to IP address {MyIpAddress.ToString()}");
        _cts = new CancellationTokenSource();
        
        _webrtcRestSignaling = new WebRTCRestSignalingPeer(
            UrlSignalingServer,
            MyName, TargetName,
            this.CreatePeerConnection);
        return _webrtcRestSignaling.Start(_cts);
        //return Task.CompletedTask;
    }

    
    public void Close(string reason)
    {
        if (!_isClosed)
        {
            _isClosed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            if (null != _webrtcRestSignaling.RTCPeerConnection)
            {
                if (!_webrtcRestSignaling.RTCPeerConnection.IsClosed)
                {
                    _webrtcRestSignaling.RTCPeerConnection.Close(reason);
                }
            }
            VideoEncoderEndPoint?.CloseVideo();
            OnClose?.Invoke(reason);
        }
    }


    public void Dispose()
    {
        if (!_isClosed)
        {
            Close("Disposing");
        }
    }
    
    
    public WebRTCPeer()
    {
        logger = Common.ApplicationLogging.LoggerFactory.CreateLogger<WebRTCPeer>();

        if (FFmpeg.Owner.Instance != null)
        {
            try
            {
                this.VideoEncoderEndPoint = new FFmpeg.EndPoint() { Width = 640, Height = 480, FramesPerSecond = 30 };
            }
            catch (Exception e)
            {
                logger.LogInformation($"Unable to instantiate ffmpeg video encoder: {e}.");
            }
        }

        if (null == this.VideoEncoderEndPoint)
        {
            try
            {
                VideoEncoderEndPoint = new Vpx.EndPoint();
            }
            catch (Exception e)
            {
                logger.LogInformation("Unable to use libvpx encoder. Trying next.");
            }
        }

        if (null == VideoEncoderEndPoint)
        {
            throw new ApplicationException("Unable to find an encoder implementation.");
        }
        
        MyHostName = Dns.GetHostName();
        logger.LogInformation($"Running on host \"{MyHostName}\"");
    }
}
