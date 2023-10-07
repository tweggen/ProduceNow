using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SIPSorceryMedia.Abstractions;


namespace ProduceNowApp.Services;

public class AvaloniaVideoEndpoint : IVideoSink
{
    public static readonly List<VideoFormat> SupportedFormats = new List<VideoFormat>();
    private IVideoEncoder _videoDecoder;
    private MediaFormatManager<VideoFormat> _formatManager;

    public event VideoSinkSampleDecodedDelegate OnVideoSinkDecodedSample;
    public event VideoSinkSampleDecodedFasterDelegate OnVideoSinkDecodedSampleFaster;

    public AvaloniaVideoEndpoint(IVideoEncoder videoDecoder)
    {
        _videoDecoder = videoDecoder;
        _formatManager = new MediaFormatManager<VideoFormat>(videoDecoder.SupportedFormats);
    }

    public Task CloseVideoSink() => Task.CompletedTask;
    public Task StartVideoSink() => Task.CompletedTask;
    public Task PauseVideoSink() => Task.CompletedTask;
    public Task ResumeVideoSink() => Task.CompletedTask;

    public void RestrictFormats(Func<VideoFormat, bool> filter) => _formatManager.RestrictFormats(filter);
    public List<VideoFormat> GetVideoSinkFormats() => _formatManager.GetSourceFormats();
    public void SetVideoSinkFormat(VideoFormat videoFormat) => _formatManager.SetSelectedFormat(videoFormat);

    public void GotVideoRtp(IPEndPoint remoteEndPoint, uint ssrc, uint seqnum, uint timestamp, int payloadID,
        bool marker, byte[] payload) =>
        throw new ApplicationException(
            "This Video End Point requires full video frames rather than individual RTP packets.");

    public void GotVideoFrame(IPEndPoint remoteEndPoint, uint timestamp, byte[] frame, VideoFormat format)
    {
        if (OnVideoSinkDecodedSample != null)
        {
            try
            {
                foreach (var decoded in _videoDecoder.DecodeVideo(frame, VideoPixelFormatsEnum.Bgr, format.Codec))
                {
                    OnVideoSinkDecodedSample(decoded.Sample, decoded.Width, decoded.Height, (int)(decoded.Width * 3),
                        VideoPixelFormatsEnum.Bgr);
                }
            }
            catch (Exception excp)
            {
                Console.WriteLine($"Exception decoding video. {excp.Message}");
            }
        }
    }
}