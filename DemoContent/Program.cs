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
            RtcCertificate2 = _rtcCertificate2
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
    
    
    static void Main()
    {
        AddConsoleLogger();
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
