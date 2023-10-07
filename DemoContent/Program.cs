using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DemoContent;
using Serilog;
using Serilog.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Encoders;
using WebSocketSharp.Server;

public class Program
{
    private static WebRTCPeer _webRtcPeer;
    private const int WEBSOCKET_PORT = 8081;

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


    static void Main()
    {
        AddConsoleLogger();
        Console.WriteLine("DemoContent");

        #if false 
        // Start web socket.
        Console.WriteLine("Starting web socket server...");
        var webSocketServer = new WebSocketServer(IPAddress.Any, WEBSOCKET_PORT);
        webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/",
            (peer) => peer.CreatePeerConnection = () => CreatePeerConnection());
        webSocketServer.Start();

        Console.WriteLine($"Waiting for web socket connections on {webSocketServer.Address}:{webSocketServer.Port}...");

        #endif
        
        
        _webRtcPeer = new WebRTCPeer();
        _webRtcPeer.Start();
        
        Console.WriteLine("Press any key exit.");
        Console.ReadLine();
    }

    #if false
    private static Task<RTCPeerConnection> CreatePeerConnection()
    {
        var pc = new RTCPeerConnection(null);

        var testPatternSource = new VideoTestPatternSource(new VpxVideoEncoder());

        MediaStreamTrack videoTrack =
            new MediaStreamTrack(testPatternSource.GetVideoSourceFormats(), MediaStreamStatusEnum.SendOnly);
        pc.addTrack(videoTrack);

        testPatternSource.OnVideoSourceEncodedSample += pc.SendVideo;
        pc.OnVideoFormatsNegotiated += (formats) => testPatternSource.SetVideoSourceFormat(formats.First());

        pc.onconnectionstatechange += async (state) =>
        {
            Console.WriteLine($"Peer connection state change to {state}.");

            switch (state)
            {
                case RTCPeerConnectionState.connected:
                    await testPatternSource.StartVideo();
                    break;
                case RTCPeerConnectionState.failed:
                    pc.Close("ice disconnection");
                    break;
                case RTCPeerConnectionState.closed:
                    await testPatternSource.CloseVideo();
                    testPatternSource.Dispose();
                    break;
            }
        };

        return Task.FromResult(pc);
    }
#endif
}
