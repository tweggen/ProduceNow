using System.Net;
using Org.BouncyCastle.Crypto;
using SIPSorcery.Net;
using Microsoft.Extensions.Logging;

namespace DemoContent;

public class Main
{
    private ILogger _logger;
    private WebRTCPeer _webRtcPeer;
    private RTCCertificate2 _rtcCertificate2;

    
    void _createNewWebRtcPeer()
    {
        _webRtcPeer = new WebRTCPeer()
        {
            RtcCertificate2 = _rtcCertificate2,
            UrlSignalingServer = EnvUrlSignalingServer,
            MyName = EnvNameSource,
            TargetName = EnvNameTarget,
            PortPairBegin = EnvRtpPortPair
        };
        if (null != EnvIpAddress)
        {
            _webRtcPeer.MyIpAddress = IPAddress.Parse(EnvIpAddress);
        }

        _webRtcPeer.OnClose += _onCloseWebRtcPeer;
        _webRtcPeer.Start();
    }


    void _closeOldWebRtcPeer()
    {
        _webRtcPeer.Dispose();
    }
    

    void _onCloseWebRtcPeer(string reason)
    {
        _closeOldWebRtcPeer();
        _createNewWebRtcPeer();
    }

    private string EnvUrlSignalingServer;
    private string EnvNameSource;
    private string EnvNameTarget;
    private string EnvIpAddress;
    private ushort EnvRtpPortPair = 0;


    private void _setup()
    {
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

        }
        else
        {
            EnvIpAddress = envIpAddress;
        }

        string envRtpPortPair = System.Environment.GetEnvironmentVariable("DEMOCONTENT_RTP_PORT_PAIR");
        if (string.IsNullOrEmpty(envRtpPortPair))
        {

        }
        else
        {
            EnvRtpPortPair = (ushort)Int32.Parse(envRtpPortPair);
        }
        
        _logger.LogInformation("Creating certificate");
        Org.BouncyCastle.X509.X509Certificate certificate;
        AsymmetricKeyParameter privateKey;
        (certificate, privateKey) = DtlsUtils.CreateSelfSignedBouncyCastleCert();
        _rtcCertificate2 = new() { Certificate = certificate, PrivateKey = privateKey };
    }
    
    
    public void Start()
    {
        _logger.LogInformation("Announcing.");
        _createNewWebRtcPeer();
    }

    public Main()
    {
        _logger = ApplicationLogging.LoggerFactory.CreateLogger<Main>();
        _setup();
    }
}