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
    private const string STUN_URL = "stun:stun.sipsorcery.com";
    private const int WEBSOCKET_PORT = 8081;

    private static Microsoft.Extensions.Logging.ILogger logger = AddConsoleLogger(); // NullLogger.Instance;

    private RTCOptions _rtcOptions = new();
    private WebRTCRestSignalingPeer? _webrtcRestSignaling = null;
    private AvaloniaVideoEndpoint _videoEP;
    private CancellationTokenSource _cts;
    
    public EventHandler<Bitmap> OnNewBitmap;


    private static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
    {
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
            builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("ProduceNow");
        SIPSorcery.LogFactory.Set(loggerFactory);
        return logger;

#if false
        var serilogLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
            .WriteTo.Console(LogEventLevel.Verbose)
            .CreateLogger();
        var factory = new SerilogLoggerFactory(serilogLogger);
        SIPSorcery.LogFactory.Set(factory);
        return factory.CreateLogger<App>();
#endif
    }


    static private string cert =
        "MIIDjDCCAnSgAwIBAgIEN0KUPTANBgkqhkiG9w0BAQsFADBbMScwJQYDVQQDDB5SZWdlcnkgU2Vs"+
        "Zi1TaWduZWQgQ2VydGlmaWNhdGUxIzAhBgNVBAoMGlJlZ2VyeSwgaHR0cHM6Ly9yZWdlcnkuY29t"+
        "MQswCQYDVQQGEwJVQTAgFw0yMzEwMTgwMDAwMDBaGA8yMTIzMTAxODE2MjA1MVowTjEaMBgGA1UE"+
        "AwwRbmFzc2F1LXJlY29yZHMuZGUxIzAhBgNVBAoMGlJlZ2VyeSwgaHR0cHM6Ly9yZWdlcnkuY29t"+
        "MQswCQYDVQQGEwJVQTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANIqQ7Dx3+icIE8K"+
        "VvXBikBCrOMEAnfoncXoCYfvkYa/tNPdsKkIgeCgvDaaQHaN6Lu5TLhxXO8I9Nsc/XuRF2TyY5id"+
        "fYSqU8eha8Vq4PBSWzTG7pTxgsBq8a3POpu1JgxyUAD7EKdbTufkDiAAf3+7s1MUfgmJqY5+E3G/"+
        "Xk5uYX2d02RYXyNMKBOQZB3HINH7DTZoqMCUzhGbkbHHf6I8q/optTnbMKDoEdyXyrIfRH6W75pm"+
        "xtcMxLp5H6aV/6I9xrnIH5g4BNQ0JhEB49JDqOMfZgv9cX8P0Oze7gEpNkD578iVuyiD+w5Aq2gM"+
        "h+GKftwqHz2J1KJsD6DSGXsCAwEAAaNjMGEwDwYDVR0TAQH/BAUwAwEB/zAOBgNVHQ8BAf8EBAMC"+
        "AYYwHQYDVR0OBBYEFFFmL53VM+4Fc4KJjQCMvK07A2EDMB8GA1UdIwQYMBaAFFFmL53VM+4Fc4KJ"+
        "jQCMvK07A2EDMA0GCSqGSIb3DQEBCwUAA4IBAQALeZnl8om/N3UuREW8onQZ2J1OsOSZHYl55UIG"+
        "wo1HppHhQG+A9iTM4o8WC4Tdh5qGNEkQHWjgnxGQIOB++ukB+hlpDHw7atCtQoUwVb1KF8g5WmlR"+
        "lhKlNxtZbLIOWFySOIQvK5jLhoURftDK+vEzT1xfg+1CCkCKt10l9dn7OMqUqWvTVY0uuvcvMYbK"+
        "DYIC6lepRSeQdUrkbGA9bNjz7kYA4n8omklNpRbSL744kKIgAww2UsXFVOO5pP6tWSFLKRFmowFE"+
        "3ZUEjvOG4h1ZHzxhlYSfjeqQC30HZkrmTfgbvYFCKMYO/rEDd2ggHVR8AdFWjLkOhGtavuiTl6a5"
        ;

    #if true
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
    #endif


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
    
    
    #if false
    
    private static unsafe void YUV2RGBManaged(byte[] YUVData, byte[] RGBData, int width, int height)
    {

        //returned pixel format is 2yuv - i.e. luminance, y, is represented for every pixel and the u and v are alternated
        //like this (where Cb = u , Cr = y)
        //Y0 Cb Y1 Cr Y2 Cb Y3 

        /*http://msdn.microsoft.com/en-us/library/ms893078.aspx
         *
         * C = Y - 16
         D = U - 128
         E = V - 128
         R = clip(( 298 * C           + 409 * E + 128) >> 8)
         G = clip(( 298 * C - 100 * D - 208 * E + 128) >> 8)
         B = clip(( 298 * C + 516 * D           + 128) >> 8)

         * here are a whole bunch more formats for doing this...
         * http://stackoverflow.com/questions/3943779/converting-to-yuv-ycbcr-colour-space-many-versions
         */


        fixed (byte* pRGBs = RGBData, pYUVs = YUVData)
        {
            for (int r = 0; r < height; r++)
            {
                byte* pRGB = pRGBs + r * width * 3;
                byte* pYUV = pYUVs + r * width * 2;

                //process two pixels at a time
                for (int c = 0; c < width; c += 2)
                {
                    int C1 = pYUV[1] - 16;
                    int C2 = pYUV[3] - 16;
                    int D = pYUV[2] - 128;
                    int E = pYUV[0] - 128;

                    int R1 = (298 * C1 + 409 * E + 128) >> 8;
                    int G1 = (298 * C1 - 100 * D - 208 * E + 128) >> 8;
                    int B1 = (298 * C1 + 516 * D + 128) >> 8;

                    int R2 = (298 * C2 + 409 * E + 128) >> 8;
                    int G2 = (298 * C2 - 100 * D - 208 * E + 128) >> 8;
                    int B2 = (298 * C2 + 516 * D + 128) >> 8;
#if true
                    //check for overflow
                    //unsurprisingly this takes the bulk of the time.
                    pRGB[0] = (byte)(R1 < 0 ? 0 : R1 > 255 ? 255 : R1);
                    pRGB[1] = (byte)(G1 < 0 ? 0 : G1 > 255 ? 255 : G1);
                    pRGB[2] = (byte)(B1 < 0 ? 0 : B1 > 255 ? 255 : B1);

                    pRGB[3] = (byte)(R2 < 0 ? 0 : R2 > 255 ? 255 : R2);
                    pRGB[4] = (byte)(G2 < 0 ? 0 : G2 > 255 ? 255 : G2);
                    pRGB[5] = (byte)(B2 < 0 ? 0 : B2 > 255 ? 255 : B2);
#else
                    pRGB[0] = (byte)(R1);
                    pRGB[1] = (byte)(G1);
                    pRGB[2] = (byte)(B1);

                    pRGB[3] = (byte)(R2);
                    pRGB[4] = (byte)(G2);
                    pRGB[5] = (byte)(B2);
#endif

                    pRGB += 6;
                    pYUV += 4;
                }
            }
        }
    }
#endif


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

    private const string REST_SIGNALING_MY_ID = "bro";
    private const string REST_SIGNALING_THEIR_ID = "uni";
    
    public void Setup(string strServerUrl)
    {
        
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
                strServerUrl + "/api/WebRTCSignal",
                REST_SIGNALING_MY_ID,
                REST_SIGNALING_THEIR_ID,
                this.CreatePeerConnection);
        }
        catch (Exception e)
        {
            _webrtcRestSignaling = null;
        }

    }

}
