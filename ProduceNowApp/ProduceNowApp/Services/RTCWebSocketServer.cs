using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
// using SIPSorceryMedia.FFmpeg;
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


    private static Task<RTCPeerConnection> CreatePeerConnection()
    {
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new VpxVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new FFmpegVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);

#if false
            SIPSorceryMedia.FFmpeg.FFmpegInit.Initialise(SIPSorceryMedia.FFmpeg.FfmpegLogLevelEnum.AV_LOG_VERBOSE, ffmpegLibFullPath, logger);
            var videoEP = new FFmpegVideoEndPoint();
            videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);


            videoEP.OnVideoSinkDecodedSampleFaster += (RawImage rawImage) =>
            {
                _form.BeginInvoke(new Action(() =>
                {
                    if (rawImage.PixelFormat == SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum.Rgb)
                    {
                        if (_picBox.Width != rawImage.Width || _picBox.Height != rawImage.Height)
                        {
                            logger.LogDebug($"Adjusting video display from {_picBox.Width}x{_picBox.Height} to {rawImage.Width}x{rawImage.Height}.");
                            _picBox.Width = rawImage.Width;
                            _picBox.Height = rawImage.Height;
                        }

                        Bitmap bmpImage =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       new Bitmap(rawImage.Width, rawImage.Height, rawImage.Stride, PixelFormat.Format24bppRgb, rawImage.Sample);
                        _picBox.Image = bmpImage;
                    }
                }));
            };

            videoEP.OnVideoSinkDecodedSample +=
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               (byte[] bmp, uint width, uint height, int stride, VideoPixelFormatsEnum pixelFormat) =>
            {
                _form.BeginInvoke(new Action(() =>
                {
                    if (pixelFormat == SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum.Rgb)
                    {
                        if (_picBox.Width != (int)width || _picBox.Height != (int)height)
                        {
                            logger.LogDebug($"Adjusting video display from {_picBox.Width}x{_picBox.Height} to {width}x{height}.");
                            _picBox.Width = (int)width;
                            _picBox.Height = (int)height;
                        }

                        unsafe
                        {
                            fixed (byte* s = bmp)
                            {
                                Bitmap bmpImage =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               new Bitmap((int)width, (int)height, (int)(bmp.Length / height), PixelFormat.Format24bppRgb, (IntPtr)s);
                                _picBox.Image = bmpImage;
                            }
                        }
                    }
                }));
            };

            RTCConfiguration config = new RTCConfiguration
            {
                //iceServers = new List<RTCIceServer> { new RTCIceServer { urls = STUN_URL } }
                 X_UseRtpFeedbackProfile = true
            };
            var pc = new RTCPeerConnection(config);

            // Add local receive only tracks. This ensures that the SDP answer includes only the codecs we support.
            if (!_options.NoAudio)
            {
                MediaStreamTrack audioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                    new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPWellKnownMediaFormatsEnum.PCMU) }, MediaStreamStatusEnum.RecvOnly);
                pc.addTrack(audioTrack);
            }
            MediaStreamTrack videoTrack =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      new MediaStreamTrack(videoEP.GetVideoSinkFormats(), MediaStreamStatusEnum.RecvOnly);
            //MediaStreamTrack videoTrack = new MediaStreamTrack(new VideoFormat(96, "VP8", 90000, "x-google-max-bitrate=5000000"), MediaStreamStatusEnum.RecvOnly);
            pc.addTrack(videoTrack);

            pc.OnVideoFrameReceived += videoEP.GotVideoFrame;
            pc.OnVideoFormatsNegotiated += (formats) => videoEP.SetVideoSinkFormat(formats.First());

            pc.onconnectionstatechange += async (state) =>
            {
                logger.LogDebug($"Peer connection state change to {state}.");

                if (state == RTCPeerConnectionState.failed)
                {
                    pc.Close("ice disconnection");
                }
                else if (state == RTCPeerConnectionState.closed)
                {
                    await videoEP.CloseVideo();
                }
            };

            // Diagnostics.
            //pc.OnReceiveReport += (re, media, rr) => logger.LogDebug($"RTCP Receive for {media} from {re}\n{rr.GetDebugSummary()}");
            pc.OnSendReport += (media, sr) => logger.LogDebug($"RTCP Send for {media}\n{sr.GetDebugSummary()}");
            //pc.GetRtpChannel().OnStunMessageReceived += (msg, ep, isRelay) => logger.LogDebug($"RECV STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) from {ep}.");
            //pc.GetRtpChannel().OnStunMessageSent += (msg, ep, isRelay) => logger.LogDebug($"SEND STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) to {ep}.");
            pc.oniceconnectionstatechange += (state) => logger.LogDebug($"ICE connection state change to {state}.");

            return Task.FromResult(pc);
#else
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
            new MediaStreamTrack(videoEP.GetVideoSinkFormats(), MediaStreamStatusEnum.RecvOnly);
        //MediaStreamTrack videoTrack = new MediaStreamTrack(new VideoFormat(96, "VP8", 90000, "x-google-max-bitrate=5000000"), MediaStreamStatusEnum.RecvOnly);
        pc.addTrack(videoTrack);

        pc.OnVideoFrameReceived += videoEP.GotVideoFrame;
        pc.OnVideoFormatsNegotiated += (formats) => videoEP.SetVideoSinkFormat(formats.First());

        pc.onconnectionstatechange += async (state) =>
        {
            logger.LogDebug($"Peer connection state change to {state}.");

            if (state == RTCPeerConnectionState.failed)
            {
                pc.Close("ice disconnection");
            }
            else if (state == RTCPeerConnectionState.closed)
            {
                await videoEP.CloseVideo();
            }
        };

        // Diagnostics.
        //pc.OnReceiveReport += (re, media, rr) => logger.LogDebug($"RTCP Receive for {media} from {re}\n{rr.GetDebugSummary()}");
        pc.OnSendReport += (media, sr) => logger.LogDebug($"RTCP Send for {media}\n{sr.GetDebugSummary()}");
        //pc.GetRtpChannel().OnStunMessageReceived += (msg, ep, isRelay) => logger.LogDebug($"RECV STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) from {ep}.");
        //pc.GetRtpChannel().OnStunMessageSent += (msg, ep, isRelay) => logger.LogDebug($"SEND STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) to {ep}.");
        pc.oniceconnectionstatechange += (state) => logger.LogDebug($"ICE connection state change to {state}.");

        return Task.FromResult(pc);
#endif
    }


    public void Setup()
    {

        X509Certificate2 wssCertificate =
            (_rtcOptions.WSSCertificate != null) ? LoadCertificate(_rtcOptions.WSSCertificate) : null;

        // Start web socket.
        Console.WriteLine("Starting web socket server...");
        var webSocketServer = new WebSocketServer((_rtcOptions.UseIPv6) ? IPAddress.IPv6Any : IPAddress.Any,
            WEBSOCKET_PORT, wssCertificate != null);
        if (webSocketServer.IsSecure)
        {
            webSocketServer.SslConfiguration.ServerCertificate = wssCertificate;
            webSocketServer.SslConfiguration.CheckCertificateRevocation = false;
            webSocketServer.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        }

        webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/",
            (peer) => peer.CreatePeerConnection = CreatePeerConnection);
        webSocketServer.Start();
    }

}