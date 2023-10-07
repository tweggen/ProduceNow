#if true

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorcery.Sys;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using WebSocketSharp.Server;

namespace ProduceNowApp.Services;


public class RTCOptions
{
    public string WSSCertificate { get; set; }
    public bool UseIPv6 { get; set; }
    public bool NoAudio { get; set; }
}


public class RTCWebSocketServer
{
    private const string STUN_URL = "stun:stun.sipsorcery.com";
    private const int WEBSOCKET_PORT = 8081;

    private static Microsoft.Extensions.Logging.ILogger logger = NullLogger.Instance;

    private RTCOptions _rtcOptions = new();
    private WebRTCRestSignalingPeer _webrtcRestSignaling;
    private AvaloniaVideoEndpoint _videoEP;
    private CancellationTokenSource _cts;

    private static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
    {
        var serilogLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();
        var factory = new SerilogLoggerFactory(serilogLogger);
        SIPSorcery.LogFactory.Set(factory);
        return factory.CreateLogger<RTCWebSocketServer>();
    }

    private static X509Certificate2 LoadCertificate(string path)
    {
        if (!File.Exists(path))
        {
            logger.LogWarning($"No certificate file could be found at {path}.");
            return null;
        }
        else
        {
            X509Certificate2 cert = new X509Certificate2(path, "", X509KeyStorageFlags.Exportable);
            if (cert == null)
            {
                logger.LogWarning($"Failed to load X509 certificate from file {path}.");
            }
            else
            {
                logger.LogInformation(
                    $"Certificate file successfully loaded {cert.Subject}, thumbprint {cert.Thumbprint}, has private key {cert.HasPrivateKey}.");
            }

            return cert;
        }
    }

    private void _onVideoSinkDecodedSampleFaster(RawImage rawImage)
    {
    
    }
    
    
    private void _onVideoSinkDecodedSample(
        byte[] sample,
        uint width,
        uint height,
        int stride,
        VideoPixelFormatsEnum pixelFormat)
    {
    }
    
    
    private Task<RTCPeerConnection> CreatePeerConnection()
    {
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new VpxVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new FFmpegVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);
        RTCConfiguration config = new RTCConfiguration
        {
            //iceServers = new List<RTCIceServer> { new RTCIceServer { urls = STUN_URL } }
            X_UseRtpFeedbackProfile = true
        };
        var pc = new RTCPeerConnection(config);
        
        
        // Add local receive only tracks. This ensures that the SDP answer includes only the codecs we support.
        if (!_rtcOptions.NoAudio)
        {
            MediaStreamTrack audioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPWellKnownMediaFormatsEnum.PCMU) },
                MediaStreamStatusEnum.RecvOnly);
            pc.addTrack(audioTrack);
        }

        MediaStreamTrack videoTrack =
            new MediaStreamTrack(_videoEP.GetVideoSinkFormats(), MediaStreamStatusEnum.RecvOnly);
        //MediaStreamTrack videoTrack = new MediaStreamTrack(new VideoFormat(96, "VP8", 90000, "x-google-max-bitrate=5000000"), MediaStreamStatusEnum.RecvOnly);
        pc.addTrack(videoTrack);

        pc.OnVideoFrameReceived += _videoEP.GotVideoFrame;
        pc.OnVideoFormatsNegotiated += (formats) => _videoEP.SetVideoSinkFormat(formats.First());


        pc.onconnectionstatechange += async (state) =>
        {
            logger.LogDebug($"Peer connection state change to {state}.");

            if (state == RTCPeerConnectionState.failed)
            {
                pc.Close("ice disconnection");
            }
            else if (state == RTCPeerConnectionState.closed)
            {
                await _videoEP.CloseVideoSink();
            }
        };

        // Diagnostics.
        pc.OnReceiveReport += (re, media, rr) => logger.LogDebug($"RTCP Receive for {media} from {re}\n{rr.GetDebugSummary()}");
        pc.OnSendReport += (media, sr) => logger.LogDebug($"RTCP Send for {media}\n{sr.GetDebugSummary()}");
        pc.GetRtpChannel().OnStunMessageReceived += (msg, ep, isRelay) => logger.LogDebug($"RECV STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) from {ep}.");
        pc.GetRtpChannel().OnStunMessageSent += (msg, ep, isRelay) => logger.LogDebug($"SEND STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) to {ep}.");
        pc.oniceconnectionstatechange += (state) => logger.LogDebug($"ICE connection state change to {state}.");

        return Task.FromResult(pc);
    }


    public Task Start()
    {
        //VideoEncoderEndPoint.OnVideoSinkDecodedSample += OnVideoFrame;
        
        _videoEP.OnVideoSinkDecodedSampleFaster += _onVideoSinkDecodedSampleFaster;
        _videoEP.OnVideoSinkDecodedSample += _onVideoSinkDecodedSample;

        return _webrtcRestSignaling.Start(_cts);
    }


    public void Close()
    {
        _cts.Cancel();
        _webrtcRestSignaling?.RTCPeerConnection?.Close("" /* empty reason string */);
    }

    private const string REST_SIGNALING_SERVER = "http://localhost:8081"; //"https://sipsorcery.cloud/api/webrtcsignal";
    private const string REST_SIGNALING_MY_ID = "unity";
    private const string REST_SIGNALING_THEIR_ID = "svr";
    
    public void Setup()
    {
        AddConsoleLogger();
        _cts = new CancellationTokenSource();
        
        _videoEP = new AvaloniaVideoEndpoint(new VpxVideoEncoder());
        _videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);

        _webrtcRestSignaling = new WebRTCRestSignalingPeer(
            REST_SIGNALING_SERVER,
            REST_SIGNALING_MY_ID,
            REST_SIGNALING_THEIR_ID,
            this.CreatePeerConnection);

    }

}
#endif