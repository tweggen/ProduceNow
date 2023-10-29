using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DemoContent;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.X509;
using Serilog;
using Serilog.Extensions.Logging;
using SIPSorcery.Net;


public class Program
{
    private static WebRTCPeer _webRtcPeer;

    private static RTCCertificate2 _rtcCertificate2;
    
    private static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
    {
        var serilogLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();
        var factory = new SerilogLoggerFactory(serilogLogger);
        SIPSorcery.LogFactory.Set(factory);
        return factory.CreateLogger("DemoContent");
    }


    static void _createNewWebRtcPeer()
    {
        _webRtcPeer = new WebRTCPeer()
        {
            RtcCertificate2 = _rtcCertificate2,
            UrlSignalingServer = EnvUrlSignalingServer,
            MyName = EnvNameSource,
            TargetName = EnvNameTarget
        };
        _webRtcPeer.OnClose += _onCloseWebRtcPeer;
        _webRtcPeer.Start();
    }


    static void _closeOldWebRtcPeer()
    {
        _webRtcPeer.Dispose();
    }
    

    static void _onCloseWebRtcPeer(string reason)
    {
        _closeOldWebRtcPeer();
        _createNewWebRtcPeer();
    }

    private static string EnvUrlSignalingServer;
    private static string EnvNameSource;
    private static string EnvNameTarget;
    private static string EnvIpAddress;
    
    
    static void Main()
    {
        AddConsoleLogger();

        string? envUrlSignalingServer = System.Environment.GetEnvironmentVariable("DEMOCONTENT_URL_SIGNALING_SERVER");
        if (string.IsNullOrWhiteSpace(envUrlSignalingServer))
        {
            envUrlSignalingServer = "http://192.168.178.21:5245/api/WebRTCSignal";
        }
        EnvUrlSignalingServer = envUrlSignalingServer;

        string? envNameSource = System.Environment.GetEnvironmentVariable("DEMOCONTENT_MY_NAME");
        if (string.IsNullOrWhiteSpace(envNameSource))
        {
            envNameSource = "uni";
        }
        EnvNameSource = envNameSource;

        string? envNameTarget = System.Environment.GetEnvironmentVariable("DEMOCONTENT_TARGET_NAME");
        if (string.IsNullOrWhiteSpace(envNameTarget))
        {
            envNameTarget = "bro";
        }
        EnvNameTarget = envNameTarget;

        string envIpAddress = System.Environment.GetEnvironmentVariable("DEMOCONTENT_IP_ADDRESS");
        if (string.IsNullOrWhiteSpace(envIpAddress))
        {
            envIpAddress = "192.168.178.21";
        }

        EnvIpAddress = envIpAddress;
            
        Console.WriteLine("DemoContent");
        Console.WriteLine("Creating certificate");
        Org.BouncyCastle.X509.X509Certificate certificate;
        AsymmetricKeyParameter privateKey;
        (certificate, privateKey) = DtlsUtils.CreateSelfSignedBouncyCastleCert();
        _rtcCertificate2 = new() { Certificate = certificate, PrivateKey = privateKey };
        Console.WriteLine("Announcing.");
        _createNewWebRtcPeer();
        
        Console.WriteLine("Press any key exit.");
        Console.ReadLine();
    }

}
