using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Media.Transformation;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.X509;
// using Serilog;
// using Serilog.Events;
// using Serilog.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorcery.Sys;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using Splat;
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
    private string EnvUrlSignalingServer;
    private string EnvNameSource;
    private string EnvNameTarget;
    
    private static Microsoft.Extensions.Logging.ILogger logger = AddConsoleLogger(); // NullLogger.Instance;

    private RTCOptions _rtcOptions = new();
    private WebRTCRestSignalingPeer? _webrtcRestSignaling = null;
    private AvaloniaVideoEndpoint _videoEP;
    private CancellationTokenSource _cts;
    
    public EventHandler<Bitmap> OnNewBitmap;


    private void _loadEnvironment()
    {
        string? envUrlSignalingServer = System.Environment.GetEnvironmentVariable("PRODUCENOW_URL_SIGNALING_SERVER");
        if (string.IsNullOrWhiteSpace(envUrlSignalingServer))
        {
            envUrlSignalingServer = "http://192.168.178.21:5245/api/WebRTCSignal";
        }

        EnvUrlSignalingServer = envUrlSignalingServer;

        string? envNameSource = System.Environment.GetEnvironmentVariable("PRODUCENOW_MY_NAME");
        if (string.IsNullOrWhiteSpace(envNameSource))
        {
            envNameSource = "bro";
        }

        EnvNameSource = envNameSource;

        string? envNameTarget = System.Environment.GetEnvironmentVariable("PRODUCENOW_TARGET_NAME");
        if (string.IsNullOrWhiteSpace(envNameTarget))
        {
            envNameTarget = "uni";
        }

        EnvNameTarget = envNameTarget;
    }
    

    private static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
    {
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
            builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("ProduceNow");
        SIPSorcery.LogFactory.Set(loggerFactory);
        return logger;
    }

    
    private void _onVideoSinkDecodedSampleFaster(RawImage rawImage)
    {
        logger.LogDebug($"SampleFaster called.");
    }


    private static unsafe void YUV420toRGBA(
        Span<byte> spanRGBA, int width, int height,
        byte[] YUVData)
    {

        fixed (byte *pRGBAs = spanRGBA, pYUVs = YUVData)
        {
            for (int row = 0; row < height; row++)
            {
                byte *pRGBA = pRGBAs + row * width * 4;
                byte* pY = pYUVs + 0 + row * width;
                byte* pU = pYUVs + width*height + (row/2) * (width/2);
                byte* pV = pYUVs + width*height + (width/2)*(height/2) + (row/2) * (width/2);

                for (int col = 0; col < width; col+=2)
                {
                    int C1 = pY[0] - 16;
                    int C2 = pY[1] - 16;
                    int D = pU[0] - 128;
                    int E = pV[0] - 128;

                    {
                        int R1 = (298 * C1 + 409 * E + 128) >> 8;
                        int G1 = (298 * C1 - 100 * D - 208 * E + 128) >> 8;
                        int B1 = (298 * C1 + 516 * D + 128) >> 8;

                        pRGBA[0] = (byte)(R1);
                        pRGBA[1] = (byte)(G1);
                        pRGBA[2] = (byte)(B1);
                        pRGBA[3] = (byte)(0xff);
                    }
                    {
                        int R2 = (298 * C2 + 409 * E + 128) >> 8;
                        int G2 = (298 * C2 - 100 * D - 208 * E + 128) >> 8;
                        int B2 = (298 * C2 + 516 * D + 128) >> 8;

                        pRGBA[4] = (byte)(R2);
                        pRGBA[5] = (byte)(G2);
                        pRGBA[6] = (byte)(B2);
                        pRGBA[7] = (byte)(0xff);
                    }

                    pRGBA += 8;
                    pY += 2;
                    pU++;
                    pV++;
                }
            }
        }
    }
    
    
    private static unsafe WriteableBitmap CreateBitmapFromYUVPixelData(
        byte[] yuvPixelData,
        uint pixelWidth,
        uint pixelHeight,
        int stride,
        VideoPixelFormatsEnum pixFormat)
    {
        // Standard may need to change on some devices 
        Vector dpi = new Vector(96, 96);
        PixelFormat avPixFormat = PixelFormat.Bgra8888;

        var bitmap = new WriteableBitmap(
            new PixelSize((int)pixelWidth, (int)pixelHeight),
            dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using (var frameBuffer = bitmap.Lock())
        {
            var spanRGBA = new Span<byte>(frameBuffer.Address.ToPointer(),
                frameBuffer.RowBytes * frameBuffer.Size.Height);
            YUV420toRGBA(spanRGBA, (int)pixelWidth, (int)pixelHeight, yuvPixelData);
        }

        return bitmap;
    }

    private static WriteableBitmap CreateBitmapFromRGBPixelData( 
        byte[] bgraPixelData, 
        uint pixelWidth, 
        uint pixelHeight,
        int stride,
        VideoPixelFormatsEnum pixFormat) 
    { 
        // Standard may need to change on some devices 
        Vector dpi = new Vector(96, 96);
        PixelFormat avPixFormat;
        switch (pixFormat)
        {
            case VideoPixelFormatsEnum.Bgr:
                avPixFormat = PixelFormat.Bgra8888;
                break;
            default:
            case VideoPixelFormatsEnum.NV12:
            case VideoPixelFormatsEnum.Bgra:
                avPixFormat = PixelFormat.Bgra8888;
                break;
            case VideoPixelFormatsEnum.I420:
            case VideoPixelFormatsEnum.Rgb:
                avPixFormat = PixelFormat.Rgba8888;
                break;
        }
  
        var bitmap = new WriteableBitmap( 
            new PixelSize((int)pixelWidth, (int)pixelHeight), 
            dpi, 
            PixelFormat.Bgra8888, 
            AlphaFormat.Premul); 
  
        using (var frameBuffer = bitmap.Lock())
        {
            for (int y = 0; y < pixelHeight; ++y)
            {
                //Span<byte> pixelRowSpan = bgraPixelData.AsSpan(y * stride);
                IntPtr pixelRowPtr = frameBuffer.Address + y * frameBuffer.RowBytes;
                
                for (int x = 0; x < pixelWidth; x++)
                {
                    byte[] rgbaPixel =
                    {
                        bgraPixelData[y*stride+x*3+0], 
                        bgraPixelData[y*stride+x*3+1], 
                        bgraPixelData[y*stride+x*3+2], 255
                    };
                    Marshal.Copy(rgbaPixel, 0, pixelRowPtr + x * 4, 4);
                }
            }
            //Marshal.Copy(
            //    bgraPixelData, 0, frameBuffer.Address, bgraPixelData.Length); 
        } 
  
        return bitmap; 
    } 

    private void _onVideoSinkDecodedSample(
        byte[] sample,
        uint width,
        uint height,
        int stride,
        VideoPixelFormatsEnum pixelFormat)
    {
        var l = sample.Length;
        WriteableBitmap? bitmap = null;
        
        /*
         * TXWTODO: Find a proper way to pass the color representation. 
         */
        if (l >= (width * height * 3))
        {
            bitmap = CreateBitmapFromRGBPixelData(sample, width, height, stride, pixelFormat);
        }
        else
        {
            bitmap = CreateBitmapFromYUVPixelData(sample, width, height, stride, pixelFormat);
        }

        OnNewBitmap?.Invoke(this, bitmap);
    }
    
    
    private Task<RTCPeerConnection> CreatePeerConnection()
    {
        logger.LogInformation("CreatePeerConnection(): Called.");
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new VpxVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new FFmpegVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);
        
        RTCPeerConnection pc = null;
        try
        {
            
            var certStore = Locator.Current.GetService<CertificateStore>();
            RTCConfiguration config = new RTCConfiguration
            {
                //iceServers = new List<RTCIceServer> { new RTCIceServer { urls = STUN_URL } }
                X_UseRtpFeedbackProfile = true,
                X_BindAddress = IPAddress.Any,
                certificates2 = new List<RTCCertificate2>() 
                {
                    certStore.CreateRtcCertificate2()
                }
            };
            pc = new RTCPeerConnection(config);
        }
        catch (Exception e)
        {
            logger.LogError($"Caught exception {e}.");
        }


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
        _videoEP.OnVideoSinkDecodedSampleFaster += _onVideoSinkDecodedSampleFaster;
        _videoEP.OnVideoSinkDecodedSample += _onVideoSinkDecodedSample;

        if (_webrtcRestSignaling != null)
        {
            return _webrtcRestSignaling.Start(_cts);
        }
        else
        {
            return Task.CompletedTask;
        }
    }


    public void Close()
    {
        _cts.Cancel();
        _webrtcRestSignaling?.RTCPeerConnection?.Close("" /* empty reason string */);
    }

    public void Setup(string serverUrl)
    {
        _loadEnvironment();
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            EnvUrlSignalingServer = serverUrl+"/api/WebRTCSignal";
        }
        // logger = AddConsoleLogger();
        _cts = new CancellationTokenSource();
        logger.LogError("Serilog: gravi off navi on!");
        Console.WriteLine("Console: gravi off navi on!");
        Debug.WriteLine("Debug: gravi off navi on!");

        var videoEncoder = Splat.Locator.Current.GetService<IVideoEncoder>();
        _videoEP = new AvaloniaVideoEndpoint(videoEncoder);
        _videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);
        
        try
        {
            _webrtcRestSignaling = new WebRTCRestSignalingPeer(
                EnvUrlSignalingServer,
                EnvNameSource,
                EnvNameTarget,
                this.CreatePeerConnection);
        }
        catch (Exception e)
        {
            _webrtcRestSignaling = null;
        }

    }

}
