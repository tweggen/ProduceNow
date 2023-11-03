using FFmpeg.AutoGen;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;

namespace DemoContent;

public class DemoFFmpegEndPoint : IVideoEndPoint
{
    private readonly VideoFrameConverter _videoFrameConverter;
    private readonly FFmpegVideoEncoder _ffmpegEncoder;
    private long _presentationTimestamp = 0;

    private List<VideoFormat> _listFormats = new()
    {
        new VideoFormat(VideoCodecsEnum.VP8, 96)
    };
    
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint FramesPerSecond { get; set; }
    
    public event EncodedSampleDelegate? OnVideoSourceEncodedSample;

    public void ExternalVideoSourceRawSample(uint durationMilliseconds, int width, int height, byte[] sample,
        VideoPixelFormatsEnum pixelFormat)
    {
        if (width != Width || height != Height)
        {
            throw new ArgumentException("Width/Height do not fit.");
        }
        var i420Frame = _videoFrameConverter.Convert(sample);
        _presentationTimestamp += durationMilliseconds;
        i420Frame.pts = _presentationTimestamp;
        byte[] encodedBuffer = _ffmpegEncoder.Encode(AVCodecID.AV_CODEC_ID_VP8, i420Frame, (int)FramesPerSecond);
        if (encodedBuffer != null)
        {
            OnVideoSourceEncodedSample?.Invoke(
                90000U / (durationMilliseconds > 0U ? 1000U / durationMilliseconds : 30U), 
                encodedBuffer);
        }
    }
    
    
    public List<VideoFormat> GetVideoSourceFormats()
    {
        return _listFormats; 
    }
    

    public void SetVideoSourceFormat(VideoFormat videoFormat)
    {
        // TXWTODO: Check, if we need to implement this,
        return;
    }

    
    public Task CloseVideo()
    {
        // TXWTODO: Check, if we need to implement this,
        return Task.CompletedTask;
    }
    
    
    public void Dispose()
    {
        _ffmpegEncoder.Dispose();
        _videoFrameConverter.Dispose();
    }
    

    public DemoFFmpegEndPoint()
    {
        _ffmpegEncoder = new FFmpegVideoEncoder();
        _videoFrameConverter = new VideoFrameConverter(
            (int) Width, (int) Height,
            AVPixelFormat.AV_PIX_FMT_BGRA,
            (int) Width, (int) Height,
            AVPixelFormat.AV_PIX_FMT_YUV420P);
        
    }
}