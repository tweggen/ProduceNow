using SIPSorceryMedia.Abstractions;

namespace DemoContent;

public interface IVideoEndPoint : IDisposable
{
    public void ExternalVideoSourceRawSample(
        uint durationMilliseconds, 
        int width, int height, byte[] sample,
        VideoPixelFormatsEnum pixelFormat);
     
    public event EncodedSampleDelegate OnVideoSourceEncodedSample;

    public List<VideoFormat> GetVideoSourceFormats();

    public void SetVideoSourceFormat(VideoFormat videoFormat);

    public Task CloseVideo();
}