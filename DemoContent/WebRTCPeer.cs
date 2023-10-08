using Microsoft.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;

namespace DemoContent;

public class WebRTCPeer
{
    private const string REST_SIGNALING_SERVER =  "http://localhost:5245/api/WebRTCSignal"; // "http://localhost:5245/api/WebRTCSignal"; // "https://sipsorcery.cloud/api/webrtcsignal";
    private const string REST_SIGNALING_MY_ID = "uni";
    private const string REST_SIGNALING_THEIR_ID = "bro";

    private static Microsoft.Extensions.Logging.ILogger logger;

    private WebRTCRestSignalingPeer _webrtcRestSignaling;
    public SIPSorceryMedia.Encoders.VideoEncoderEndPoint VideoEncoderEndPoint { get; }
    private CancellationTokenSource _cts;

    public WebRTCPeer()
    {
        logger = SIPSorcery.LogFactory.CreateLogger("webrtc");

        VideoEncoderEndPoint = new SIPSorceryMedia.Encoders.VideoEncoderEndPoint();
        
        _cts = new CancellationTokenSource();

        _webrtcRestSignaling = new WebRTCRestSignalingPeer(
            REST_SIGNALING_SERVER,
            REST_SIGNALING_MY_ID,
            REST_SIGNALING_THEIR_ID,
            this.CreatePeerConnection);
    }

    public Task Start()
    {
        return _webrtcRestSignaling.Start(_cts);
        //return Task.CompletedTask;
    }

    public void Close(string reason)
    {
        _cts.Cancel();
        _webrtcRestSignaling?.RTCPeerConnection?.Close(reason);
    }

    private Task<SIPSorcery.Net.RTCPeerConnection> CreatePeerConnection()
    {
        var pc = new SIPSorcery.Net.RTCPeerConnection(null);

        // Set up sources and hook up send events to peer connection.
        //AudioExtrasSource audioSrc = new AudioExtrasSource(new AudioEncoder(), new AudioSourceOptions { AudioSource = AudioSourcesEnum.None });
        //audioSrc.OnAudioSourceEncodedSample += pc.SendAudio;
        var testPatternSource = new VideoTestPatternSource();
        testPatternSource.SetMaxFrameRate(true);
        testPatternSource.OnVideoSourceRawSample += VideoEncoderEndPoint.ExternalVideoSourceRawSample;
        #if false
        testPatternSource.OnVideoSourceRawSample += delegate(uint milliseconds, int width, int height, byte[] sample,
            VideoPixelFormatsEnum format)
        {
            logger.LogDebug($"Have new frame {width}x{height}");
        };
        #endif
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
            }
        };

        //pc.GetRtpChannel().OnStunMessageReceived += (msg, ep, isRelay) =>
        //{
        //    bool hasUseCandidate = msg.Attributes.Any(x => x.AttributeType == SIPSorcery.Net.STUNAttributeTypesEnum.UseCandidate);
        //    logger.LogDebug($"STUN {msg.Header.MessageType} received from {ep}, use candidate {hasUseCandidate}.");
        //};

        return Task.FromResult(pc);
    }
}